using System.Text.Json;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Services.IA;
using HeimdallWeb.Scanners;
using HeimdallWeb.Interfaces;
using System.Threading;

namespace HeimdallWeb.Services;

public class ScanService : IScanService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository; // restored original name
    private readonly ITechnologyRepository _technologyRepository;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public ScanService(
        IHistoryRepository historyRepository, 
        IFindingRepository findingRepository, 
        ITechnologyRepository technologyRepository,
        AppDbContext db, 
        IConfiguration config
    )
    {
        _historyRepository = historyRepository;
        _findingRepository = findingRepository;
        _technologyRepository = technologyRepository; // temporary, will correct below
        _db = db;
        _config = config;
    }

    public async Task<int> RunScanAndPersist(string domain, HistoryModel historyModel, CancellationToken cancellationToken = default)
    {
        if (NetworkUtils.IsIPAddress(domain))
            throw new ArgumentException("Por favor, insira um nome de domínio válido, não um endereço IP.");

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

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

            // Persist in a transaction
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
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
