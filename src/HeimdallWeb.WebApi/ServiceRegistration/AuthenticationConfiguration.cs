using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HeimdallWeb.WebApi.ServiceRegistration;

/// <summary>
/// Extension method para registrar autenticação JWT.
/// Valida tokens JWT obtidos de cookies e configura segurança de tokens.
/// </summary>
public static class AuthenticationConfiguration
{
    /// <summary>
    /// Configura autenticação JWT com suporte a leitura de cookies.
    /// </summary>
    /// <remarks>
    /// - Valida issuer, audience, lifetime e chave de assinatura
    /// - Obtém o token JWT do cookie "authHeimdallCookie"
    /// - Padrão de segurança seguindo OWASP para armazenamento de JWT em cookies
    /// </remarks>
    /// <param name="services">IServiceCollection para registrar o serviço</param>
    /// <param name="configuration">IConfiguration para obter parâmetros JWT do appsettings.json</param>
    /// <returns>AuthenticationBuilder para encadeamento e adicionar outros esquemas se necessário</returns>
    /// <exception cref="InvalidOperationException">Lançada se JWT:Key não estiver configurado</exception>
    public static AuthenticationBuilder AddJwtAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "HeimdallWeb";
        var jwtAudience = configuration["Jwt:Audience"] ?? "HeimdallWebUsers";

        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };

                // Support JWT from cookie (authHeimdallCookie)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["authHeimdallCookie"];
                        return Task.CompletedTask;
                    }
                };
            });
    }

    /// <summary>
    /// Registra o serviço de autorização vazio (com ou sem policies customizadas).
    /// </summary>
    /// <param name="services">IServiceCollection para registrar o serviço</param>
    /// <returns>IServiceCollection para encadeamento de chamadas</returns>
    public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}
