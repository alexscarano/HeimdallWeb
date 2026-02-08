namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_user_findings_summary.
/// Per-user findings breakdown by severity.
/// </summary>
public class UserFindingsSummary
{
    public int UserId { get; set; }
    public int TotalFindings { get; set; }
    public int TotalCritical { get; set; }
    public int TotalHigh { get; set; }
    public int TotalMedium { get; set; }
    public int TotalLow { get; set; }
    public int TotalInformational { get; set; }
    public decimal PercentCritical { get; set; }
    public decimal PercentHigh { get; set; }
    public decimal PercentMedium { get; set; }
    public decimal PercentLow { get; set; }
    public decimal PercentInformational { get; set; }
}
