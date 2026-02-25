using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Technology entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class TechnologyRepository : ITechnologyRepository
{
    private readonly AppDbContext _context;

    public TechnologyRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Technology>> GetByHistoryIdAsync(int historyId, CancellationToken ct = default)
    {
        return await _context.Technologies
            .AsNoTracking()
            .Where(t => t.HistoryId == historyId)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<Technology> AddAsync(Technology technology, CancellationToken ct = default)
    {
        if (technology == null)
            throw new ArgumentNullException(nameof(technology));

        await _context.Technologies.AddAsync(technology, ct);
        // SaveChanges will be called by UnitOfWork

        return technology;
    }

    public async Task AddRangeAsync(IEnumerable<Technology> technologies, CancellationToken ct = default)
    {
        if (technologies == null)
            throw new ArgumentNullException(nameof(technologies));

        await _context.Technologies.AddRangeAsync(technologies, ct);
        // SaveChanges will be called by UnitOfWork
    }
}
