using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Data;

/// <summary>
/// Application Database Context for HeimdallWeb.
/// Uses PostgreSQL with Npgsql provider.
/// Applies all entity configurations from Configurations/ directory.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ScanHistory> ScanHistories { get; set; } = null!;
    public DbSet<Finding> Findings { get; set; } = null!;
    public DbSet<Technology> Technologies { get; set; } = null!;
    public DbSet<IASummary> IASummaries { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<UserUsage> UserUsages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ScanHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new FindingConfiguration());
        modelBuilder.ApplyConfiguration(new TechnologyConfiguration());
        modelBuilder.ApplyConfiguration(new IASummaryConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new UserUsageConfiguration());

        // SQL VIEWs will be added manually later after PostgreSQL conversion
        // For now, only tables are configured
    }
}
