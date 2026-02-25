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
        // Delete-then-insert upsert: removes any existing entry (expired or not) before inserting
        // the new one so the unique constraint on cache_key is never violated.
        await _unitOfWork.ScanCaches.DeleteByCacheKeyAsync(cacheKey, ct);

        var cache = new ScanCache(cacheKey, resultJson, DateTime.UtcNow.Add(expiration));
        await _unitOfWork.ScanCaches.AddAsync(cache, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CleanupExpiredCacheAsync(CancellationToken ct = default)
    {
        await _unitOfWork.ScanCaches.DeleteExpiredAsync(ct);
    }

    /// <inheritdoc/>
    public async Task ClearCacheForTargetAsync(string target, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(target))
            return;

        await _unitOfWork.ScanCaches.DeleteByTargetAsync(target, ct);
    }
}
