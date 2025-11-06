using System.Diagnostics;
using System.Text.Json;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Scanners;
using HeimdallWeb.Services.IA;

namespace HeimdallWeb.Services;

public class ScanService : IScanService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository; 
    private readonly ITechnologyRepository _technologyRepository;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public ScanService(
        AppDbContext db, 
        IHistoryRepository historyRepository, 
        IFindingRepository findingRepository, 
        ITechnologyRepository technologyRepository,
        IConfiguration config
    )
    {
        _db = db;
        _historyRepository = historyRepository;
        _findingRepository = findingRepository;
        _technologyRepository = technologyRepository; 
        _config = config;
    }

    public async Task<int> RunScanAndPersist(string domainRaw, HistoryModel historyModel, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

        string domain = domainRaw.NormalizeUrl();

        // Run scanners
        var scanner = new ScannerManager();
        var scanTask = scanner.RunAllAsync(domain, linkedToken);

        try
        {
            var result = await scanTask; // will throw if canceled
            var jsonString = result.ToString();

            // Preprocess JSON
            JsonPreprocessor.PreProcessScanResults(ref jsonString);

            // Call IA
            var gemini = new GeminiService(_config);
            var iaResponse = await gemini.GeneratePrompt(jsonString);

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


                await tx.CommitAsync(cancellationToken);

                return historyId;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("O usuário cancelou a requisição.");
            throw new TimeoutException("O scan demorou muito tempo e foi cancelado.");
        }
    }
}
