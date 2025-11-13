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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new HistoryMap());
            modelBuilder.ApplyConfiguration(new TechnologyMap());
            modelBuilder.ApplyConfiguration(new IASummaryMap());
            modelBuilder.ApplyConfiguration(new FindingMap());  
            modelBuilder.ApplyConfiguration(new UserUsageMap());
            modelBuilder.ApplyConfiguration(new LogMap());
        }
    }
}