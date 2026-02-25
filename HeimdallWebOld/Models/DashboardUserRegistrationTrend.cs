namespace HeimdallWeb.Models;

/// <summary>
/// Tendência de registro de usuários por dia (últimos 30 dias).
/// Usado para gráficos de linha.
/// Mapeado para vw_dashboard_user_registration_trend.
/// </summary>
public class DashboardUserRegistrationTrend
{
    public DateTime registration_date { get; set; }
    public int new_users { get; set; }
}
