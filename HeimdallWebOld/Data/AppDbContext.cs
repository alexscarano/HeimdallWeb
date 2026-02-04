using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserModel> User { get; set; }
        public DbSet<HistoryModel> History { get; set; }
        public DbSet <TechnologyModel> Technology { get; set; }
        public DbSet <IASummaryModel> IASummary {  get; set; }
        public DbSet <FindingModel> Finding { get; set; }
        public DbSet<UserUsageModel> UserUsage { get; set; }
        public DbSet<LogModel> Log { get; set; }

        // Dashboard VIEWs (read-only, no keys)
        public DbSet<DashboardUserStats> DashboardUserStats { get; set; }
        public DbSet<DashboardScanStats> DashboardScanStats { get; set; }
        public DbSet<DashboardLogsOverview> DashboardLogsOverview { get; set; }
        public DbSet<DashboardRecentActivity> DashboardRecentActivity { get; set; }
        public DbSet<DashboardScanTrendDaily> DashboardScanTrendDaily { get; set; }
        public DbSet<DashboardUserRegistrationTrend> DashboardUserRegistrationTrend { get; set; }

        // User Statistics VIEWs
        public DbSet<UserScanSummary> UserScanSummary { get; set; }
        public DbSet<UserFindingsSummary> UserFindingsSummary { get; set; }
        public DbSet<UserRiskTrend> UserRiskTrend { get; set; }
        public DbSet<UserCategoryBreakdown> UserCategoryBreakdown { get; set; }

        // Admin IA Summary VIEWs
        public DbSet<AdminIASummaryStats> AdminIASummaryStats { get; set; }
        public DbSet<AdminRiskDistributionDaily> AdminRiskDistributionDaily { get; set; }
        public DbSet<AdminTopCategory> AdminTopCategory { get; set; }
        public DbSet<AdminMostVulnerableTarget> AdminMostVulnerableTarget { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new HistoryMap());
            modelBuilder.ApplyConfiguration(new TechnologyMap());
            modelBuilder.ApplyConfiguration(new IASummaryMap());
            modelBuilder.ApplyConfiguration(new FindingMap());  
            modelBuilder.ApplyConfiguration(new UserUsageMap());
            modelBuilder.ApplyConfiguration(new LogMap());

            // Mapear VIEWs SQL (sem chave primária)
            modelBuilder.Entity<DashboardUserStats>()
                .HasNoKey()
                .ToView("vw_dashboard_user_stats");

            modelBuilder.Entity<DashboardScanStats>()
                .HasNoKey()
                .ToView("vw_dashboard_scan_stats");

            modelBuilder.Entity<DashboardLogsOverview>()
                .HasNoKey()
                .ToView("vw_dashboard_logs_overview");

            modelBuilder.Entity<DashboardRecentActivity>()
                .HasNoKey()
                .ToView("vw_dashboard_recent_activity");

            modelBuilder.Entity<DashboardScanTrendDaily>()
                .HasNoKey()
                .ToView("vw_dashboard_scan_trend_daily");

            modelBuilder.Entity<DashboardUserRegistrationTrend>()
                .HasNoKey()
                .ToView("vw_dashboard_user_registration_trend");

            // Mapear User Statistics VIEWs
            modelBuilder.Entity<UserScanSummary>()
                .HasNoKey()
                .ToView("vw_user_scan_summary");

            modelBuilder.Entity<UserFindingsSummary>()
                .HasNoKey()
                .ToView("vw_user_findings_summary");

            modelBuilder.Entity<UserRiskTrend>()
                .HasNoKey()
                .ToView("vw_user_risk_trend");

            modelBuilder.Entity<UserCategoryBreakdown>()
                .HasNoKey()
                .ToView("vw_user_category_breakdown");

            // Mapear Admin IA Summary VIEWs
            modelBuilder.Entity<AdminIASummaryStats>()
                .HasNoKey()
                .ToView("vw_admin_ia_summary_stats");

            modelBuilder.Entity<AdminRiskDistributionDaily>()
                .HasNoKey()
                .ToView("vw_admin_risk_distribution_daily");

            modelBuilder.Entity<AdminTopCategory>()
                .HasNoKey()
                .ToView("vw_admin_top_categories");

            modelBuilder.Entity<AdminMostVulnerableTarget>()
                .HasNoKey()
                .ToView("vw_admin_most_vulnerable_targets");
        }
    }
}