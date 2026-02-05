using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for IASummary entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class IASummaryRepository : IIASummaryRepository
{
    private readonly AppDbContext _context;

    public IASummaryRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IASummary?> GetByHistoryIdAsync(int historyId, CancellationToken ct = default)
    {
        return await _context.IASummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(ia => ia.HistoryId == historyId, ct);
    }

    public async Task<IASummary> AddAsync(IASummary summary, CancellationToken ct = default)
    {
        if (summary == null)
            throw new ArgumentNullException(nameof(summary));

        await _context.IASummaries.AddAsync(summary, ct);
        // SaveChanges will be called by UnitOfWork

        return summary;
    }
}
