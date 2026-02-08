using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Entities.Views;
using HeimdallWeb.Infrastructure.Data.Configurations;
using HeimdallWeb.Infrastructure.Data.Views.ViewModels;
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

    // Read-only DbSets for SQL VIEWs (14 total)
    // Dashboard VIEWs (6)
    public DbSet<DashboardUserStats> DashboardUserStats { get; set; } = null!;
    public DbSet<DashboardScanStats> DashboardScanStats { get; set; } = null!;
    public DbSet<DashboardLogsOverview> DashboardLogsOverview { get; set; } = null!;
    public DbSet<DashboardRecentActivity> DashboardRecentActivity { get; set; } = null!;
    public DbSet<DashboardScanTrendDaily> DashboardScanTrendDaily { get; set; } = null!;
    public DbSet<DashboardUserRegistrationTrend> DashboardUserRegistrationTrend { get; set; } = null!;
    
    // User-specific VIEWs (4)
    public DbSet<UserScanSummary> UserScanSummaries { get; set; } = null!;
    public DbSet<UserFindingsSummary> UserFindingsSummaries { get; set; } = null!;
    public DbSet<UserRiskTrend> UserRiskTrends { get; set; } = null!;
    public DbSet<UserCategoryBreakdown> UserCategoryBreakdowns { get; set; } = null!;
    
    // Admin VIEWs (4)
    public DbSet<AdminIASummaryStats> AdminIASummaryStats { get; set; } = null!;
    public DbSet<AdminRiskDistributionDaily> AdminRiskDistributionDaily { get; set; } = null!;
    public DbSet<AdminTopCategories> AdminTopCategories { get; set; } = null!;
    public DbSet<AdminMostVulnerableTargets> AdminMostVulnerableTargets { get; set; } = null!;

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
        
        // 1. vw_dashboard_user_stats
        modelBuilder.Entity<DashboardUserStats>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_dashboard_user_stats");
            entity.Property(e => e.TotalUsers).HasColumnName("total_users");
            entity.Property(e => e.ActiveUsers).HasColumnName("active_users");
            entity.Property(e => e.BlockedUsers).HasColumnName("blocked_users");
            entity.Property(e => e.NewUsersLast7Days).HasColumnName("new_users_last_7_days");
            entity.Property(e => e.NewUsersLast30Days).HasColumnName("new_users_last_30_days");
        });
        
        // 2. vw_dashboard_scan_stats
        modelBuilder.Entity<DashboardScanStats>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_dashboard_scan_stats");
            entity.Property(e => e.TotalScans).HasColumnName("total_scans");
            entity.Property(e => e.ScansLast24h).HasColumnName("scans_last_24h");
            entity.Property(e => e.AvgScanTimeS).HasColumnName("avg_scan_time_s");
            entity.Property(e => e.SuccessRate).HasColumnName("success_rate");
            entity.Property(e => e.FailRate).HasColumnName("fail_rate");
        });
        
        // 3. vw_dashboard_logs_overview
        modelBuilder.Entity<DashboardLogsOverview>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_dashboard_logs_overview");
            entity.Property(e => e.TotalLogs).HasColumnName("total_logs");
            entity.Property(e => e.LogsToday).HasColumnName("logs_today");
            entity.Property(e => e.LogsErrorsLast24h).HasColumnName("logs_errors_last_24h");
            entity.Property(e => e.LogsWarnLast24h).HasColumnName("logs_warn_last_24h");
            entity.Property(e => e.LogsInfoLast24h).HasColumnName("logs_info_last_24h");
        });
        
        // 4. vw_dashboard_recent_activity
        modelBuilder.Entity<DashboardRecentActivity>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_dashboard_recent_activity");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.RemoteIp).HasColumnName("remote_ip");
            entity.Property(e => e.Username).HasColumnName("username");
        });
        
        // 5. vw_dashboard_scan_trend_daily
        modelBuilder.Entity<DashboardScanTrendDaily>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_dashboard_scan_trend_daily");
            entity.Property(e => e.ScanDate).HasColumnName("scan_date");
            entity.Property(e => e.ScanCount).HasColumnName("scan_count");
            entity.Property(e => e.SuccessfulScans).HasColumnName("successful_scans");
            entity.Property(e => e.FailedScans).HasColumnName("failed_scans");
        });
        
        // 6. vw_dashboard_user_registration_trend
        modelBuilder.Entity<DashboardUserRegistrationTrend>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_dashboard_user_registration_trend");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.NewUsers).HasColumnName("new_users");
        });
        
        // 7. vw_user_scan_summary
        modelBuilder.Entity<UserScanSummary>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_user_scan_summary");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TotalScans).HasColumnName("total_scans");
            entity.Property(e => e.CompletedScans).HasColumnName("completed_scans");
            entity.Property(e => e.FailedScans).HasColumnName("failed_scans");
            entity.Property(e => e.UniqueTargets).HasColumnName("unique_targets");
            entity.Property(e => e.AvgScanDurationSeconds).HasColumnName("avg_scan_duration_seconds");
            entity.Property(e => e.LastScanDate).HasColumnName("last_scan_date");
            entity.Property(e => e.ScansLast7Days).HasColumnName("scans_last_7_days");
            entity.Property(e => e.ScansLast30Days).HasColumnName("scans_last_30_days");
        });
        
        // 8. vw_user_findings_summary
        modelBuilder.Entity<UserFindingsSummary>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_user_findings_summary");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TotalFindings).HasColumnName("total_findings");
            entity.Property(e => e.TotalCritical).HasColumnName("total_critical");
            entity.Property(e => e.TotalHigh).HasColumnName("total_high");
            entity.Property(e => e.TotalMedium).HasColumnName("total_medium");
            entity.Property(e => e.TotalLow).HasColumnName("total_low");
            entity.Property(e => e.TotalInformational).HasColumnName("total_informational");
            entity.Property(e => e.PercentCritical).HasColumnName("percent_critical");
            entity.Property(e => e.PercentHigh).HasColumnName("percent_high");
            entity.Property(e => e.PercentMedium).HasColumnName("percent_medium");
            entity.Property(e => e.PercentLow).HasColumnName("percent_low");
            entity.Property(e => e.PercentInformational).HasColumnName("percent_informational");
        });
        
        // 9. vw_user_risk_trend
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

        // 10. vw_user_category_breakdown
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
        
        // 11. vw_admin_ia_summary_stats
        modelBuilder.Entity<AdminIASummaryStats>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_admin_ia_summary_stats");
            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.ScanTotalFindings).HasColumnName("scan_total_findings");
            entity.Property(e => e.ScanMaxSeverity).HasColumnName("scan_max_severity");
            entity.Property(e => e.ScanCriticalCount).HasColumnName("scan_critical_count");
            entity.Property(e => e.ScanHighCount).HasColumnName("scan_high_count");
            entity.Property(e => e.ScanMediumCount).HasColumnName("scan_medium_count");
            entity.Property(e => e.ScanLowCount).HasColumnName("scan_low_count");
            entity.Property(e => e.ScanInformationalCount).HasColumnName("scan_informational_count");
        });
        
        // 12. vw_admin_risk_distribution_daily
        modelBuilder.Entity<AdminRiskDistributionDaily>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_admin_risk_distribution_daily");
            entity.Property(e => e.RiskDate).HasColumnName("risk_date");
            entity.Property(e => e.CriticalFindings).HasColumnName("critical_findings");
            entity.Property(e => e.HighFindings).HasColumnName("high_findings");
            entity.Property(e => e.MediumFindings).HasColumnName("medium_findings");
            entity.Property(e => e.LowFindings).HasColumnName("low_findings");
            entity.Property(e => e.InformationalFindings).HasColumnName("informational_findings");
            entity.Property(e => e.TotalScans).HasColumnName("total_scans");
        });
        
        // 13. vw_admin_top_categories
        modelBuilder.Entity<AdminTopCategories>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_admin_top_categories");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.TotalFindingsInCategory).HasColumnName("total_findings_in_category");
            entity.Property(e => e.CriticalInCategory).HasColumnName("critical_in_category");
            entity.Property(e => e.HighInCategory).HasColumnName("high_in_category");
            entity.Property(e => e.MediumInCategory).HasColumnName("medium_in_category");
            entity.Property(e => e.LowInCategory).HasColumnName("low_in_category");
            entity.Property(e => e.PercentageOfTotal).HasColumnName("percentage_of_total");
        });
        
        // 14. vw_admin_most_vulnerable_targets
        modelBuilder.Entity<AdminMostVulnerableTargets>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_admin_most_vulnerable_targets");
            entity.Property(e => e.Target).HasColumnName("target");
            entity.Property(e => e.ScanCount).HasColumnName("scan_count");
            entity.Property(e => e.TotalFindings).HasColumnName("total_findings");
            entity.Property(e => e.TotalCritical).HasColumnName("total_critical");
            entity.Property(e => e.TotalHigh).HasColumnName("total_high");
            entity.Property(e => e.TotalMedium).HasColumnName("total_medium");
            entity.Property(e => e.HighestRiskLevel).HasColumnName("highest_risk_level");
            entity.Property(e => e.LastScanDate).HasColumnName("last_scan_date");
        });
    }
}
