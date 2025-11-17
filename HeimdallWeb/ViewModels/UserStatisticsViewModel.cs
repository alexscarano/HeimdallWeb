using HeimdallWeb.Models;

namespace HeimdallWeb.ViewModels;

/// <summary>
/// ViewModel para a página de Estatísticas do Usuário.
/// Consolida dados de múltiplas views para exibir no dashboard individual.
/// </summary>
public class UserStatisticsViewModel
{
    public UserScanSummary? ScanSummary { get; set; }
    public UserFindingsSummary? FindingsSummary { get; set; }
    public List<UserRiskTrend> RiskTrend { get; set; } = new();
    public List<UserCategoryBreakdown> CategoryBreakdown { get; set; } = new();   
    public List<HeimdallWeb.Models.HistoryModel> RecentScans { get; set; } = new();
}
