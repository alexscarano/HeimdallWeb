-- ============================================================
-- VIEW: vw_user_risk_trend
-- Descrição: Tendência de riscos ao longo do tempo para usuário
-- Uso: Dashboard do usuário - gráfico de evolução de riscos
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding e usa sintaxe PostgreSQL.
-- ============================================================
DROP VIEW IF EXISTS vw_user_risk_trend;

CREATE OR REPLACE VIEW vw_user_risk_trend AS
SELECT
    h.user_id,
    h.created_date::date AS risk_date,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS critical_count,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS high_count,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS medium_count,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 1) AS low_count,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 0) AS informational_count,
    COUNT(DISTINCT f.history_id) AS scans_on_date
FROM
    tb_finding f
INNER JOIN
    tb_history h ON f.history_id = h.history_id
WHERE
    h.has_completed = true
AND
    h.created_date >= (CURRENT_DATE - INTERVAL '30 days')
GROUP BY
    h.user_id, h.created_date::date
ORDER BY
    h.user_id, risk_date DESC;
