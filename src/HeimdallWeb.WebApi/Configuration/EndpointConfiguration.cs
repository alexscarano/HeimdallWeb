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
    /// Endpoints disponíveis (5 grupos):
    /// - Authentication: /api/v1/auth (login, register, logout)
    /// - Scan: /api/v1/scan (iniciar, listar, detalhes de scan)
    /// - History: /api/v1/history (histórico de scans)
    /// - User: /api/v1/user (perfil, configurações)
    /// - 📊 Dashboard: /api/v1/dashboard (estatísticas de admin)
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

        return app;
    }
}
