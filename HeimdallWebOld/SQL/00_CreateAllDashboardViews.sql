-- ============================================================
-- Script Master - Executar todas as VIEWs do Dashboard
-- ============================================================
-- Este script executa todos os scripts de VIEWs em ordem.
-- Execute este arquivo para criar todas as VIEWs de uma vez.
--
-- VIEWs para Dashboard Admin (gerais):
-- 01_vw_dashboard_user_stats.sql
-- 02_vw_dashboard_scan_stats.sql
-- 03_vw_dashboard_logs_overview.sql
-- 04_vw_dashboard_recent_activity.sql
-- 05_vw_dashboard_scan_trend_daily.sql
-- 06_vw_dashboard_user_registration_trend.sql
--
-- VIEWs para Estatísticas do Usuário (individuais):
-- 07_vw_user_scan_summary.sql
-- 08_vw_user_findings_summary.sql
-- 09_vw_user_risk_trend.sql
-- 10_vw_user_category_breakdown.sql
--
-- VIEWs para Dashboard Admin (dados de IA Summary):
-- 11_vw_admin_ia_summary_stats.sql
-- 12_vw_admin_risk_distribution_daily.sql
-- 13_vw_admin_top_categories.sql
-- 14_vw_admin_most_vulnerable_targets.sql
-- ============================================================

USE db_heimdall;

-- ==========================
-- 1. vw_dashboard_user_stats
-- ==========================
DROP VIEW IF EXISTS vw_dashboard_user_stats;

CREATE OR REPLACE VIEW vw_dashboard_user_stats AS
SELECT 
    COUNT(*) AS total_users,
    SUM(CASE WHEN is_active = 1 THEN 1 ELSE 0 END) AS active_users,
    SUM(CASE WHEN is_active = 0 THEN 1 ELSE 0 END) AS blocked_users,
    SUM(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) AS new_users_last_7_days,
    SUM(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) AS new_users_last_30_days
FROM tb_user;


-- ==========================
-- 2. vw_dashboard_scan_stats
-- ==========================
DROP VIEW IF EXISTS vw_dashboard_scan_stats;

