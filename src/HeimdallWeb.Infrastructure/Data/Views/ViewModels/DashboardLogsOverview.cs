namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_dashboard_logs_overview.
/// Aggregated log statistics for admin dashboard.
/// </summary>
public class DashboardLogsOverview
{
    public int TotalLogs { get; set; }
    public int LogsToday { get; set; }
    public int LogsErrorsLast24h { get; set; }
    public int LogsWarnLast24h { get; set; }
    public int LogsInfoLast24h { get; set; }
}
