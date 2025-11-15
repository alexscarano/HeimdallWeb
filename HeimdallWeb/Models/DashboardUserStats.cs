namespace HeimdallWeb.Models;

/// <summary>
/// Estatísticas agregadas de usuários do sistema.
/// Mapeado para vw_dashboard_user_stats.
/// </summary>
public class DashboardUserStats
{
    public int total_users { get; set; }
    public int active_users { get; set; }
    public int blocked_users { get; set; }
    public int new_users_last_7_days { get; set; }
    public int new_users_last_30_days { get; set; }
}
