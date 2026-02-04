namespace HeimdallWeb.Models;

/// <summary>
/// Tendência de scans por dia (últimos 30 dias).
/// Usado para gráficos de linha.
/// Mapeado para vw_dashboard_scan_trend_daily.
/// </summary>
public class DashboardScanTrendDaily
{
    public DateTime scan_date { get; set; }
    public int scan_count { get; set; }
    public int successful_scans { get; set; }
    public int failed_scans { get; set; }
}
