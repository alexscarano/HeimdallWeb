namespace HeimdallWeb.Models;

/// <summary>
/// Representa uma entrada de atividade recente do sistema.
/// Mapeado para vw_dashboard_recent_activity.
/// </summary>
public class DashboardRecentActivity
{
    public DateTime timestamp { get; set; }
    public int? user_id { get; set; }
    public string level { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public string source { get; set; } = string.Empty;
    public string? remote_ip { get; set; }
    public string? username { get; set; }
}
