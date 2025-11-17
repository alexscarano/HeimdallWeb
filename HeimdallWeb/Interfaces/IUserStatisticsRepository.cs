using HeimdallWeb.Models;
using HeimdallWeb.ViewModels;

namespace HeimdallWeb.Interfaces;

public interface IUserStatisticsRepository
{
    /// <summary>
    /// Retorna todas as estatísticas consolidadas para um usuário específico
    /// </summary>
    Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId);
    
    /// <summary>
    /// Retorna resumo de scans do usuário
    /// </summary>
    Task<UserScanSummary?> GetUserScanSummaryAsync(int userId);
    
    /// <summary>
    /// Retorna resumo de findings do usuário
    /// </summary>
    Task<UserFindingsSummary?> GetUserFindingsSummaryAsync(int userId);
    
    /// <summary>
    /// Retorna tendência de riscos do usuário (últimos 30 dias)
    /// </summary>
    Task<List<UserRiskTrend>> GetUserRiskTrendAsync(int userId);
    
    /// <summary>
    /// Retorna breakdown de categorias do usuário
    /// </summary>
    Task<List<UserCategoryBreakdown>> GetUserCategoryBreakdownAsync(int userId);
}
