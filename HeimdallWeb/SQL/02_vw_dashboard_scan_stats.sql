-- ============================================================
-- VIEW: vw_dashboard_scan_stats
-- Descrição: Estatísticas de scans/varreduras
-- Cache: 30 segundos
-- ============================================================
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
