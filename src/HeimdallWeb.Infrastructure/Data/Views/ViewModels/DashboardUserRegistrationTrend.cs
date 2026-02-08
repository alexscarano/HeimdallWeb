namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_dashboard_user_registration_trend.
/// Maps to PostgreSQL VIEW that returns daily user registration statistics (last 30 days).
/// Used by Admin Dashboard for registration trend chart.
/// </summary>
public class DashboardUserRegistrationTrend
{
    public DateTime RegistrationDate { get; set; }
    public int NewUsers { get; set; }
}
