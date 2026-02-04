using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for IASummary entity operations.
/// </summary>
public interface IIASummaryRepository
{
    /// <summary>
    /// Gets the AI summary for a specific scan history.
    /// </summary>
    Task<IASummary?> GetByHistoryIdAsync(int historyId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new AI summary.
    /// </summary>
    Task<IASummary> AddAsync(IASummary summary, CancellationToken ct = default);
}
