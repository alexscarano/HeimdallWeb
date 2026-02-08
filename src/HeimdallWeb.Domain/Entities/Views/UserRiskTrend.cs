namespace HeimdallWeb.Domain.Entities.Views;

/// <summary>
/// Read-only entity mapped to vw_user_risk_trend SQL VIEW.
/// Shows risk trend over the last 30 days per user.
/// </summary>
public class UserRiskTrend
{
    public int UserId { get; set; }
    public DateTime RiskDate { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
    public int InformationalCount { get; set; }
    public int ScansOnDate { get; set; }
}
