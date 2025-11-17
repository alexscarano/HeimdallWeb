-- ============================================================
-- VIEW: vw_dashboard_logs_overview
-- Descrição: Visão geral dos logs do sistema
-- Cache: 30 segundos
-- ============================================================
DROP VIEW IF EXISTS vw_dashboard_logs_overview;

CREATE OR REPLACE VIEW vw_dashboard_logs_overview AS
SELECT 
    COUNT(*) AS total_logs,
    SUM(CASE WHEN DATE(timestamp) = CURDATE() THEN 1 ELSE 0 END) AS logs_today,
    SUM(CASE WHEN level = 'ERROR' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_errors_last_24h,
    SUM(CASE WHEN level = 'WARNING' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_warn_last_24h,
    SUM(CASE WHEN level = 'INFO' AND timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN 1 ELSE 0 END) AS logs_info_last_24h
FROM tb_log;
