namespace HeimdallWeb.Models;

/// <summary>
/// Estatísticas agregadas de scans/varreduras.
/// Mapeado para vw_dashboard_scan_stats.
/// </summary>
public class DashboardScanStats
{
    public int total_scans { get; set; }
    public int scans_last_24h { get; set; }
    public double avg_scan_time_s { get; set; }
    public decimal success_rate { get; set; }
    public decimal fail_rate { get; set; }
}
