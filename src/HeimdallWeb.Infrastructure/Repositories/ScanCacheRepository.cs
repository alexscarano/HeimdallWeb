using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ScanCache entity.
/// Uses batch DELETE via ExecuteDeleteAsync for efficient cache cleanup.
/// </summary>
public class ScanCacheRepository : IScanCacheRepository
{
    private readonly AppDbContext _context;

    public ScanCacheRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<ScanCache?> GetValidCacheAsync(string cacheKey, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _context.ScanCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey && c.ExpiresAt > now, ct);
    }

    /// <inheritdoc/>
    public async Task<ScanCache> AddAsync(ScanCache cache, CancellationToken ct = default)
    {
        if (cache == null)
            throw new ArgumentNullException(nameof(cache));

        await _context.ScanCaches.AddAsync(cache, ct);
        // SaveChanges will be called by UnitOfWork
        return cache;
    }

    /// <inheritdoc/>
    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // ExecuteDeleteAsync issues a single DELETE statement — no entity tracking required
        await _context.ScanCaches
            .Where(c => c.ExpiresAt <= now)
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteByTargetAsync(string target, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(target))
            return;

        // The cache JSON stores the normalized URL (e.g. "https://google.com") but the caller
        // may pass the protocol-stripped value from ScanTarget.Value (e.g. "google.com").
        // Cover all three variants so the cascade delete always hits the cache entry.
        var sql = """
            DELETE FROM tb_scan_cache
            WHERE (result_json::jsonb ->> 'Target') IN (
                {0},
                'https://' || {0},
                'http://' || {0},
                'https://www.' || {0},
                'http://www.' || {0}
            )
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, new object[] { target }, ct);
    }

    /// <inheritdoc/>
    public async Task DeleteByCacheKeyAsync(string cacheKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            return;

        await _context.ScanCaches
            .Where(c => c.CacheKey == cacheKey)
            .ExecuteDeleteAsync(ct);
    }
}
