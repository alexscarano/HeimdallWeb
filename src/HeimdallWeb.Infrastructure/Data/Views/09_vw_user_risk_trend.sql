-- ============================================================
-- VIEW: vw_user_risk_trend
-- Descrição: Tendência de riscos ao longo do tempo para usuário
-- Uso: Dashboard do usuário - gráfico de evolução de riscos
-- PostgreSQL compatible version
-- ============================================================
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
WHERE h.has_completed = true
  AND ia.created_date >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY h.user_id, DATE(ia.created_date)
ORDER BY h.user_id, risk_date DESC;
