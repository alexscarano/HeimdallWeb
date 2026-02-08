namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_admin_ia_summary_stats.
/// AI summary statistics with comparison to actual findings.
/// </summary>
public class AdminIASummaryStats
{
    public int HistoryId { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ScanTotalFindings { get; set; }
    public int ScanMaxSeverity { get; set; }
    public int ScanCriticalCount { get; set; }
    public int ScanHighCount { get; set; }
    public int ScanMediumCount { get; set; }
    public int ScanLowCount { get; set; }
    public int ScanInformationalCount { get; set; }
}
