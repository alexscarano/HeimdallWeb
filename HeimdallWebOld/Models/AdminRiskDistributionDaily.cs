namespace HeimdallWeb.Models;

/// <summary>
/// Distribuição de riscos por dia (últimos 30 dias).
/// Mapeado para vw_admin_risk_distribution_daily.
/// </summary>
public class AdminRiskDistributionDaily
{
    public DateTime risk_date { get; set; }
    public int critical_findings { get; set; }
    public int high_findings { get; set; }
    public int medium_findings { get; set; }
    public int low_findings { get; set; }
    public int total_summaries { get; set; }
}
