using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    public partial class CreateDashboardViews_20251115 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // vw_dashboard_user_stats
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_user_stats;

CREATE VIEW vw_dashboard_user_stats AS
SELECT 
    COUNT(*) AS total_users,
    SUM(CASE WHEN is_active = 1 THEN 1 ELSE 0 END) AS active_users,
    SUM(CASE WHEN is_active = 0 THEN 1 ELSE 0 END) AS blocked_users,
    SUM(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) AS new_users_last_7_days,
    SUM(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) AS new_users_last_30_days
FROM tb_user;");

            // vw_dashboard_scan_stats
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_scan_stats;

CREATE VIEW vw_dashboard_scan_stats AS
SELECT 
    COUNT(*) AS total_scans,
    SUM(CASE WHEN created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS scans_last_24h,
    // average duration in seconds (duration is stored as TIME)
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

            // vw_dashboard_logs_overview
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_logs_overview;

CREATE VIEW vw_dashboard_logs_overview AS
SELECT 
    COUNT(*) AS total_logs,
    SUM(CASE WHEN DATE(timestamp) = CURDATE() THEN 1 ELSE 0 END) AS logs_today,
    SUM(CASE WHEN level = 'ERROR' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_errors_last_24h,
    SUM(CASE WHEN level = 'WARNING' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_warn_last_24h,
    SUM(CASE WHEN level = 'INFO' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_info_last_24h
FROM tb_log;");

            // vw_dashboard_recent_activity
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_recent_activity;

CREATE VIEW vw_dashboard_recent_activity AS
SELECT 
    l.timestamp,
    l.user_id,
    l.level,
    l.message,
    l.source,
    l.remote_ip,
    u.username
FROM tb_log l
LEFT JOIN tb_user u ON l.user_id = u.user_id
ORDER BY l.timestamp DESC
LIMIT 50;");

            // vw_dashboard_scan_trend_daily
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_scan_trend_daily;

CREATE VIEW vw_dashboard_scan_trend_daily AS
SELECT 
    DATE(created_date) AS scan_date,
    COUNT(*) AS scan_count,
    SUM(CASE WHEN has_completed = 1 THEN 1 ELSE 0 END) AS successful_scans,
    SUM(CASE WHEN has_completed = 0 THEN 1 ELSE 0 END) AS failed_scans
FROM tb_history
WHERE created_date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(created_date)
ORDER BY scan_date DESC;");

            // vw_dashboard_user_registration_trend
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_user_registration_trend;

CREATE VIEW vw_dashboard_user_registration_trend AS
SELECT 
    DATE(created_at) AS registration_date,
    COUNT(*) AS new_users
FROM tb_user
WHERE created_at >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(created_at)
ORDER BY registration_date DESC;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_user_registration_trend;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_scan_trend_daily;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_recent_activity;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_logs_overview;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_scan_stats;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_dashboard_user_stats;");
        }
    }
}
