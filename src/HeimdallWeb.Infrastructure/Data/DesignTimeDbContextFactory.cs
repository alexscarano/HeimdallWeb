using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HeimdallWeb.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating AppDbContext instances during EF Core migrations.
/// This is required for commands like: dotnet ef migrations add, dotnet ef database update.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Default connection string for development
        // This will be overridden by actual configuration at runtime
        var connectionString = "Host=localhost;Database=heimdallweb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
