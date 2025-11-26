using System.Diagnostics;
using System.Text.Json;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;
using HeimdallWeb.Scanners;
using HeimdallWeb.Services.IA;
using ASHelpers.Extensions;

namespace HeimdallWeb.Services;

public class ScanService : IScanService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository; 
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ILogRepository _logRepository;
    private readonly IUserUsageRepository _userUsageRepository;
    private readonly IIASummaryRepository _iaSummaryRepository;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly int _maxRequests;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ScanService(
        IHistoryRepository historyRepository, 
        IFindingRepository findingRepository, 
        ITechnologyRepository technologyRepository, 
        ILogRepository logRepository, 
        IUserUsageRepository userUsageRepository, 
        IIASummaryRepository iaSummaryRepository,
        AppDbContext db, IConfiguration config,
        IHttpContextAccessor httpContextAccessor)
    {
        _historyRepository = historyRepository;
        _findingRepository = findingRepository;
        _technologyRepository = technologyRepository;
        _logRepository = logRepository;
        _userUsageRepository = userUsageRepository;
        _iaSummaryRepository = iaSummaryRepository;
        _db = db;
        _config = config;
        _maxRequests = 5;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> RunScanAndPersist(string domainRaw, HistoryModel historyModel, CancellationToken cancellationToken = default)
    {
        int currentUserId = 0;
        bool isUserAdmin = false;
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cookie = CookiesHelper.getAuthCookie(httpContext.Request);
                currentUserId = CookiesHelper.getUserIDFromCookie(cookie);
                if (currentUserId > 0)
                {
                    historyModel.user_id = currentUserId;

                    // Verificar se usuário está bloqueado
                    var user = await _db.User.FirstOrDefaultAsync(u => u.user_id == currentUserId);
                    if (user is null && !user.is_active)
                    {
                        throw new Exception("Sua conta está bloqueada. Entre em contato com o administrador.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("bloqueada"))
                throw;
            throw new Exception("Não foi possível identificar o usuário atual.");
        }

        (var user_usage_count, var user_usage, isUserAdmin) =
            await _userUsageRepository.GetUserUsageCount(currentUserId, DateTime.Now.Date);

        if (!isUserAdmin && user_usage_count > _maxRequests)
        {
            throw new Exception($"O limite diário de requisições ({_maxRequests}) foi atingido");
        }

        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

        string domain = domainRaw.NormalizeUrl();

        await _logRepository.AddLogImmediate(new LogModel
        {
            code = LogEventCode.INIT_SCAN,
            message = "Iniciando processo de varredura",
            source = "ScanService",
            user_id = currentUserId,
            details = $"Target: {domain}",
            remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
        });

        // Run scanners
        var scanner = new ScannerManager();
        var scanTask = scanner.RunAllAsync(domain, linkedToken);

        try
        {
            var result = await scanTask; // will throw if canceled
            var jsonString = result.ToString();

            // Preprocess JSON
            JsonPreprocessor.PreProcessScanResults(ref jsonString);

            await _logRepository.AddLogImmediate(new LogModel
            {
                code = LogEventCode.AI_REQUEST,
                message = "Enviando requisição à IA",
                source = "ScanService",
                user_id = currentUserId,
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
            });

            // Call IA
            var gemini = new GeminiService(_config, _logRepository, _httpContextAccessor);
            var iaResponse = await gemini.GeneratePrompt(jsonString);

            await _logRepository.AddLogImmediate(new LogModel
            {
                code = LogEventCode.AI_RESPONSE,
                message = "Resposta da IA recebida com sucesso",
                source = "ScanService",
                user_id = currentUserId,
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
            });

            using var doc = JsonDocument.Parse(iaResponse);

            historyModel.target = domain;
            historyModel.raw_json_result = jsonString;
            historyModel.summary = doc.RootElement.GetProperty("resumo").GetString();
            historyModel.created_date = DateTime.Now;

            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                stopwatch.Stop();
                historyModel.duration = stopwatch.Elapsed;
                historyModel.has_completed = true;
                
                // Adicionar history ao contexto (sem persistir ainda)
                var createdHistory = await _historyRepository.insertHistory(historyModel);
                // Força SaveChanges para gerar o history_id (necessário para foreign keys)
                await _db.SaveChangesAsync(cancellationToken);
                var historyId = createdHistory.history_id;
 
                // Adicionar findings, technologies, summary e usage ao contexto
                await _findingRepository.SaveFindingsFromAI(iaResponse, historyId);
                await _technologyRepository.SaveTechnologiesFromAI(iaResponse, historyId);
                await _iaSummaryRepository.SaveIASummaryFromFindings(historyId, iaResponse);

                await _userUsageRepository.AddUserUsage(new UserUsageModel
                {
                    user_id = currentUserId,
                    date = DateTime.Now,
                    request_counts = user_usage.request_counts >= 0 ? user_usage.request_counts + 1 : 0
                });
                
                // Adicionar log de sucesso ao contexto
                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_OK,
                    message = "Registro salvo com sucesso",
                    source = "ScanService",
                    user_id = currentUserId,
                    history_id = historyId,
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
                });

                // Persistir todas as mudanças de uma vez
                await _db.SaveChangesAsync(cancellationToken);
                
                // Commitar a transação
                await tx.CommitAsync(cancellationToken);

                return historyId;
            }
            catch (Exception dbEx)
            {
                await tx.RollbackAsync(cancellationToken);
                
                // Log de erro fora da transação
                await _logRepository.AddLogImmediate(new LogModel
                {
                    code = LogEventCode.DB_SAVE_ERROR,
                    message = "Erro ao salvar dados no banco",
                    source = "ScanService",
                    user_id = currentUserId,
                    details = dbEx.ToStringNullable(),
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
                });
                
                throw;
            }
        }
        catch (OperationCanceledException ex)
        {
            // Salvar histórico com has_completed = 0
            stopwatch.Stop();
            historyModel.target = domain;
            historyModel.duration = stopwatch.Elapsed;
            historyModel.has_completed = false;
            historyModel.created_date = DateTime.Now;
            
            try
            {
                await _historyRepository.insertHistory(historyModel);
                await _db.SaveChangesAsync(); // Persistir fora da transação principal
            }
            catch { /* Ignore save errors for failed scans */ }

            await _logRepository.AddLogImmediate(new LogModel
            {
                code = LogEventCode.SCAN_ERROR,
                message = "Erro durante o processo de scan",
                source = "ScanService",
                user_id = currentUserId,
                details = ex.ToStringNullable(),
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
            });
            
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("O usuário cancelou a requisição.");
            throw new TimeoutException("O scan demorou muito tempo e foi cancelado.");
        }
        catch (Exception ex)
        {
            // Salvar histórico com has_completed = 0 para qualquer outro erro
            stopwatch.Stop();
            historyModel.target = domain;
            historyModel.duration = stopwatch.Elapsed;
            historyModel.has_completed = false;
            historyModel.created_date = DateTime.Now;
            
            try
            {
                await _historyRepository.insertHistory(historyModel);
                await _db.SaveChangesAsync(); // Persistir fora da transação principal
            }
            catch { /* Ignore save errors for failed scans */ }

            await _logRepository.AddLogImmediate(new LogModel
            {
                code = LogEventCode.SCAN_ERROR,
                message = "Erro durante o processo de scan",
                source = "ScanService",
                user_id = currentUserId,
                details = ex.ToStringNullable(),
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
            });
            
            throw;
        }
    }
}
