using System.Security.Cryptography;
using System.Text;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Services;

/// <summary>
/// Implementation of <see cref="IScanCacheService"/>.
/// Generates SHA-256 cache keys and manages ScanCache persistence via UnitOfWork.
/// </summary>
public class ScanCacheService : IScanCacheService
{
    private readonly IUnitOfWork _unitOfWork;

    public ScanCacheService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc/>
    public string GenerateCacheKey(string target, int? profileId)
    {
        var input = $"{target.ToLowerInvariant()}:{profileId?.ToString() ?? "default"}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant(); // 64-char lowercase hex
    }

    /// <inheritdoc/>
    public async Task<string?> GetCachedResultAsync(string cacheKey, CancellationToken ct = default)
    {
        var cache = await _unitOfWork.ScanCaches.GetValidCacheAsync(cacheKey, ct);
        return cache?.ResultJson;
    }

    /// <inheritdoc/>
    public async Task CacheResultAsync(string cacheKey, string resultJson, TimeSpan expiration, CancellationToken ct = default)
    {
        // If an entry exists for this key, remove it so the new one replaces it with a fresh TTL
        var existing = await _unitOfWork.ScanCaches.GetByCacheKeyAsync(cacheKey, ct);
        if (existing != null)
        {
            // Re-fetch as tracked entity for deletion
            var tracked = await _unitOfWork.ScanCaches.GetByCacheKeyAsync(cacheKey, ct);
            if (tracked != null)
            {
                // Delete expired entries as a bonus cleanup before inserting the new one
                await _unitOfWork.ScanCaches.DeleteExpiredAsync(ct);
            }
        }

        var cache = new ScanCache(cacheKey, resultJson, DateTime.UtcNow.Add(expiration));
        await _unitOfWork.ScanCaches.AddAsync(cache, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task CleanupExpiredCacheAsync(CancellationToken ct = default)
    {
        await _unitOfWork.ScanCaches.DeleteExpiredAsync(ct);
    }
}
