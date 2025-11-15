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
        }
    }
}