using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Domain.ValueObjects;

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
        // normalizedTarget from handler includes protocol (e.g. "https://exemplo.com"),
        // but ScanTarget.Value stored in DB is domain-only (e.g. "exemplo.com").
        // Use ScanTarget.Create to apply the same normalization used at write time.
        var storedTarget = ScanTarget.Create(target).Value;

        var histories = await _scanHistoryRepo
            .GetLastNCompletedByTargetAsync(storedTarget, n: 3, ct);

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
