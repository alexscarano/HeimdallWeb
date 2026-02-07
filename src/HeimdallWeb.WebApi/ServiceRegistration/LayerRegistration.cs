using HeimdallWeb.Application;
using HeimdallWeb.Infrastructure;

namespace HeimdallWeb.WebApi.ServiceRegistration;

/// <summary>
/// Extension method para registrar as camadas da aplicação (Application e Infrastructure).
/// Centraliza a configuração de handlers, validators, repositories e serviços.
/// </summary>
public static class LayerRegistration
{
    /// <summary>
    /// Registra todos os serviços da camada Application.
    /// Inclui: 19 handlers de comando/query, 9 validadores, 3 serviços.
    /// </summary>
    /// <param name="services">IServiceCollection para registrar os serviços</param>
    /// <returns>IServiceCollection para encadeamento de chamadas</returns>
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddApplication();
        return services;
    }

    /// <summary>
    /// Registra todos os serviços da camada Infrastructure.
    /// Inclui: DbContext, 9 repositórios, Unit of Work, serviços externos (Gemini).
    /// </summary>
    /// <param name="services">IServiceCollection para registrar os serviços</param>
    /// <param name="configuration">IConfiguration para obter connection strings e configurações</param>
    /// <returns>IServiceCollection para encadeamento de chamadas</returns>
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        return services;
    }
}
