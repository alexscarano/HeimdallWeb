using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Cached result of a security scan. Prevents duplicate scans for the same target
/// within a configurable TTL window, reducing external API usage and improving latency.
/// </summary>
public class ScanCache
{
    /// <summary>Primary key (auto-generated).</summary>
    public int Id { get; private set; }

    /// <summary>
    /// SHA-256 hex digest of the normalized target URL and profile ID.
    /// Exactly 64 characters (lowercase hex). Unique constraint enforced at DB level.
    /// </summary>
    public string CacheKey { get; private set; } = string.Empty;

    /// <summary>
    /// Complete scan result serialized as JSON. Stored as PostgreSQL JSONB.
    /// </summary>
    public string ResultJson { get; private set; } = string.Empty;

    /// <summary>When this cache entry was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>When this cache entry expires and should no longer be served.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>Returns true when the cache entry has passed its expiration timestamp.</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    // Parameterless constructor for EF Core
    private ScanCache() { }

    /// <summary>
    /// Creates a new ScanCache entry.
    /// </summary>
    /// <param name="cacheKey">SHA-256 hex key (64 lowercase characters).</param>
    /// <param name="resultJson">Full scan result JSON to cache.</param>
    /// <param name="expiresAt">Absolute expiry timestamp in UTC.</param>
    public ScanCache(string cacheKey, string resultJson, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            throw new ValidationException("CacheKey cannot be empty.");

        if (cacheKey.Length > 64)
            throw new ValidationException("CacheKey cannot exceed 64 characters.");

        if (string.IsNullOrWhiteSpace(resultJson))
            throw new ValidationException("ResultJson cannot be empty.");

        if (expiresAt <= DateTime.UtcNow)
            throw new ValidationException("ExpiresAt must be a future timestamp.");

        CacheKey = cacheKey;
        ResultJson = resultJson;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }
}
