using HeimdallWeb.Domain.DTOs;
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

    public async Task DeleteAsync(int historyId, CancellationToken ct = default)
    {
        var history = await _context.ScanHistories
            .FirstOrDefaultAsync(h => h.HistoryId == historyId, ct);

        if (history != null)
        {
            _context.ScanHistories.Remove(history);
            // SaveChanges will be called by UnitOfWork
        }

        await Task.CompletedTask;
    }

    public async Task<ScanHistory?> GetByIdWithIncludesAsync(int historyId, CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .Include(h => h.Findings)
            .Include(h => h.Technologies)
            .Include(h => h.IASummaries)
            .FirstOrDefaultAsync(h => h.HistoryId == historyId, ct);
    }

    public async Task<(IEnumerable<ScanHistorySummaryResponse> Items, int TotalCount)> GetByUserIdPaginatedAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.ScanHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(h => h.CreatedDate)
            .Select(h => new ScanHistorySummaryResponse(
                h.HistoryId,
                h.Target.Value,
                h.CreatedDate,
                h.Duration != null ? ((TimeSpan)h.Duration).ToString(@"hh\:mm\:ss") : null,
                h.HasCompleted,
                h.Summary,
                h.Findings.Count(),
                h.Technologies.Count()
            ))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<ScanHistory>> GetAllByUserIdWithIncludesAsync(int userId, CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .Include(h => h.Findings)
            .Include(h => h.Technologies)
            .Include(h => h.IASummaries)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ScanHistory>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .CountAsync(h => h.UserId == userId, ct);
    }

    public async Task<IEnumerable<ScanHistory>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        return await _context.ScanHistories
            .AsNoTracking()
            .Include(h => h.User)
            .Include(h => h.Findings)
            .OrderByDescending(h => h.CreatedDate)
            .Take(count)
            .ToListAsync(ct);
    }
}
