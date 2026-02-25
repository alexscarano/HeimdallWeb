using Microsoft.OpenApi.Models;

namespace HeimdallWeb.WebApi.ServiceRegistration;

/// <summary>
/// Extension method para registrar Swagger/OpenAPI na aplicação.
/// Configuração de documentação de API (desenvolvimento apenas).
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configura o Swagger Gen com documentação de segurança JWT via cookie.
    /// </summary>
    /// <param name="services">IServiceCollection para registrar o serviço</param>
    /// <returns>IServiceCollection para encadeamento de chamadas</returns>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Name = "authHeimdallCookie",
                Description = "JWT token stored in cookie (set automatically after /api/v1/auth/login)"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Cookie"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
