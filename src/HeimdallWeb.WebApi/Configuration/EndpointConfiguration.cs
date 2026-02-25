using HeimdallWeb.WebApi.Endpoints;

namespace HeimdallWeb.WebApi.Configuration;

/// <summary>
/// Extension method para registrar todos os grupos de endpoints (Minimal APIs).
/// Centraliza o mapeamento de routes em uma única chamada.
/// </summary>
public static class EndpointConfiguration
{
    /// <summary>
    /// Registra todos os grupos de endpoints da aplicação.
    /// 
    /// Endpoints disponíveis (7 grupos):
    /// - Authentication: /api/v1/auth (login, register, logout, forgot-password, reset-password, google)
    /// - Scan: /api/v1/scan (iniciar, listar, detalhes de scan)
    /// - History: /api/v1/history (histórico de scans)
    /// - User: /api/v1/user (perfil, configurações)
    /// - Dashboard: /api/v1/dashboard (estatísticas de admin)
    /// - Profiles: /api/v1/profiles (scan profiles — public)
    /// - Monitor: /api/v1/monitor (monitoramento periódico de targets — Sprint 4)
    /// - Support: /api/v1/support (contact form — Sprint 5)
    /// </summary>
    /// <remarks>
    /// Cada endpoint group é mapeado em sua própria classe estática (Endpoints/*.cs)
    /// seguindo o padrão: public static RouteGroupBuilder MapXyzEndpoints(this WebApplication app)
    /// </remarks>
    /// <param name="app">WebApplication para mapear os endpoints</param>
    /// <returns>WebApplication para encadeamento de chamadas</returns>
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapScanEndpoints();
        app.MapHistoryEndpoints();
        app.MapUserEndpoints();
        app.MapDashboardEndpoints();
        app.MapProfileEndpoints();
        app.MapMonitorEndpoints();
        app.MapSupportEndpoints(); // Sprint 5
        app.MapNotificationEndpoints(); // G0

        return app;
    }
}
