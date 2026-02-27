using HeimdallWeb.Domain.Interfaces.Repositories;

namespace HeimdallWeb.Application.Services.AI;

public class ScanContextService : IScanContextService
{
    private readonly IScanHistoryRepository _scanHistoryRepo;

    public ScanContextService(IScanHistoryRepository scanHistoryRepo)
    {
        _scanHistoryRepo = scanHistoryRepo ?? throw new ArgumentNullException(nameof(scanHistoryRepo));
    }

    public async Task<HistoricalDiffContext?> BuildHistoricalDiffAsync(
        string target, CancellationToken ct = default)
    {
        var histories = await _scanHistoryRepo
            .GetLastNCompletedByTargetAsync(target, n: 3, ct);

        if (!histories.Any()) return null;

        var entries = histories
            .SelectMany(h => h.Findings)
            .GroupBy(f => f.Type)
            .Select(g => new CategoryHistoryEntry(
                Categoria: g.Key,
                Risco: g.OrderByDescending(f => (int)f.Severity).First().Severity.ToString(),
                PresenteHaScans: g.Select(f => f.HistoryId).Distinct().Count()
            ));

        return new HistoricalDiffContext(entries);
    }
}
