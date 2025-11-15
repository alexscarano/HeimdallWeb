-- ============================================================
-- VIEW: vw_dashboard_scan_trend_daily
-- Descrição: Tendência de scans por dia (últimos 30 dias)
-- Uso: Gráfico de linha (Chart.js)
-- Cache: 30 segundos
-- ============================================================

USE heimdall_db;

DROP VIEW IF EXISTS vw_dashboard_scan_trend_daily;

CREATE VIEW vw_dashboard_scan_trend_daily AS
SELECT 
    DATE(created_date) AS scan_date,
    COUNT(*) AS scan_count,
    SUM(CASE WHEN has_completed = 1 THEN 1 ELSE 0 END) AS successful_scans,
    SUM(CASE WHEN has_completed = 0 THEN 1 ELSE 0 END) AS failed_scans
FROM tb_history
WHERE created_date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(created_date)
ORDER BY scan_date DESC;

-- Verificar VIEW criada
SELECT * FROM vw_dashboard_scan_trend_daily LIMIT 10;
