-- ============================================================
-- VIEW: vw_dashboard_scan_stats
-- Descrição: Estatísticas de scans/varreduras
-- Cache: 30 segundos
-- PostgreSQL compatible version
-- ============================================================
DROP VIEW IF EXISTS vw_dashboard_scan_stats;

CREATE OR REPLACE VIEW vw_dashboard_scan_stats AS
SELECT
    COUNT(*) AS total_scans,
    SUM(CASE WHEN created_date >= NOW() - INTERVAL '24 hours' THEN 1 ELSE 0 END) AS scans_last_24h,
    -- average duration in seconds (duration is stored as INTERVAL in PostgreSQL)
    COALESCE(AVG(EXTRACT(EPOCH FROM duration)), 0) AS avg_scan_time_s,
    ROUND(
        SUM(CASE WHEN has_completed = true THEN 1 ELSE 0 END)::numeric * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS success_rate,
    ROUND(
        SUM(CASE WHEN has_completed = false THEN 1 ELSE 0 END)::numeric * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS fail_rate
FROM tb_history;
