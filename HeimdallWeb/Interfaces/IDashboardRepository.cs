using HeimdallWeb.ViewModels;

namespace HeimdallWeb.Interfaces;

/// <summary>
/// Repositório responsável por fornecer dados agregados para o dashboard administrativo.
/// Consome exclusivamente VIEWs SQL otimizadas.
/// Implementa cache em memória para reduzir carga no banco.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Retorna o ViewModel completo do dashboard administrativo.
    /// Cache: 30 segundos (estatísticas) e logs paginados sem cache.
    /// </summary>
    Task<AdminDashboardViewModel> GetAdminDashboardDataAsync(int logPage = 1, int logPageSize = 10);

    /// <summary>
    /// Placeholder para métricas do usuário (não admin).
    /// A ser implementado futuramente.
    /// </summary>
    Task<UserMetricsViewModel?> GetUserMetricsAsync(int userId);
}
