-- ============================================================
-- VIEW: vw_admin_risk_distribution_daily
-- Descrição: Distribuição de riscos por dia (últimos 30 dias)
-- Uso: Dashboard do Admin - gráfico de tendência de vulnerabilidades
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding para maior robustez.
-- ============================================================
DROP VIEW IF EXISTS vw_admin_risk_distribution_daily;

CREATE OR REPLACE VIEW vw_admin_risk_distribution_daily AS
SELECT
    h.created_date::date AS risk_date,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS critical_findings,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS high_findings,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS medium_findings,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 1) AS low_findings,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 0) AS informational_findings,
    COUNT(DISTINCT f.history_id) AS total_scans
FROM
    tb_finding f
INNER JOIN
    tb_history h ON f.history_id = h.history_id
WHERE
    h.has_completed = true
AND
    h.created_date >= (CURRENT_DATE - INTERVAL '30 days')
GROUP BY
    h.created_date::date
ORDER BY
    risk_date DESC;
