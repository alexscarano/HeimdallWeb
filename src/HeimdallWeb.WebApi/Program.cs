using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HeimdallWeb.Application;
using HeimdallWeb.Infrastructure;
using HeimdallWeb.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ===== Services Configuration =====

// Add API Explorer and Swagger (development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Next.js frontend (localhost:3000) - CRITICAL
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Important for JWT cookies
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HeimdallWeb";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HeimdallWebUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
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
});

// Application Layer (19 handlers, 9 validators, 3 services)
builder.Services.AddApplication();

// Infrastructure Layer (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// ===== Middleware Pipeline =====

var app = builder.Build();

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HeimdallWeb API v1");
    });
}

app.UseHttpsRedirection();

// MIDDLEWARE PIPELINE - ORDER MATTERS!
app.UseCors();            // 1️⃣ CORS first
app.UseAuthentication();  // 2️⃣ Then authentication
app.UseAuthorization();   // 3️⃣ Then authorization
app.UseRateLimiter();     // 4️⃣ Rate limiting last

// ===== Endpoint Registration (5 classes) =====
app.MapAuthenticationEndpoints();
app.MapScanEndpoints();
app.MapHistoryEndpoints();
app.MapUserEndpoints();
app.MapDashboardEndpoints();

app.Run();
