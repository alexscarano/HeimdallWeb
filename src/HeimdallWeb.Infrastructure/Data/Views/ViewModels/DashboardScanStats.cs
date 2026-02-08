namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_dashboard_scan_stats.
/// Aggregated scan statistics for admin dashboard.
/// </summary>
public class DashboardScanStats
{
    public int TotalScans { get; set; }
    public int ScansLast24h { get; set; }
    public double AvgScanTimeS { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal FailRate { get; set; }
}
