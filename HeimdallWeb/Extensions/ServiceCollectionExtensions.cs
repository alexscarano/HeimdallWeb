using HeimdallWeb.Repository.Interfaces;
using HeimdallWeb.Services;
using HeimdallWeb.Services.Interfaces;

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
                    // Enable retry on failure for transient faults
                    mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IFindingRepository, FindingRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IScanService, ScanService>();
            return services;
        }
    }
}
