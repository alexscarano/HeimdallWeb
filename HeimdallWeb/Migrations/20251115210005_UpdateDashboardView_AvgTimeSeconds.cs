using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDashboardView_AvgTimeSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_scan_stats;

CREATE VIEW vw_dashboard_scan_stats AS
SELECT 
    COUNT(*) AS total_scans,
    SUM(CASE WHEN created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS scans_last_24h,
    COALESCE(AVG(TIME_TO_SEC(duration)), 0) AS avg_scan_time_s,
    ROUND(
        SUM(CASE WHEN has_completed = 1 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS success_rate,
    ROUND(
        SUM(CASE WHEN has_completed = 0 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS fail_rate
FROM tb_history;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_scan_stats;");
        }
    }
}