CREATE OR REPLACE VIEW vw_dashboard_scan_stats AS
SELECT 
    COUNT(*) AS total_scans,
    SUM(CASE WHEN created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS scans_last_24h,
    -- average duration in seconds (duration is stored as TIME)
    COALESCE(AVG(TIME_TO_SEC(duration)), 0) AS avg_scan_time_s,
    ROUND(
        SUM(CASE WHEN has_completed = 1 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS success_rate,
    ROUND(
        SUM(CASE WHEN has_completed = 0 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS fail_rate
FROM tb_history;

-- =============================
-- 3. vw_dashboard_logs_overview
-- =============================
DROP VIEW IF EXISTS vw_dashboard_logs_overview;

CREATE OR REPLACE VIEW vw_dashboard_logs_overview AS
SELECT 
    COUNT(*) AS total_logs,
    SUM(CASE WHEN DATE(timestamp) = CURDATE() THEN 1 ELSE 0 END) AS logs_today,
    SUM(CASE WHEN level = 'ERROR' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_errors_last_24h,
    SUM(CASE WHEN level = 'WARNING' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_warn_last_24h,
    SUM(CASE WHEN level = 'INFO' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_info_last_24h
FROM tb_log;


-- =================================
-- 4. vw_dashboard_recent_activity
-- =================================
DROP VIEW IF EXISTS vw_dashboard_recent_activity;

CREATE OR REPLACE VIEW vw_dashboard_recent_activity AS
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
LIMIT 50;

-- ==================================
-- 5. vw_dashboard_scan_trend_daily
-- ==================================
DROP VIEW IF EXISTS vw_dashboard_scan_trend_daily;

CREATE OR REPLACE VIEW vw_dashboard_scan_trend_daily AS
SELECT 
    DATE(created_date) AS scan_date,
    COUNT(*) AS scan_count,
    SUM(CASE WHEN has_completed = 1 THEN 1 ELSE 0 END) AS successful_scans,
    SUM(CASE WHEN has_completed = 0 THEN 1 ELSE 0 END) AS failed_scans
FROM tb_history
WHERE created_date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(created_date)
ORDER BY scan_date DESC;

-- ==========================================
-- 6. vw_dashboard_user_registration_trend
-- ==========================================
DROP VIEW IF EXISTS vw_dashboard_user_registration_trend;

CREATE OR REPLACE VIEW vw_dashboard_user_registration_trend AS
SELECT 
    DATE(created_at) AS registration_date,
    COUNT(*) AS new_users
FROM tb_user
WHERE created_at >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(created_at)
ORDER BY registration_date DESC;

-- ==================================
-- 7. vw_user_scan_summary
-- ==================================
DROP VIEW IF EXISTS vw_user_scan_summary;

CREATE OR REPLACE VIEW vw_user_scan_summary AS
SELECT 
    h.user_id,
    COUNT(DISTINCT h.history_id) AS total_scans,
    SUM(CASE WHEN h.has_completed = 1 THEN 1 ELSE 0 END) AS completed_scans,
    SUM(CASE WHEN h.has_completed = 0 THEN 1 ELSE 0 END) AS failed_scans,
    COUNT(DISTINCT h.target) AS unique_targets,
    COALESCE(AVG(CASE WHEN h.has_completed = 1 THEN TIME_TO_SEC(h.duration) END), 0) AS avg_scan_duration_seconds,
    MAX(h.created_date) AS last_scan_date,
    SUM(CASE WHEN h.created_date >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) AS scans_last_7_days,
    SUM(CASE WHEN h.created_date >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) AS scans_last_30_days
FROM tb_history h
GROUP BY h.user_id;

-- ==================================
-- 8. vw_user_findings_summary
-- ==================================
DROP VIEW IF EXISTS vw_user_findings_summary;

CREATE OR REPLACE VIEW vw_user_findings_summary AS
SELECT 
    h.user_id,
    SUM(ia.total_findings) AS total_findings,
    SUM(ia.findings_critical) AS total_critical,
    SUM(ia.findings_high) AS total_high,
    SUM(ia.findings_medium) AS total_medium,
    SUM(ia.findings_low) AS total_low,
    ROUND(SUM(ia.findings_critical) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_critical,
    ROUND(SUM(ia.findings_high) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_high,
    ROUND(SUM(ia.findings_medium) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_medium,
    ROUND(SUM(ia.findings_low) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_low
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
GROUP BY h.user_id;

-- ==================================
-- 9. vw_user_risk_trend
-- ==================================
DROP VIEW IF EXISTS vw_user_risk_trend;

CREATE OR REPLACE VIEW vw_user_risk_trend AS
SELECT 
    h.user_id,
    DATE(ia.created_date) AS risk_date,
    SUM(ia.findings_critical) AS critical_count,
    SUM(ia.findings_high) AS high_count,
    SUM(ia.findings_medium) AS medium_count,
    SUM(ia.findings_low) AS low_count,
    COUNT(DISTINCT ia.ia_summary_id) AS scans_on_date
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
  AND ia.created_date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY h.user_id, DATE(ia.created_date)
ORDER BY h.user_id, risk_date DESC;

-- ==================================
-- 10. vw_user_category_breakdown
-- ==================================
DROP VIEW IF EXISTS vw_user_category_breakdown;

CREATE OR REPLACE VIEW vw_user_category_breakdown AS
SELECT 
    h.user_id,
    ia.main_category,
    COUNT(*) AS category_count,
    SUM(ia.findings_critical) AS critical_in_category,
    SUM(ia.findings_high) AS high_in_category,
    SUM(ia.findings_medium) AS medium_in_category,
    SUM(ia.findings_low) AS low_in_category
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
  AND ia.main_category IS NOT NULL
GROUP BY h.user_id, ia.main_category
ORDER BY h.user_id, category_count DESC;

-- ==================================
-- 11. vw_admin_ia_summary_stats
-- ==================================
DROP VIEW IF EXISTS vw_admin_ia_summary_stats;

CREATE OR REPLACE VIEW vw_admin_ia_summary_stats AS
SELECT 
    SUM(ia.total_findings) AS total_findings_all_scans,
    SUM(ia.findings_critical) AS total_critical,
    SUM(ia.findings_high) AS total_high,
    SUM(ia.findings_medium) AS total_medium,
    SUM(ia.findings_low) AS total_low,
    COALESCE(AVG(ia.total_findings), 0) AS avg_findings_per_scan,
    SUM(CASE WHEN ia.overall_risk = 'Critico' THEN 1 ELSE 0 END) AS scans_critical_risk,
    SUM(CASE WHEN ia.overall_risk = 'Alto' THEN 1 ELSE 0 END) AS scans_high_risk,
    SUM(CASE WHEN ia.overall_risk = 'Medio' THEN 1 ELSE 0 END) AS scans_medium_risk,
    SUM(CASE WHEN ia.overall_risk = 'Baixo' THEN 1 ELSE 0 END) AS scans_low_risk,
    SUM(CASE WHEN ia.created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN ia.findings_critical ELSE 0 END) AS critical_last_24h,
    SUM(CASE WHEN ia.created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN ia.findings_high ELSE 0 END) AS high_last_24h
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1;

-- ==================================
-- 12. vw_admin_risk_distribution_daily
-- ==================================
DROP VIEW IF EXISTS vw_admin_risk_distribution_daily;

CREATE OR REPLACE VIEW vw_admin_risk_distribution_daily AS
SELECT 
    DATE(ia.created_date) AS risk_date,
    SUM(ia.findings_critical) AS critical_findings,
    SUM(ia.findings_high) AS high_findings,
    SUM(ia.findings_medium) AS medium_findings,
    SUM(ia.findings_low) AS low_findings,
    COUNT(DISTINCT ia.ia_summary_id) AS total_summaries
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
  AND ia.created_date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(ia.created_date)
ORDER BY risk_date DESC;

-- ==================================
-- 13. vw_admin_top_categories
-- ==================================
DROP VIEW IF EXISTS vw_admin_top_categories;

CREATE OR REPLACE VIEW vw_admin_top_categories AS
SELECT 
    ia.main_category,
    COUNT(*) AS category_occurrences,
    SUM(ia.total_findings) AS total_findings_in_category,
    SUM(ia.findings_critical) AS critical_in_category,
    SUM(ia.findings_high) AS high_in_category,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM tb_ia_summary WHERE main_category IS NOT NULL), 2) AS percentage_of_total
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
  AND ia.main_category IS NOT NULL
GROUP BY ia.main_category
ORDER BY category_occurrences DESC
LIMIT 10;

-- ==================================
-- 14. vw_admin_most_vulnerable_targets
-- ==================================
DROP VIEW IF EXISTS vw_admin_most_vulnerable_targets;

CREATE OR REPLACE VIEW vw_admin_most_vulnerable_targets AS
SELECT 
    h.target,
    COUNT(DISTINCT h.history_id) AS scan_count,
    SUM(ia.total_findings) AS total_findings,
    SUM(ia.findings_critical) AS total_critical,
    SUM(ia.findings_high) AS total_high,
    MAX(ia.overall_risk) AS highest_risk_level,
    MAX(ia.created_date) AS last_scan_date
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
GROUP BY h.target
HAVING SUM(ia.total_findings) > 0
ORDER BY total_critical DESC, total_high DESC, total_findings DESC
LIMIT 20;

-- ============================================================
-- Verificação das VIEWs criadas
-- ============================================================
SHOW FULL TABLES WHERE TABLE_TYPE LIKE 'VIEW';
