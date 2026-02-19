using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ScanCache aggregate.
/// </summary>
public interface IScanCacheRepository
{
    /// <summary>
    /// Returns a valid (non-expired) cache entry for the given key,
    /// or null if no entry exists or the entry has expired.
    /// </summary>
    Task<ScanCache?> GetValidCacheAsync(string cacheKey, CancellationToken ct = default);

    /// <summary>Persists a new scan cache entry and returns the tracked entity.</summary>
    Task<ScanCache> AddAsync(ScanCache cache, CancellationToken ct = default);

    /// <summary>
    /// Batch-deletes all expired cache entries.
    /// Uses <c>ExecuteDeleteAsync</c> for a single-round-trip DELETE statement.
    /// </summary>
    Task DeleteExpiredAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a cache entry by its key regardless of expiry state,
    /// or null if no entry exists. Used to check for an existing entry before upsert.
    /// </summary>
    Task<ScanCache?> GetByCacheKeyAsync(string cacheKey, CancellationToken ct = default);
}
