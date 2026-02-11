/*
 * 
 * Configuration is now split into extension methods organized by responsibility:
 * 
 * ServiceRegistration/: Registers DI services
 *   - SwaggerConfiguration.cs: API documentation
 *   - CorsConfiguration.cs: CORS for Next.js frontend
 *   - AuthenticationConfiguration.cs: JWT authentication
 *   - RateLimitingConfiguration.cs: Request throttling
 *   - LayerRegistration.cs: Application & Infrastructure layers
 * 
 * Middleware/: Middleware pipeline configuration
 *   - DevelopmentMiddleware.cs: Development-only services (Swagger)
 *   - SecurityMiddleware.cs: Security pipeline (CORS, Auth, AuthZ, RateLimit)
 * 
 * Configuration/: Endpoint mapping
 *   - EndpointConfiguration.cs: Maps all Minimal API endpoints
 */

using HeimdallWeb.WebApi.ServiceRegistration;
using HeimdallWeb.WebApi.Middleware;
using HeimdallWeb.WebApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICE REGISTRATION =====
// Register all services using extension methods organized by concern

// Documentation & Client UI
builder.Services.AddSwaggerConfiguration();

// Network Security & CORS
builder.Services.AddCorsConfiguration();

// Authentication & Authorization
builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorizationConfiguration();

// Request Throttling
builder.Services.AddRateLimitingConfiguration();

// Application Layers (DDD + Repository Pattern)
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

// ===== BUILD & CONFIGURE MIDDLEWARE PIPELINE =====

var app = builder.Build();

// Development-only services (Swagger UI)
app.UseSwaggerDevelopment();

// Static files (profile images, uploads) — must be before auth to serve public assets
app.UseStaticFiles();

// Global exception handler (must be early in pipeline to catch all exceptions)
app.UseGlobalExceptionHandler();

// Security middleware pipeline (order is CRITICAL - DO NOT CHANGE)
// Order: HTTPS → CORS → Authentication → Authorization → RateLimiting
app.UseSecurityMiddlewarePipeline();

// ===== ENDPOINT REGISTRATION =====
// All Minimal API endpoints (grouped by domain concern)
app.MapAllEndpoints();

app.Run();
