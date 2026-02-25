using HeimdallWeb.Domain.Entities.Views;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for user statistics SQL VIEWs (read-only).
/// </summary>
public interface IUserStatisticsViewRepository
{
    Task<IEnumerable<UserRiskTrend>> GetUserRiskTrendAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<UserCategoryBreakdown>> GetUserCategoryBreakdownAsync(int userId, CancellationToken ct = default);
}
