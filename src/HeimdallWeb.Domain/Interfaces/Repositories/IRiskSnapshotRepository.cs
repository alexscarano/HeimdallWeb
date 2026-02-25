using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for RiskSnapshot aggregate.
/// </summary>
public interface IRiskSnapshotRepository
{
    /// <summary>
    /// Returns the most recent risk snapshot for a given monitored target,
    /// or null if no snapshots exist yet.
    /// </summary>
    Task<RiskSnapshot?> GetLatestByTargetIdAsync(int monitoredTargetId, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent snapshots for a given monitored target, newest first.
    /// </summary>
    /// <param name="monitoredTargetId">FK to the monitored target.</param>
    /// <param name="limit">Maximum number of snapshots to return. Default 30.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<RiskSnapshot>> GetByTargetIdAsync(int monitoredTargetId, int limit = 30, CancellationToken ct = default);

    /// <summary>Persists a new risk snapshot and returns the tracked entity.</summary>
    Task<RiskSnapshot> AddAsync(RiskSnapshot snapshot, CancellationToken ct = default);
}
