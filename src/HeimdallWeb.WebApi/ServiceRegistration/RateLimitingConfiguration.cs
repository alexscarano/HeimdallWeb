using System.Threading.RateLimiting;

namespace HeimdallWeb.WebApi.ServiceRegistration;

/// <summary>
/// Extension method para registrar Rate Limiting (limitação de requisições).
/// Protege a API contra abuso com limites globais e específicos por endpoint.
/// </summary>
public static class RateLimitingConfiguration
{
    /// <summary>
    /// Configura rate limiting com limite global e policy customizada para scan.
    /// </summary>
    /// <remarks>
    /// - Limite Global: 85 requisições/minuto por IP (proteção geral)
    /// - Limite Scan: 4 requisições/minuto por IP (proteção de endpoint caro)
    ///   - Scan é a operação mais custosa, exige limite mais restritivo
    /// </remarks>
    /// <param name="services">IServiceCollection para registrar o serviço</param>
    /// <returns>IServiceCollection para encadeamento de chamadas</returns>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limit: 85 requests/minute per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 85,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Scan-specific rate limit: 4 requests/minute per IP
            options.AddPolicy("ScanPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 4,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Auth rate limit: 10 requests/minute per IP (brute force protection)
            options.AddPolicy("AuthPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        return services;
    }
}
