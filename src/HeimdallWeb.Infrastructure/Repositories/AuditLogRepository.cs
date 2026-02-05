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
}
