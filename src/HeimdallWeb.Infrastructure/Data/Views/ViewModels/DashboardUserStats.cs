namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_dashboard_user_stats.
/// Aggregated user statistics for admin dashboard.
/// </summary>
public class DashboardUserStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int BlockedUsers { get; set; }
    public int NewUsersLast7Days { get; set; }
    public int NewUsersLast30Days { get; set; }
}
