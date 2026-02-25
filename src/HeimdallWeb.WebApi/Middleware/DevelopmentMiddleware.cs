namespace HeimdallWeb.WebApi.Middleware;

/// <summary>
/// Extension method para registrar middleware específico do ambiente de desenvolvimento.
/// Inclui Swagger UI e outras ferramentas de desenvolvedor.
/// </summary>
public static class DevelopmentMiddleware
{
    /// <summary>
    /// Configura Swagger UI apenas em ambiente de desenvolvimento.
    /// </summary>
    /// <remarks>
    /// Swagger é desabilitado em produção por questões de segurança.
    /// A documentação interativa ajuda no desenvolvimento e testes de endpoints.
    /// </remarks>
    /// <param name="app">WebApplication para adicionar o middleware</param>
    /// <returns>WebApplication para encadeamento de chamadas</returns>
    public static WebApplication UseSwaggerDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "HeimdallWeb API v1");
            });
        }

        return app;
    }
}
