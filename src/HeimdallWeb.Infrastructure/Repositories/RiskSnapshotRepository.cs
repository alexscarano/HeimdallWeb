using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RiskSnapshot entity.
/// All read operations use AsNoTracking() for optimal performance.
/// </summary>
public class RiskSnapshotRepository : IRiskSnapshotRepository
{
    private readonly AppDbContext _context;

    public RiskSnapshotRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<RiskSnapshot?> GetLatestByTargetIdAsync(int monitoredTargetId, CancellationToken ct = default)
    {
        return await _context.RiskSnapshots
            .AsNoTracking()
            .Where(s => s.MonitoredTargetId == monitoredTargetId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RiskSnapshot>> GetByTargetIdAsync(int monitoredTargetId, int limit = 30, CancellationToken ct = default)
    {
        return await _context.RiskSnapshots
            .AsNoTracking()
            .Where(s => s.MonitoredTargetId == monitoredTargetId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<RiskSnapshot> AddAsync(RiskSnapshot snapshot, CancellationToken ct = default)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        await _context.RiskSnapshots.AddAsync(snapshot, ct);
        // SaveChanges will be called by UnitOfWork
        return snapshot;
    }
}
