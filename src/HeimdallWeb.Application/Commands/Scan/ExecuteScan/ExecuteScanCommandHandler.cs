using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Services;
using HeimdallWeb.Application.Services.AI;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

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
    private readonly IConfiguration _configuration;
    private readonly int _maxDailyRequests;

    public ExecuteScanCommandHandler(
        IUnitOfWork unitOfWork,
        IScannerService scannerService,
        IGeminiService geminiService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
        _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _maxDailyRequests = 5; // Default daily quota for regular users
    }

    public async Task<ExecuteScanResponse> Handle(ExecuteScanCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command before executing
        var validator = new ExecuteScanCommandValidator();
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var normalizedTarget = NormalizeUrl(command.Target);

        try
        {
            // Step 1: Validate user exists and is active
            await ValidateUserAsync(command.UserId, cancellationToken);

            // Step 2: Check rate limiting (daily quota)
            await CheckRateLimitAsync(command.UserId, cancellationToken);

            // Step 3: Log scan initialization
            await LogScanInitializationAsync(command.UserId, normalizedTarget, command.RemoteIp, cancellationToken);

            // Step 4: Run security scanners with timeout (75 seconds)
            var (scanResultJson, aiSummary, aiResponseJson) = await RunScannersAndAnalyzeAsync(
                normalizedTarget,
                command.UserId,
                command.RemoteIp,
                cancellationToken);

            // Step 5: Save scan results to database within a transaction
            var historyId = await SaveScanResultsAsync(
                command.UserId,
                normalizedTarget,
                scanResultJson,
                aiSummary,
                aiResponseJson,
                stopwatch.Elapsed,
                command.RemoteIp,
                cancellationToken);

            stopwatch.Stop();

            // Step 6: Log successful completion
            await LogScanCompletionAsync(command.UserId, historyId, command.RemoteIp, cancellationToken);

            return new ExecuteScanResponse(
                HistoryId: historyId,
                Target: normalizedTarget,
                Summary: aiSummary,
                Duration: stopwatch.Elapsed,
                HasCompleted: true,
                CreatedDate: DateTime.UtcNow
            );
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();

            // Save incomplete scan record
            await SaveIncompleteScanAsync(
                command.UserId,
                normalizedTarget,
                stopwatch.Elapsed,
                command.RemoteIp,
                "Scan timeout or user cancellation",
                cancellationToken);

            await LogScanErrorAsync(command.UserId, command.RemoteIp, ex.Message, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                throw new Common.Exceptions.ApplicationException("Scan was cancelled by the user.");

            throw new TimeoutException("Scan took too long and was cancelled (max 75 seconds).");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Save incomplete scan record
            await SaveIncompleteScanAsync(
                command.UserId,
                normalizedTarget,
                stopwatch.Elapsed,
                command.RemoteIp,
                $"Error: {ex.Message}",
                cancellationToken);

            await LogScanErrorAsync(command.UserId, command.RemoteIp, ex.Message, cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Validates that the user exists and is active.
    /// Throws ApplicationException if user is blocked or not found.
    /// </summary>
    private async Task ValidateUserAsync(int userId, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, ct);
        
        if (user == null)
            throw new NotFoundException("User", userId);

        if (!user.IsActive)
            throw new Common.Exceptions.ApplicationException("Your account is blocked. Please contact the administrator.");
    }

    /// <summary>
    /// Checks if the user has exceeded their daily quota.
    /// Admins bypass this check.
    /// </summary>
    private async Task CheckRateLimitAsync(int userId, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, ct);
        var isAdmin = user?.UserType == UserType.Admin;

        if (isAdmin)
            return; // Admins have no quota limits

        var usage = await _unitOfWork.UserUsages.GetByUserAndDateAsync(userId, DateTime.UtcNow.Date, ct);
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
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // Run all scanners
            var scanResultJson = await _scannerService.RunAllScannersAsync(target, linkedCts.Token);

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
    /// </summary>
    private async Task<int> SaveScanResultsAsync(
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
                await _unitOfWork.SaveChangesAsync(cancellationToken); // Generate HistoryId

                var historyId = createdHistory.HistoryId;

                // Parse AI response to save findings and technologies
                await ParseAndSaveFindingsAsync(historyId, aiResponseJson, cancellationToken);
                await ParseAndSaveTechnologiesAsync(historyId, aiResponseJson, cancellationToken);
                await ParseAndSaveIASummaryAsync(historyId, aiResponseJson, cancellationToken);

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

                // Final save
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return historyId;
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
    /// Saves an incomplete scan record (due to timeout or error).
    /// </summary>
    private async Task SaveIncompleteScanAsync(
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

            await _unitOfWork.ScanHistories.AddAsync(scanHistory, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            // Ignore errors when saving incomplete scan records
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
            var achados = doc.RootElement.GetProperty("achados");

            var findings = new List<Finding>();
            foreach (var achado in achados.EnumerateArray())
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
                await _unitOfWork.Findings.AddRangeAsync(findings, ct);
        }
        catch
        {
            // If parsing fails, just skip findings (non-critical)
        }
    }

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
    private async Task LogScanInitializationAsync(int userId, string target, string remoteIp, CancellationToken ct)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog(
            code: LogEventCode.INIT_SCAN,
            level: "Info",
            message: "Scan initialization started",
            source: "ExecuteScanCommandHandler",
            details: $"Target: {target}",
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
