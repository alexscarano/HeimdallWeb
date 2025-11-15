using System.Diagnostics;
using System.Text.Json;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;
using HeimdallWeb.Scanners;
using HeimdallWeb.Services.IA;

namespace HeimdallWeb.Services;

public class ScanService : IScanService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository; 
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ILogRepository _logRepository;
    private readonly IUserUsageRepository _userUsageRepository;
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
        AppDbContext db, IConfiguration config,
        IHttpContextAccessor httpContextAccessor)
    {
        _historyRepository = historyRepository;
        _findingRepository = findingRepository;
        _technologyRepository = technologyRepository;
        _logRepository = logRepository;
        _userUsageRepository = userUsageRepository;
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
                }
            }
        }
        catch
        {
            throw new Exception("N�o foi poss�vel identificar o usu�rio atual.");
        }

        (var user_usage_count, var user_usage, isUserAdmin) =
            await _userUsageRepository.GetUserUsageCount(currentUserId, DateTime.Now.Date);

        if (!isUserAdmin && user_usage_count > _maxRequests)
        {
            throw new Exception($"O limite di�rio de requisi��es ({_maxRequests}) foi atingido");
        }

        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

        string domain = domainRaw.NormalizeUrl();

        await _logRepository.AddLog(new LogModel
        {
            code = LogEventCode.INIT_SCAN,
            message = "Iniciando processo de varredura",
            source = "ScanService",
            user_id = currentUserId,
            details = $"Target: {domain}"
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

            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.AI_REQUEST,
                message = "Enviando requisição à IA",
                source = "ScanService",
                user_id = currentUserId
            });

            // Call IA
            var gemini = new GeminiService(_config, _logRepository);
            var iaResponse = await gemini.GeneratePrompt(jsonString);

            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.AI_RESPONSE,
                message = "Resposta da IA recebida com sucesso",
                source = "ScanService",
                user_id = currentUserId
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
                var createdHistory = await _historyRepository.insertHistory(historyModel);
                var historyId = createdHistory.history_id;

 
                await _findingRepository.SaveFindingsFromAI(iaResponse, historyId);
                await _technologyRepository.SaveTechnologiesFromAI(iaResponse, historyId);


                await _userUsageRepository.AddUserUsage(new UserUsageModel
                {
                    user_id = currentUserId,
                    date = DateTime.Now,
                    request_counts = user_usage.request_counts >= 0 
                                ? user_usage.request_counts + 1 : 0
                });
                
                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_OK,
                    message = "Registro salvo com sucesso",
                    source = "ScanService",
                    user_id = currentUserId,
                    history_id = historyId
                });

                await tx.CommitAsync(cancellationToken);

                return historyId;
            }
            catch (Exception dbEx)
            {
                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_ERROR,
                    message = "Erro ao salvar dados no banco",
                    source = "ScanService",
                    user_id = currentUserId,
                    details = dbEx.ToString()
                });
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (OperationCanceledException ex)
        {
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.SCAN_ERROR,
                message = "Erro durante o processo de scan",
                source = "ScanService",
                user_id = currentUserId,
                details = ex.ToString()
            });
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("O usuário cancelou a requisição.");
            throw new TimeoutException("O scan demorou muito tempo e foi cancelado.");
        }
    }
}
