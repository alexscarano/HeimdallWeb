namespace HeimdallWeb.WebApi.Middleware;

/// <summary>
/// Extension methods para registrar middleware de segurança no pipeline.
/// Ordem: HTTPS → CORS → Authentication → Authorization → RateLimiting
/// ⚠️ ORDEM CRÍTICA - Não alterar sem documentar razão!
/// </summary>
public static class SecurityMiddleware
{
    /// <summary>
    /// Configura o pipeline de middleware de segurança na ordem correta.
    /// 
    ///  ORDEM IMPORTA:
    /// 1️⃣ HTTPS Redirection (protocolo seguro)
    /// 2️⃣ CORS (antes de auth, controla origens permitidas)
    /// 3️⃣ Authentication (identifica usuário via JWT)
    /// 4️⃣ Authorization (verifica permissões)
    /// 5️⃣ RateLimiting (limita requisições por IP)
    /// </summary>
    /// <remarks>
    /// A ordem é crucial:
    /// - HTTPS redireciona HTTP para HTTPS
    /// - CORS vem antes de auth para validar origens antes de gastar recursos
    /// - Auth obtém a identidade do usuário
    /// - Authorization valida se tem permissão para o recurso
    /// - RateLimiting é aplicado por último para contar todas as requisições
    /// </remarks>
    /// <param name="app">WebApplication para adicionar o middleware</param>
    /// <returns>WebApplication para encadeamento de chamadas</returns>
    public static WebApplication UseSecurityMiddlewarePipeline(this WebApplication app)
    {
        // 0️⃣ Security Headers (prevent common web attacks)
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next();
        });

        // 1️⃣ HTTPS Redirection (protocol-level security)
        app.UseHttpsRedirection();

        // 2️⃣ CORS (resource sharing from Next.js frontend)
        app.UseCors();

        // 3️⃣ Authentication (JWT from cookie)
        app.UseAuthentication();

        // 4️⃣ Authorization (policy-based access control)
        app.UseAuthorization();

        // 5️⃣ Rate Limiting (per-IP request throttling)
        app.UseRateLimiter();

        return app;
    }
}
