-- ============================================================
-- VIEW: vw_dashboard_scan_stats
-- Descrição: Estatísticas de scans/varreduras
-- Cache: 30 segundos
-- ============================================================

USE heimdall_db;

DROP VIEW IF EXISTS vw_dashboard_scan_stats;

CREATE VIEW vw_dashboard_scan_stats AS
SELECT 
    COUNT(*) AS total_scans,
    SUM(CASE WHEN created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS scans_last_24h,
    COALESCE(AVG(TIMESTAMPDIFF(MICROSECOND, created_date, DATE_ADD(created_date, INTERVAL duration MICROSECOND)) / 1000), 0) AS avg_scan_time_ms,
    ROUND(
        SUM(CASE WHEN has_completed = 1 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS success_rate,
    ROUND(
        SUM(CASE WHEN has_completed = 0 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0),
        2
    ) AS fail_rate
FROM tb_history;

-- Verificar VIEW criada
SELECT * FROM vw_dashboard_scan_stats;
