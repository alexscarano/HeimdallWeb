using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.ViewModels;

/// <summary>
/// ViewModel principal do dashboard administrativo.
/// Agrega todas as estatísticas e dados necessários para a view.
/// </summary>
public class AdminDashboardViewModel
{
    public DashboardUserStats UserStats { get; set; } = new();
    public DashboardScanStats ScanStats { get; set; } = new();
    public DashboardLogsOverview LogsOverview { get; set; } = new();
    public PaginatedResult<DashboardRecentActivity> RecentActivity { get; set; } = new();
    public List<DashboardScanTrendDaily> ScanTrend { get; set; } = new();
    public List<DashboardUserRegistrationTrend> UserRegistrationTrend { get; set; } = new();
    
    // Novas propriedades para dados de IA Summary
    public AdminIASummaryStats? IASummaryStats { get; set; }
    public List<AdminRiskDistributionDaily> RiskDistribution { get; set; } = new();
    public List<AdminTopCategory> TopCategories { get; set; } = new();
    public List<AdminMostVulnerableTarget> MostVulnerableTargets { get; set; } = new();
}
