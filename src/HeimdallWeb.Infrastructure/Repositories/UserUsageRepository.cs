using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserUsage entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class UserUsageRepository : IUserUsageRepository
{
    private readonly AppDbContext _context;

    public UserUsageRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserUsage?> GetByUserAndDateAsync(int userId, DateTime date, CancellationToken ct = default)
    {
        // Normalize to date only (no time component)
        var normalizedDate = date.Date;

        return await _context.UserUsages
            .AsNoTracking()
            .FirstOrDefaultAsync(uu => uu.UserId == userId && uu.Date == normalizedDate, ct);
    }

    public async Task<UserUsage> AddAsync(UserUsage usage, CancellationToken ct = default)
    {
        if (usage == null)
            throw new ArgumentNullException(nameof(usage));

        await _context.UserUsages.AddAsync(usage, ct);
        // SaveChanges will be called by UnitOfWork

        return usage;
    }

    public async Task UpdateAsync(UserUsage usage, CancellationToken ct = default)
    {
        if (usage == null)
            throw new ArgumentNullException(nameof(usage));

        _context.UserUsages.Update(usage);
        // SaveChanges will be called by UnitOfWork

        await Task.CompletedTask;
    }
}
