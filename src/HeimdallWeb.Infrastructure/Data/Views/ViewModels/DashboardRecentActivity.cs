namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_dashboard_recent_activity.
/// Recent log activity (last 50 entries) for admin dashboard.
/// </summary>
public class DashboardRecentActivity
{
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
    public string? Username { get; set; }
}
