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
    /// Deletes all cache entries where the JSON result's Target matches the provided URL.
    /// Used for cascade deletion when a user deletes their scan history.
    /// </summary>
    Task DeleteByTargetAsync(string target, CancellationToken ct = default);

    /// <summary>
    /// Deletes the cache entry with the given key, regardless of expiry state.
    /// Used for upsert logic: delete-then-insert to refresh an existing entry's TTL.
    /// No-op if no entry exists for the key.
    /// </summary>
    Task DeleteByCacheKeyAsync(string cacheKey, CancellationToken ct = default);
}
