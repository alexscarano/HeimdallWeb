using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Application.Services;
using HeimdallWeb.Application.Services.AI;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Commands.Scan.ExecuteScan;

/// <summary>
/// Handler for ExecuteScanCommand.
/// Orchestrates the entire scan process:
/// 1. Validates user authentication and active status
/// 2. Checks rate limiting (daily quota)
/// 3. Runs security scanners via ScannerManager
/// 4. Calls Gemini AI for vulnerability analysis
/// 5. Saves scan results with transaction management
/// 6. Handles timeouts (75s max) and errors gracefully
/// 
/// Extracted from ScanService.RunScanAndPersist (lines 48-265).
/// </summary>
public class ExecuteScanCommandHandler : ICommandHandler<ExecuteScanCommand, ExecuteScanResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScannerService _scannerService;
    private readonly IGeminiService _geminiService;
    private readonly IScoreCalculatorService _scoreCalculatorService;
    private readonly IConfiguration _configuration;
    private readonly IScanCacheService _scanCacheService;
    private readonly ILogger<ExecuteScanCommandHandler> _logger;
    private readonly int _maxDailyRequests;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public ExecuteScanCommandHandler(
        IUnitOfWork unitOfWork,
        IScannerService scannerService,
        IGeminiService geminiService,
        IScoreCalculatorService scoreCalculatorService,
        IConfiguration configuration,
        IScanCacheService scanCacheService,
        ILogger<ExecuteScanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
        _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
        _scoreCalculatorService = scoreCalculatorService ?? throw new ArgumentNullException(nameof(scoreCalculatorService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _scanCacheService = scanCacheService ?? throw new ArgumentNullException(nameof(scanCacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxDailyRequests = 5; // Default daily quota for regular users
    }

    public async Task<ExecuteScanResponse> Handle(ExecuteScanCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command before executing
        var validator = new ExecuteScanCommandValidator();
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var normalizedTarget = NormalizeUrl(command.Target);

        // Check scan cache — return immediately if a valid cached result exists (unless force refresh is requested)
        var cacheKey = _scanCacheService.GenerateCacheKey(normalizedTarget, command.ProfileId);

        if (!command.ForceRefresh)
        {
            var cachedJson = await _scanCacheService.GetCachedResultAsync(cacheKey, cancellationToken);
            if (cachedJson != null)
            {
                ExecuteScanResponse? cached = null;
                try
                {
                    cached = JsonSerializer.Deserialize<ExecuteScanResponse>(cachedJson);
                }
                catch (JsonException ex)
                {
                    // Malformed cache entry — proceed with a live scan
                    _logger.LogWarning(ex, "Malformed cache entry for key {Key}. Running live scan.", cacheKey);
                }

                if (cached != null)
                {
                    _logger.LogDebug("Scan cache hit for target {Target} (key: {Key}).", normalizedTarget, cacheKey);
                    stopwatch.Stop();

                    // These throw business exceptions (NotFoundException, ApplicationException) — must propagate
                    await ValidateUserAsync(command.UserId, cancellationToken);
                    await CheckRateLimitAsync(command.UserId, cancellationToken);

                    var user = await _unitOfWork.Users.GetByPublicIdAsync(command.UserId, cancellationToken);
                    var userInternalId = user!.UserId;

                    // History creation is non-critical — failure must not abort cache hit
                    Guid historyId;
                    try
                    {
                        historyId = await CreateCachedScanHistoryAsync(
                            userInternalId,
                            normalizedTarget,
                            cached.Summary,
                            cached.Score,
                            cached.Grade,
                            stopwatch.Elapsed,
                            command.RemoteIp,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create history for cache hit. Returning cached data without persisting history.");
                        historyId = Guid.Empty;
                    }

                    return cached with
                    {
                        IsCached = true,
                        Duration = stopwatch.Elapsed,
                        HistoryId = historyId
                    };
                }
            }
        }

        try
        {
            // Step 1: Validate user exists and is active
            await ValidateUserAsync(command.UserId, cancellationToken);

            // Step 2: Check rate limiting (daily quota)
            await CheckRateLimitAsync(command.UserId, cancellationToken);

            // Resolve user PublicId to get internal UserId for FK operations
            var user = await _unitOfWork.Users.GetByPublicIdAsync(command.UserId, cancellationToken);
            var userInternalId = user!.UserId; // Already validated in step 1

            // Step 2b: Validate profile (optional) — resolve name for audit log
            string? profileName = null;
            if (command.ProfileId.HasValue)
            {
                var profile = await _unitOfWork.ScanProfiles.GetByIdAsync(command.ProfileId.Value, cancellationToken);
                if (profile != null)
                    profileName = profile.Name;
                // Unknown profile ID is silently ignored — all scanners run with default settings
            }

            // Step 3: Log scan initialization
            await LogScanInitializationAsync(userInternalId, normalizedTarget, command.RemoteIp, cancellationToken, profileName);

            // Step 4: Run security scanners with timeout (75 seconds)
            var (scanResultJson, aiSummary, aiResponseJson) = await RunScannersAndAnalyzeAsync(
                normalizedTarget,
                userInternalId,
                command.RemoteIp,
                cancellationToken,
                command.EnabledScanners);

            // Step 5: Save scan results to database within a transaction
            var (historyPublicId, score, grade) = await SaveScanResultsAsync(
                userInternalId,
                normalizedTarget,
                scanResultJson,
                aiSummary,
                aiResponseJson,
                stopwatch.Elapsed,
                command.RemoteIp,
                cancellationToken);

            // Step 6: Log successful completion (need to resolve historyPublicId to internal ID for logging)
            var history = await _unitOfWork.ScanHistories.GetByPublicIdAsync(historyPublicId, cancellationToken);
            await LogScanCompletionAsync(userInternalId, history!.HistoryId, command.RemoteIp, cancellationToken);

            // Step 6b: Create scan-complete notification for the user
            var scanNotification = new Notification(
                userId: userInternalId,
                title: $"Scan concluído: {normalizedTarget}",
                body: $"Score: {score} ({grade})",
                type: Domain.Enums.NotificationType.ScanComplete);
            await _unitOfWork.Notifications.AddAsync(scanNotification, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            stopwatch.Stop();

            var response = new ExecuteScanResponse(
                HistoryId: historyPublicId,
                Target: normalizedTarget,
                Summary: aiSummary,
                Duration: stopwatch.Elapsed,
                HasCompleted: true,
                CreatedDate: DateTime.UtcNow,
                Score: score,
                Grade: grade,
                ProfileId: command.ProfileId,
                IsCached: false
            );

            // Step 7: Cache the successful scan result for subsequent identical requests
            try
            {
                var responseJson = JsonSerializer.Serialize(response);
                await _scanCacheService.CacheResultAsync(cacheKey, responseJson, CacheTtl, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Cache failures are non-critical — log and continue
                _logger.LogWarning(ex, "Failed to cache scan result for target {Target}.", normalizedTarget);
            }

            return response;
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();

            // Resolve user for logging in error cases
            var user = await _unitOfWork.Users.GetByPublicIdAsync(command.UserId, CancellationToken.None);
            var userInternalId = user?.UserId ?? 0;

            // Save incomplete scan record and get its PublicId
            var incompleteHistoryId = await SaveIncompleteScanAsync(
                userInternalId,
                normalizedTarget,
                stopwatch.Elapsed,
                command.RemoteIp,
                "Scan timeout or user cancellation",
                CancellationToken.None);

            await LogScanErrorAsync(userInternalId, command.RemoteIp, ex.Message, CancellationToken.None);

            // Return incomplete scan instead of throwing
            return new ExecuteScanResponse(
                HistoryId: incompleteHistoryId,
                Target: normalizedTarget,
                Summary: cancellationToken.IsCancellationRequested
                    ? "Scan was cancelled by the user."
                    : "Scan took too long and was cancelled (max 75 seconds).",
                Duration: stopwatch.Elapsed,
                HasCompleted: false,
                CreatedDate: DateTime.UtcNow,
                Score: null,
                Grade: null,
                ProfileId: command.ProfileId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Resolve user for logging in error cases
            var user = await _unitOfWork.Users.GetByPublicIdAsync(command.UserId, CancellationToken.None);
            var userInternalId = user?.UserId ?? 0;

            // Save incomplete scan record and get its PublicId
            // Log the full exception internally but return a sanitized message to the client
            var incompleteHistoryId = await SaveIncompleteScanAsync(
                userInternalId,
                normalizedTarget,
                stopwatch.Elapsed,
                command.RemoteIp,
                $"Error: {ex.Message}",
                CancellationToken.None);

            await LogScanErrorAsync(userInternalId, command.RemoteIp, ex.Message, CancellationToken.None);

            // Return incomplete scan with a generic message to prevent information leakage
            return new ExecuteScanResponse(
                HistoryId: incompleteHistoryId,
                Target: normalizedTarget,
                Summary: "An unexpected error occurred during the scan. Please try again later.",
                Duration: stopwatch.Elapsed,
                HasCompleted: false,
                CreatedDate: DateTime.UtcNow,
                Score: null,
                Grade: null,
                ProfileId: command.ProfileId
            );
        }
    }

    /// <summary>
    /// Validates that the user exists and is active.
    /// Throws ApplicationException if user is blocked or not found.
    /// </summary>
    private async Task ValidateUserAsync(Guid userPublicId, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByPublicIdAsync(userPublicId, ct);

        if (user == null)
            throw new NotFoundException("User", userPublicId);

        if (!user.IsActive)
            throw new Common.Exceptions.ApplicationException("Your account is blocked. Please contact the administrator.");
    }

    /// <summary>
    /// Checks if the user has exceeded their daily quota.
    /// Admins bypass this check.
    /// </summary>
    private async Task CheckRateLimitAsync(Guid userPublicId, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByPublicIdAsync(userPublicId, ct);
        var isAdmin = user?.UserType == UserType.Admin;

        if (isAdmin)
            return; // Admins have no quota limits

        var usage = await _unitOfWork.UserUsages.GetByUserAndDateAsync(user!.UserId, DateTime.UtcNow.Date, ct);
        var currentCount = usage?.RequestCounts ?? 0;

        if (currentCount >= _maxDailyRequests)
            throw new Common.Exceptions.ApplicationException($"Daily scan limit ({_maxDailyRequests}) has been reached.");
    }

    /// <summary>
    /// Runs all security scanners and calls Gemini AI for analysis.
    /// Applies a 75-second timeout to the entire operation.
    /// </summary>
    private async Task<(string scanResultJson, string aiSummary, string aiResponseJson)> RunScannersAndAnalyzeAsync(
        string target,
        int userId,
        string remoteIp,
        CancellationToken cancellationToken,
        IEnumerable<string>? enabledScanners = null)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // Run scanners (filtered if enabledScanners provided)
        var scanResultJson = await _scannerService.RunAllScannersAsync(target, linkedCts.Token, enabledScanners);

        // Preprocess JSON (normalize timestamps, headers, SSL results, etc.)
        PreProcessScanResults(ref scanResultJson);

        // Log AI request
        await LogAIRequestAsync(userId, remoteIp, CancellationToken.None);

        // Call Gemini AI for vulnerability analysis
        var aiResponse = await _geminiService.AnalyzeScanResultsAsync(scanResultJson, linkedCts.Token);

        // Sanitize AI response (may contain quotes/escapes from scan data)
        PreProcessScanResults(ref aiResponse);

        // Log AI response
        await LogAIResponseAsync(userId, remoteIp, CancellationToken.None);

        // Extract summary from AI response
        using var doc = JsonDocument.Parse(aiResponse);
        var summary = doc.RootElement.GetProperty("resumo").GetString() ?? "No summary available";

        return (scanResultJson, summary, aiResponse);
    }

    /// <summary>
    /// Saves all scan results to the database within a transaction.
    /// Includes ScanHistory, Findings, Technologies, IASummary, UserUsage, and AuditLog.
    /// Also calculates and persists the security score and grade.
    /// Returns the PublicId of the created history plus the computed Score and Grade.
    /// </summary>
    private async Task<(Guid PublicId, int Score, string Grade)> SaveScanResultsAsync(
        int userId,
        string target,
        string scanResultJson,
        string aiSummary,
        string aiResponseJson,
        TimeSpan duration,
        string remoteIp,
        CancellationToken ct)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
            {
                // Create ScanHistory entity
                var scanTarget = ScanTarget.Create(target);
                var scanHistory = new ScanHistory(scanTarget, userId);
                scanHistory.CompleteScan(duration, scanResultJson, aiSummary);

                // Add to repository
                var createdHistory = await _unitOfWork.ScanHistories.AddAsync(scanHistory, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken); // Generate HistoryId and PublicId

                var historyId = createdHistory.HistoryId;

                // Parse AI response to save findings and technologies
                await ParseAndSaveFindingsAsync(historyId, aiResponseJson, cancellationToken);
                await ParseAndSaveTechnologiesAsync(historyId, aiResponseJson, cancellationToken);
                await ParseAndSaveIASummaryAsync(historyId, aiResponseJson, cancellationToken);

                // Supplement: extract findings directly from raw scanner results as fallback
                // This ensures scoring even if the AI misses some vulnerabilities
                await ExtractFindingsFromRawScanAsync(historyId, scanResultJson, cancellationToken);

                // Calculate security score from ALL persisted findings (AI + raw scanner)
                var findings = await _unitOfWork.Findings.GetByHistoryIdAsync(historyId, cancellationToken);
                var (score, grade) = await _scoreCalculatorService.CalculateAsync(findings, cancellationToken);
                createdHistory.SetScore(score, grade);

                // Update UserUsage (increment request count)
                await UpdateUserUsageAsync(userId, cancellationToken);

                // Log success
                await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
                    code: LogEventCode.DB_SAVE_OK,
                    level: "Info",
                    message: "Scan results saved successfully",
                    source: "ExecuteScanCommandHandler",
                    userId: userId,
                    historyId: historyId,
                    remoteIp: remoteIp
                ), cancellationToken);

                // Final save (persists score/grade on the history record)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return (createdHistory.PublicId, score, grade);
            }, ct);
        }
        catch (Exception ex)
        {
            // Log error outside transaction
            await LogDatabaseErrorAsync(userId, remoteIp, ex.Message, CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// Creates a ScanHistory record for a cache hit, reusing the cached summary and score.
    /// Returns the PublicId of the newly created history.
    /// </summary>
    private async Task<Guid> CreateCachedScanHistoryAsync(
        int userId,
        string target,
        string summary,
        int? score,
        string? grade,
        TimeSpan duration,
        string remoteIp,
        CancellationToken ct)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async (cancellationToken) =>
            {
                var scanTarget = ScanTarget.Create(target);
                var scanHistory = new ScanHistory(scanTarget, userId);

                // Use the actual elapsed duration (cache lookup time) so ScanDuration validation passes.
                // The actual detailed results come from the cache JSON returned to the client.
                scanHistory.CompleteScan(duration, "{\"cached\": true}", summary);

                // Link to the original scan so the detail page can resolve findings/rawJson from it
                var sourceHistory = await _unitOfWork.ScanHistories.GetLatestCompletedByTargetAsync(
                    scanTarget.Value, cancellationToken);
                if (sourceHistory != null)
                    scanHistory.SetSourceHistory(sourceHistory.HistoryId);

                if (score.HasValue && !string.IsNullOrEmpty(grade))
                {
                    scanHistory.SetScore(score.Value, grade);
                }

                var createdHistory = await _unitOfWork.ScanHistories.AddAsync(scanHistory, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Update UserUsage (increment request count)
                await UpdateUserUsageAsync(userId, cancellationToken);

                // Log success for cache hit
                await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
                    code: LogEventCode.DB_SAVE_OK,
                    level: "Info",
                    message: "Scan history created from cache hit",
                    source: "ExecuteScanCommandHandler",
                    userId: userId,
                    historyId: createdHistory.HistoryId,
                    remoteIp: remoteIp
                ), cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return createdHistory.PublicId;
            }, ct);
        }
        catch (Exception ex)
        {
            await LogDatabaseErrorAsync(userId, remoteIp, ex.Message, CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// Saves an incomplete scan record (due to timeout or error).
    /// Returns the PublicId of the created history, or Guid.Empty if save failed.
    /// </summary>
    private async Task<Guid> SaveIncompleteScanAsync(
        int userId,
        string target,
        TimeSpan duration,
        string remoteIp,
        string summary,
        CancellationToken ct)
    {
        try
        {
            var scanTarget = ScanTarget.Create(target);
            var scanHistory = new ScanHistory(scanTarget, userId);
            scanHistory.MarkAsIncomplete(summary);

            var createdHistory = await _unitOfWork.ScanHistories.AddAsync(scanHistory, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return createdHistory.PublicId;
        }
        catch
        {
            // If save fails, return empty Guid (caller should handle appropriately)
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Updates or creates UserUsage record for daily quota tracking.
    /// </summary>
    private async Task UpdateUserUsageAsync(int userId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var usage = await _unitOfWork.UserUsages.GetByUserAndDateAsync(userId, today, ct);

        if (usage == null)
        {
            usage = new UserUsage(userId, today);
            usage.IncrementRequests();
            await _unitOfWork.UserUsages.AddAsync(usage, ct);
        }
        else
        {
            usage.IncrementRequests();
        }
    }

    /// <summary>
    /// Parses AI response JSON and saves findings to database.
    /// </summary>
    private async Task ParseAndSaveFindingsAsync(int historyId, string aiResponseJson, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(aiResponseJson);

            // Check if "achados" property exists
            if (!doc.RootElement.TryGetProperty("achados", out var achadosElement))
            {
                // Log warning: AI response doesn't contain findings array
                await LogParsingWarningAsync(
                    historyId,
                    "AI response missing 'achados' property - findings not saved",
                    ct);
                return;
            }

            var findings = new List<Finding>();
            foreach (var achado in achadosElement.EnumerateArray())
            {
                var descricao = achado.GetProperty("descricao").GetString() ?? "";
                var categoria = achado.GetProperty("categoria").GetString() ?? "";
                var risco = achado.GetProperty("risco").GetString() ?? "";
                var evidencia = achado.GetProperty("evidencia").GetString() ?? "";
                var recomendacao = achado.GetProperty("recomendacao").GetString() ?? "";

                var finding = new Finding(
                    type: categoria,
                    description: descricao,
                    severity: ParseSeverity(risco),
                    evidence: evidencia,
                    recommendation: recomendacao,
                    historyId: historyId
                );

                findings.Add(finding);
            }

            if (findings.Any())
            {
                await _unitOfWork.Findings.AddRangeAsync(findings, ct);

                // Log success
                await LogParsingSuccessAsync(
                    historyId,
                    $"Successfully parsed and saved {findings.Count} findings",
                    ct);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the scan
            await LogParsingErrorAsync(historyId, ex.Message, ct);
        }
    }

    /// <summary>
    /// Extracts findings directly from raw scanner JSON results as a supplementary source.
    /// This ensures that scanner-detected vulnerabilities (with severity fields) are always
    /// counted towards the score, even if the AI fails to generate corresponding achados.
    /// Only adds findings that are not duplicated by AI-generated ones.
    /// </summary>
    private async Task ExtractFindingsFromRawScanAsync(int historyId, string scanResultJson, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(scanResultJson);
            var root = doc.RootElement;
            var rawFindings = new List<Finding>();

            // 1. Extract from PortScanner results (resultsPortScanner array)
            if (root.TryGetProperty("resultsPortScanner", out var portResults) &&
                portResults.ValueKind == JsonValueKind.Array)
            {
                foreach (var portEntry in portResults.EnumerateArray())
                {
                    var isOpen = portEntry.TryGetProperty("open", out var openProp) && openProp.GetBoolean();
                    if (!isOpen) continue;

                    var severity = portEntry.TryGetProperty("severity", out var sevProp)
                        ? sevProp.GetString() ?? ""
                        : "";
                    var description = portEntry.TryGetProperty("description", out var descProp)
                        ? descProp.GetString() ?? ""
                        : "";
                    var port = portEntry.TryGetProperty("port", out var portProp)
                        ? portProp.GetInt32()
                        : 0;

                    // Only add findings with actual security severity (skip Informativo)
                    var parsedSeverity = ParseSeverity(severity);
                    if (parsedSeverity == SeverityLevel.Informational) continue;

                    var banner = portEntry.TryGetProperty("banner", out var bannerProp)
                        ? bannerProp.GetString() ?? ""
                        : "";
                    var evidence = !string.IsNullOrEmpty(banner)
                        ? $"Porta {port} aberta. Banner: {banner}"
                        : $"Porta {port} aberta";

                    rawFindings.Add(new Finding(
                        type: "Portas Abertas",
                        description: description,
                        severity: parsedSeverity,
                        evidence: evidence,
                        recommendation: GetPortRecommendation(port),
                        historyId: historyId
                    ));
                }
            }

            // 2. Extract from SecurityHeaders scanner (securityHeaders.missing array)
            if (root.TryGetProperty("securityHeaders", out var secHeaders))
            {
                if (secHeaders.TryGetProperty("missing", out var missingHeaders) &&
                    missingHeaders.ValueKind == JsonValueKind.Array)
                {
                    foreach (var header in missingHeaders.EnumerateArray())
                    {
                        var headerName = header.GetString() ?? "";
                        if (string.IsNullOrEmpty(headerName)) continue;

                        // Critical headers get higher severity
                        var headerSeverity = headerName switch
                        {
                            "Strict-Transport-Security" => SeverityLevel.High,
                            "Content-Security-Policy" => SeverityLevel.Medium,
                            "X-Frame-Options" => SeverityLevel.Medium,
                            "X-Content-Type-Options" => SeverityLevel.Low,
                            _ => SeverityLevel.Low
                        };

                        rawFindings.Add(new Finding(
                            type: "Headers de Segurança",
                            description: $"Header de segurança ausente: {headerName}",
                            severity: headerSeverity,
                            evidence: $"Header {headerName} não presente na resposta HTTP",
                            recommendation: $"Adicionar o header {headerName} para melhorar a segurança.",
                            historyId: historyId
                        ));
                    }
                }
            }

            // Only save non-duplicate findings
            if (rawFindings.Any())
            {
                // Check existing findings to avoid duplicates
                var existingFindings = await _unitOfWork.Findings.GetByHistoryIdAsync(historyId, ct);
                var existingDescriptions = new HashSet<string>(
                    existingFindings.Select(f => f.Description),
                    StringComparer.OrdinalIgnoreCase);

                var newFindings = rawFindings
                    .Where(f => !existingDescriptions.Contains(f.Description))
                    .ToList();

                if (newFindings.Any())
                {
                    await _unitOfWork.Findings.AddRangeAsync(newFindings, ct);
                }
            }
        }
        catch
        {
            // Non-critical: silently skip if raw parsing fails
        }
    }

    private static string GetPortRecommendation(int port) => port switch
    {
        3306 or 5432 or 27017 or 1433 or 1521 =>
            "Remover o acesso público ao banco de dados. Use firewalls e VPN para restringir o acesso.",
        3389 =>
            "Remover acesso público ao RDP. Use VPN ou bastion host.",
        6379 or 11211 =>
            "Remover acesso público ao serviço de cache. Configure autenticação e firewall.",
        22 =>
            "Utilize autenticação por chave SSH e desative login por senha.",
        21 or 20 =>
            "Desative FTP e utilize SFTP sobre SSH para transferências seguras.",
        23 =>
            "Desative Telnet imediatamente. Utilize SSH como alternativa segura.",
        _ =>
            "Avalie se esta porta precisa estar exposta publicamente e configure firewall adequadamente."
    };

    /// <summary>
    /// Parses AI response JSON and saves technologies to database.
    /// </summary>
    private async Task ParseAndSaveTechnologiesAsync(int historyId, string aiResponseJson, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(aiResponseJson);
            var tecnologias = doc.RootElement.GetProperty("tecnologias");

            var technologies = new List<Technology>();
            foreach (var tech in tecnologias.EnumerateArray())
            {
                var nome = tech.GetProperty("nome_tecnologia").GetString() ?? "";
                var versao = tech.GetProperty("versao").GetString();
                var categoria = tech.GetProperty("categoria_tecnologia").GetString() ?? "";
                var descricao = tech.GetProperty("descricao_tecnologia").GetString() ?? "";

                var technology = new Technology(
                    name: nome,
                    category: categoria,
                    description: descricao,
                    version: versao,
                    historyId: historyId
                );

                technologies.Add(technology);
            }

            if (technologies.Any())
                await _unitOfWork.Technologies.AddRangeAsync(technologies, ct);
        }
        catch
        {
            // If parsing fails, just skip technologies (non-critical)
        }
    }

    /// <summary>
    /// Parses AI response JSON and saves IA summary to database.
    /// </summary>
    private async Task ParseAndSaveIASummaryAsync(int historyId, string aiResponseJson, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(aiResponseJson);

            // Count findings by severity
            var achados = doc.RootElement.GetProperty("achados");
            int criticos = 0, altos = 0, medios = 0, baixos = 0, informativos = 0;

            foreach (var achado in achados.EnumerateArray())
            {
                var risco = achado.GetProperty("risco").GetString() ?? "";
                switch (risco.ToLowerInvariant())
                {
                    case "critico": criticos++; break;
                    case "alto": altos++; break;
                    case "medio": medios++; break;
                    case "baixo": baixos++; break;
                    case "informativo": informativos++; break;
                }
            }

            var overallRisk = DetermineOverallRisk(criticos, altos, medios);
            var resumo = doc.RootElement.GetProperty("resumo").GetString() ?? "";

            var totalFindings = criticos + altos + medios + baixos + informativos;
            var iaSummary = new IASummary(
                summaryText: resumo,
                mainCategory: "Security Scan",
                overallRisk: overallRisk,
                totalFindings: totalFindings,
                findingsCritical: criticos,
                findingsHigh: altos,
                findingsMedium: medios,
                findingsLow: baixos,
                historyId: historyId
            );

            await _unitOfWork.IASummaries.AddAsync(iaSummary, ct);
        }
        catch
        {
            // If parsing fails, just skip IA summary (non-critical)
        }
    }

    private SeverityLevel ParseSeverity(string risco)
    {
        return risco.ToLowerInvariant() switch
        {
            "critico" => SeverityLevel.Critical,
            "alto" => SeverityLevel.High,
            "medio" => SeverityLevel.Medium,
            "baixo" => SeverityLevel.Low,
            "informativo" => SeverityLevel.Informational,
            _ => SeverityLevel.Informational
        };
    }

    private string DetermineOverallRisk(int criticos, int altos, int medios)
    {
        if (criticos > 0) return "Critical";
        if (altos > 0) return "High";
        if (medios > 0) return "Medium";
        return "Low";
    }

    // Logging helpers
    private async Task LogScanInitializationAsync(
        int userId,
        string target,
        string remoteIp,
        CancellationToken ct,
        string? profileName = null)
    {
        var details = profileName != null
            ? $"Target: {target} | Profile: {profileName}"
            : $"Target: {target}";

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.INIT_SCAN,
            level: "Info",
            message: "Scan initialization started",
            source: "ExecuteScanCommandHandler",
            details: details,
            userId: userId,
            remoteIp: remoteIp
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogAIRequestAsync(int userId, string remoteIp, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.AI_REQUEST,
            level: "Info",
            message: "AI analysis request sent",
            source: "ExecuteScanCommandHandler",
            userId: userId,
            remoteIp: remoteIp
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogAIResponseAsync(int userId, string remoteIp, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.AI_RESPONSE,
            level: "Info",
            message: "AI analysis response received",
            source: "ExecuteScanCommandHandler",
            userId: userId,
            remoteIp: remoteIp
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogScanCompletionAsync(int userId, int historyId, string remoteIp, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.SCAN_COMPLETED,
            level: "Info",
            message: "Scan completed successfully",
            source: "ExecuteScanCommandHandler",
            userId: userId,
            historyId: historyId,
            remoteIp: remoteIp
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogScanErrorAsync(int userId, string remoteIp, string errorDetails, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.SCAN_ERROR,
            level: "Error",
            message: "Scan error occurred",
            source: "ExecuteScanCommandHandler",
            details: errorDetails,
            userId: userId,
            remoteIp: remoteIp
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogDatabaseErrorAsync(int userId, string remoteIp, string errorDetails, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.DB_SAVE_ERROR,
            level: "Error",
            message: "Database save error",
            source: "ExecuteScanCommandHandler",
            details: errorDetails,
            userId: userId,
            remoteIp: remoteIp
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogParsingWarningAsync(int historyId, string details, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.SCAN_COMPLETED,
            level: "Warning",
            message: "AI findings parsing warning",
            source: "ExecuteScanCommandHandler.ParseAndSaveFindingsAsync",
            details: details,
            historyId: historyId
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogParsingSuccessAsync(int historyId, string details, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.SCAN_COMPLETED,
            level: "Info",
            message: "Findings parsed successfully",
            source: "ExecuteScanCommandHandler.ParseAndSaveFindingsAsync",
            details: details,
            historyId: historyId
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogParsingErrorAsync(int historyId, string errorMessage, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.SCAN_ERROR,
            level: "Error",
            message: "Findings parsing failed",
            source: "ExecuteScanCommandHandler.ParseAndSaveFindingsAsync",
            details: $"Error: {errorMessage}",
            historyId: historyId
        ), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Normalizes URL by adding https:// if missing and validating format.
    /// </summary>
    private string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty");

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = $"https://{url}";
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL format");
        }

        return uriResult.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    /// <summary>
    /// Preprocesses scan results JSON (normalizes timestamps, headers, SSL results, ports, redirects).
    /// Removes null bytes and control characters that break PostgreSQL JSONB.
    /// </summary>
    private void PreProcessScanResults(ref string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            jsonString = "{}";
            return;
        }

        // STEP 1: Remove null bytes in ALL forms
        // - Real null bytes: \0 (char 0)
        // - Unicode escape in C#: \u0000
        // - JSON escape sequence: \\u0000 (literal string in JSON)
        jsonString = jsonString
            .Replace("\u0000", "")       // Real null byte (char 0)
            .Replace("\0", "")           // Alternative null byte notation
            .Replace("\\u0000", "");     // Escaped null in JSON strings

        // STEP 2: Remove other problematic escape sequences
        // PostgreSQL JSONB doesn't accept \u0001 through \u001F (except \t \n \r)
        for (int i = 1; i <= 31; i++)
        {
            if (i == 9 || i == 10 || i == 13) continue; // Skip \t \n \r

            // Remove both real control chars and JSON escape sequences
            var escapeSeq = $"\\u{i:x4}";
            jsonString = jsonString.Replace(escapeSeq, "");
        }

        // STEP 3: Remove actual control characters from the string
        var cleaned = new System.Text.StringBuilder(jsonString.Length);
        foreach (char c in jsonString)
        {
            // Allow: printable chars (≥32), tabs (9), newlines (10), carriage returns (13)
            if (c >= 32 || c == '\t' || c == '\n' || c == '\r')
            {
                cleaned.Append(c);
            }
            // Skip: other control characters (0-8, 11-12, 14-31)
        }

        jsonString = cleaned.ToString();
    }
}
