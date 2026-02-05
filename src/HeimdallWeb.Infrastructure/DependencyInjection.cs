using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using HeimdallWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HeimdallWeb.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration.
/// Registers all infrastructure services, repositories, DbContext, and external services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context (PostgreSQL with Npgsql)
        var connectionString = configuration.GetConnectionString("AppDbConnectionString")
            ?? throw new InvalidOperationException("Connection string 'AppDbConnectionString' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(30); // 30 seconds timeout
                }));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IScanHistoryRepository, ScanHistoryRepository>();
        services.AddScoped<IFindingRepository, FindingRepository>();
        services.AddScoped<ITechnologyRepository, TechnologyRepository>();
        services.AddScoped<IIASummaryRepository, IASummaryRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserUsageRepository, UserUsageRepository>();

        // Note: Security Scanners and External Services (GeminiService) will be added in Phase 3
        // after refactoring them to remove legacy dependencies

        return services;
    }
}
