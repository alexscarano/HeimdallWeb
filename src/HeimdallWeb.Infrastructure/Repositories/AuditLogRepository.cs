using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AuditLog entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AuditLog> AddAsync(AuditLog log, CancellationToken ct = default)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        await _context.AuditLogs.AddAsync(log, ct);
        // SaveChanges will be called by UnitOfWork

        return log;
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? source = null,
        string? username = null,
        CancellationToken ct = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(l => l.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(l => l.Level == level);
        }

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            query = query.Where(l => l.Source == source);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            query = query.Where(l => l.User != null && EF.Functions.ILike(l.User.Username, $"%{username}%"));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Apply pagination and ordering
        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (logs, totalCount);
    }

    public async Task<List<string>> GetDistinctSourcesAsync(CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.Source != null)
            .Select(l => l.Source!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetDistinctMessagesAsync(CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Select(l => l.Message)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync(ct);
    }
}
