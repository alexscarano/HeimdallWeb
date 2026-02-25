namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_admin_most_vulnerable_targets.
/// Most vulnerable targets ranked by findings count.
/// </summary>
public class AdminMostVulnerableTargets
{
    public string Target { get; set; } = string.Empty;
    public int ScanCount { get; set; }
    public int TotalFindings { get; set; }
    public int TotalCritical { get; set; }
    public int TotalHigh { get; set; }
    public int TotalMedium { get; set; }
    public int HighestRiskLevel { get; set; }
    public DateTime LastScanDate { get; set; }
}
