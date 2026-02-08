namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_admin_risk_distribution_daily.
/// Daily risk distribution across all scans (last 30 days).
/// </summary>
public class AdminRiskDistributionDaily
{
    public DateTime RiskDate { get; set; }
    public int CriticalFindings { get; set; }
    public int HighFindings { get; set; }
    public int MediumFindings { get; set; }
    public int LowFindings { get; set; }
    public int InformationalFindings { get; set; }
    public int TotalScans { get; set; }
}
