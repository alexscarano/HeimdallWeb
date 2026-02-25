using HeimdallWeb.Domain.Entities.Views;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository for querying user statistics SQL VIEWs (read-only).
/// </summary>
public class UserStatisticsViewRepository : IUserStatisticsViewRepository
{
    private readonly AppDbContext _context;

    public UserStatisticsViewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRiskTrend>> GetUserRiskTrendAsync(int userId, CancellationToken ct = default)
    {
        return await _context.UserRiskTrends
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RiskDate)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<UserCategoryBreakdown>> GetUserCategoryBreakdownAsync(int userId, CancellationToken ct = default)
    {
        return await _context.UserCategoryBreakdowns
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CategoryCount)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
