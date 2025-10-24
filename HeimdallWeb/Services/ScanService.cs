using System.Text.Json;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Services.IA;
using HeimdallWeb.Repository.Interfaces;
using HeimdallWeb.Scanners;
using HeimdallWeb.Services.Interfaces;

namespace HeimdallWeb.Services;

public class ScanService : IScanService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public ScanService(IHistoryRepository historyRepository, IFindingRepository findingRepository, AppDbContext db, IConfiguration config)
    {
        _historyRepository = historyRepository;
        _findingRepository = findingRepository;
        _db = db;
        _config = config;
    }

    public async Task<int> RunScanAndPersistAsync(string domain, HistoryModel historyModel, CancellationToken cancellationToken = default)
    {
        if (NetworkUtils.IsIPAddress(domain))
            throw new ArgumentException("Por favor, insira um nome de domínio válido, não um endereço IP.");

        // Run scanners
        var scanner = new ScannerManager();
        var scanTask = scanner.RunAllAsync(domain);
        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        var completed = await Task.WhenAny(scanTask, timeoutTask);

        if (completed == timeoutTask)
            throw new TimeoutException("O scan demorou muito tempo e foi cancelado.");

        var result = await scanTask;
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
        historyModel.created_date = DateTime.UtcNow;

        // Persist in a transaction
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var createdHistory = await _historyRepository.insertHistory(historyModel);
            var historyId = createdHistory.history_id;

            await _findingRepository.SaveFindingsFromIA(iaResponse, historyId);

            await tx.CommitAsync(cancellationToken);

            return historyId;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
