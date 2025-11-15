using HeimdallWeb.Interfaces;
using HeimdallWeb.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HeimdallWeb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            // Use DbContext pooling for better throughput and enable retry on failure for transient errors
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
                {
                    mySqlOptions.CommandTimeout(90);
                }));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IFindingRepository, FindingRepository>();
            services.AddScoped<ITechnologyRepository, TechnologyRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IUserUsageRepository, UserUsageRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IScanService, ScanService>();
            return services;
        }

        public static IServiceCollection RoutesSettings(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
            });

            return services;
        }
    }
}
