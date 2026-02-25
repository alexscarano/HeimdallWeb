namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_dashboard_scan_trend_daily.
/// Maps to PostgreSQL VIEW that returns daily scan statistics (last 30 days).
/// Used by Admin Dashboard for trend chart.
/// </summary>
public class DashboardScanTrendDaily
{
    public DateTime ScanDate { get; set; }
    public int ScanCount { get; set; }
    public int SuccessfulScans { get; set; }
    public int FailedScans { get; set; }
}
