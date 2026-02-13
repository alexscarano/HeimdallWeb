namespace HeimdallWeb.Models;

/// <summary>
/// Estatísticas gerais de findings e riscos (agregadas).
/// Mapeado para vw_admin_ia_summary_stats.
/// </summary>
public class AdminIASummaryStats
{
    public long total_findings_all_scans { get; set; }
    public long total_critical { get; set; }
    public long total_high { get; set; }
    public long total_medium { get; set; }
    public long total_low { get; set; }
    public decimal avg_findings_per_scan { get; set; }
    public int scans_critical_risk { get; set; }
    public int scans_high_risk { get; set; }
    public int scans_medium_risk { get; set; }
    public int scans_low_risk { get; set; }
    public long critical_last_24h { get; set; }
    public long high_last_24h { get; set; }
}
