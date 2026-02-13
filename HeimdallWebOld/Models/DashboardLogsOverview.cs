namespace HeimdallWeb.Models;

/// <summary>
/// Visão geral dos logs do sistema (contadores por nível e período).
/// Mapeado para vw_dashboard_logs_overview.
/// </summary>
public class DashboardLogsOverview
{
    public int total_logs { get; set; }
    public int logs_today { get; set; }
    public int logs_errors_last_24h { get; set; }
    public int logs_warn_last_24h { get; set; }
    public int logs_info_last_24h { get; set; }
}
