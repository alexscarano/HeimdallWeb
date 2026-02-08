using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Entities.Views;
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

    // Read-only DbSets for SQL VIEWs
    public DbSet<UserRiskTrend> UserRiskTrends { get; set; } = null!;
    public DbSet<UserCategoryBreakdown> UserCategoryBreakdowns { get; set; } = null!;

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

        // SQL VIEWs - mapped as keyless entities (read-only)
        modelBuilder.Entity<UserRiskTrend>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_user_risk_trend");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RiskDate).HasColumnName("risk_date");
            entity.Property(e => e.CriticalCount).HasColumnName("critical_count");
            entity.Property(e => e.HighCount).HasColumnName("high_count");
            entity.Property(e => e.MediumCount).HasColumnName("medium_count");
            entity.Property(e => e.LowCount).HasColumnName("low_count");
            entity.Property(e => e.InformationalCount).HasColumnName("informational_count");
            entity.Property(e => e.ScansOnDate).HasColumnName("scans_on_date");
        });

        modelBuilder.Entity<UserCategoryBreakdown>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_user_category_breakdown");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.CategoryCount).HasColumnName("category_count");
            entity.Property(e => e.CriticalInCategory).HasColumnName("critical_in_category");
            entity.Property(e => e.HighInCategory).HasColumnName("high_in_category");
            entity.Property(e => e.MediumInCategory).HasColumnName("medium_in_category");
            entity.Property(e => e.LowInCategory).HasColumnName("low_in_category");
            entity.Property(e => e.InformationalInCategory).HasColumnName("informational_in_category");
        });
    }
}
