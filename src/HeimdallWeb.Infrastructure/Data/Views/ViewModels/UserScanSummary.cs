namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_user_scan_summary.
/// Per-user scan statistics summary.
/// </summary>
public class UserScanSummary
{
    public int UserId { get; set; }
    public int TotalScans { get; set; }
    public int CompletedScans { get; set; }
    public int FailedScans { get; set; }
    public int UniqueTargets { get; set; }
    public double AvgScanDurationSeconds { get; set; }
    public DateTime? LastScanDate { get; set; }
    public int ScansLast7Days { get; set; }
    public int ScansLast30Days { get; set; }
}
