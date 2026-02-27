namespace HeimdallWeb.Application.Services.AI;

public interface IScanContextService
{
    Task<HistoricalDiffContext?> BuildHistoricalDiffAsync(
        string target, CancellationToken ct = default);
}
