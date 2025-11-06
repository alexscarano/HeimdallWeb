using System.Text.Json;
using HeimdallWeb.Models;
using HeimdallWeb.Services.IA;
using HeimdallWeb.Scanners;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Helpers;

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

            historyModel.Target = domain;
            historyModel.RawJsonResult = jsonString;
            historyModel.Summary = doc.RootElement.GetProperty("resumo").GetString();
            historyModel.CreatedAt = DateTime.Now;


            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var createdHistory = await _historyRepository.insertHistory(historyModel);
                var historyId = createdHistory.HistoryId;

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
