namespace HeimdallWeb.WebApi.ServiceRegistration;

/// <summary>
/// Extension method para registrar CORS (Cross-Origin Resource Sharing).
/// Configuração para permitir requisições do frontend Next.js (localhost:3000).
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Configura CORS para o frontend Next.js com suporte a cookies de autenticação.
    /// </summary>
    /// <remarks>
    /// Permite requisições de localhost:3000 (desenvolvimento) e https://localhost:3000 (produção local).
    /// AllowCredentials é essencial para enviar/receber cookies JWT automaticamente.
    /// </remarks>
    /// <param name="services">IServiceCollection para registrar o serviço</param>
    /// <returns>IServiceCollection para encadeamento de chamadas</returns>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins
                        ("http://localhost:3000", "https://localhost:3000",
                         "http://localhost:3001",  "https://localhost:3001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Important for JWT cookies
            });
        });

        return services;
    }
}
