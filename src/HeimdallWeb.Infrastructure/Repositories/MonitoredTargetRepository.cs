using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for MonitoredTarget entity.
/// All read operations use AsNoTracking() for optimal performance.
/// </summary>
public class MonitoredTargetRepository : IMonitoredTargetRepository
{
    private readonly AppDbContext _context;

    public MonitoredTargetRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<MonitoredTarget?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.MonitoredTargets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MonitoredTarget>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.MonitoredTargets
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MonitoredTarget>> GetDueForCheckAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _context.MonitoredTargets
            .AsNoTracking()
            .Where(t => t.IsActive && t.NextCheck <= now)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<MonitoredTarget> AddAsync(MonitoredTarget target, CancellationToken ct = default)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        await _context.MonitoredTargets.AddAsync(target, ct);
        // SaveChanges will be called by UnitOfWork
        return target;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(MonitoredTarget target, CancellationToken ct = default)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        _context.MonitoredTargets.Remove(target);
        // SaveChanges will be called by UnitOfWork
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(MonitoredTarget target, CancellationToken ct = default)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        _context.MonitoredTargets.Update(target);
        // SaveChanges will be called by UnitOfWork
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByUserAndUrlAsync(int userId, string url, CancellationToken ct = default)
    {
        return await _context.MonitoredTargets
            .AsNoTracking()
            .AnyAsync(t => t.UserId == userId && t.Url == url, ct);
    }
}
