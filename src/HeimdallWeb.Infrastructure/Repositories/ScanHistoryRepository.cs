using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ScanHistory entity.
/// Uses EF Core with PostgreSQL for data access.
/// Supports JSONB queries for raw_json_result column.
/// </summary>
public class ScanHistoryRepository : IScanHistoryRepository
{
    private readonly AppDbContext _context;

    public ScanHistoryRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ScanHistory?> GetByIdAsync(int historyId, CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .Include(h => h.Findings)
            .Include(h => h.Technologies)
            .Include(h => h.IASummaries)
            .FirstOrDefaultAsync(h => h.HistoryId == historyId, ct);
    }

    public async Task<IEnumerable<ScanHistory>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task<ScanHistory> AddAsync(ScanHistory history, CancellationToken ct = default)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        await _context.ScanHistories.AddAsync(history, ct);
        // SaveChanges will be called by UnitOfWork

        return history;
    }

    public async Task UpdateAsync(ScanHistory history, CancellationToken ct = default)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        _context.ScanHistories.Update(history);
        // SaveChanges will be called by UnitOfWork

        await Task.CompletedTask;
    }
}
