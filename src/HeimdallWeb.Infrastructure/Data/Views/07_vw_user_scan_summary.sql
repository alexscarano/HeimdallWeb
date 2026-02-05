-- ============================================================
-- VIEW: vw_user_scan_summary
-- Descrição: Resumo de scans para um usuário específico
-- Uso: Dashboard do usuário - estatísticas gerais
-- PostgreSQL compatible version
-- ============================================================
DROP VIEW IF EXISTS vw_user_scan_summary;

CREATE OR REPLACE VIEW vw_user_scan_summary AS
SELECT
    h.user_id,
    COUNT(DISTINCT h.history_id) AS total_scans,
    SUM(CASE WHEN h.has_completed = true THEN 1 ELSE 0 END) AS completed_scans,
    SUM(CASE WHEN h.has_completed = false THEN 1 ELSE 0 END) AS failed_scans,
    COUNT(DISTINCT h.target) AS unique_targets,
    COALESCE(AVG(CASE WHEN h.has_completed = true THEN EXTRACT(EPOCH FROM h.duration) END), 0) AS avg_scan_duration_seconds,
    MAX(h.created_date) AS last_scan_date,
    SUM(CASE WHEN h.created_date >= NOW() - INTERVAL '7 days' THEN 1 ELSE 0 END) AS scans_last_7_days,
    SUM(CASE WHEN h.created_date >= NOW() - INTERVAL '30 days' THEN 1 ELSE 0 END) AS scans_last_30_days
FROM tb_history h
GROUP BY h.user_id;
