using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Finding entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class FindingRepository : IFindingRepository
{
    private readonly AppDbContext _context;

    public FindingRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Finding>> GetByHistoryIdAsync(int historyId, CancellationToken ct = default)
    {
        return await _context.Findings
            .AsNoTracking()
            .Where(f => f.HistoryId == historyId)
            .OrderByDescending(f => f.Severity)
            .ThenBy(f => f.Type)
            .ToListAsync(ct);
    }

    public async Task<Finding> AddAsync(Finding finding, CancellationToken ct = default)
    {
        if (finding == null)
            throw new ArgumentNullException(nameof(finding));

        await _context.Findings.AddAsync(finding, ct);
        // SaveChanges will be called by UnitOfWork

        return finding;
    }

    public async Task AddRangeAsync(IEnumerable<Finding> findings, CancellationToken ct = default)
    {
        if (findings == null)
            throw new ArgumentNullException(nameof(findings));

        await _context.Findings.AddRangeAsync(findings, ct);
        // SaveChanges will be called by UnitOfWork
    }

    public async Task<IEnumerable<Finding>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Findings
            .AsNoTracking()
            .Include(f => f.History)
            .Where(f => f.History != null && f.History.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Finding>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Findings
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Findings
            .AsNoTracking()
            .Include(f => f.History)
            .CountAsync(f => f.History != null && f.History.UserId == userId, ct);
    }
}
