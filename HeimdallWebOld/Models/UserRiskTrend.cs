namespace HeimdallWeb.Models;

/// <summary>
/// Tendência de riscos ao longo do tempo para usuário.
/// Mapeado para vw_user_risk_trend.
/// </summary>
public class UserRiskTrend
{
    public int user_id { get; set; }
    public DateTime risk_date { get; set; }
    public int critical_count { get; set; }
    public int high_count { get; set; }
    public int medium_count { get; set; }
    public int low_count { get; set; }
    public int scans_on_date { get; set; }
}
