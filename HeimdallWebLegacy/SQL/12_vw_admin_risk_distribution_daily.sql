-- ============================================================
-- VIEW: vw_admin_risk_distribution_daily
-- Descrição: Distribuição de riscos por dia (últimos 30 dias)
-- Uso: Dashboard do Admin - gráfico de tendência de vulnerabilidades
-- ============================================================

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