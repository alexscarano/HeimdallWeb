-- ============================================================
-- VIEW: vw_user_findings_summary
-- Descrição: Resumo de findings por severidade para um usuário
-- Uso: Dashboard do usuário - contagem de vulnerabilidades
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding para maior robustez.
-- ============================================================
DROP VIEW IF EXISTS vw_user_findings_summary;

CREATE OR REPLACE VIEW vw_user_findings_summary AS
SELECT
    h.user_id,
    COUNT(f.finding_id) AS total_findings,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS total_critical,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS total_high,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS total_medium,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 1) AS total_low,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 0) AS total_informational,
    -- Porcentagens
    ROUND( (COUNT(f.finding_id) FILTER (WHERE f.severity = 4))::numeric * 100.0 / NULLIF(COUNT(f.finding_id), 0), 2) AS percent_critical,
    ROUND( (COUNT(f.finding_id) FILTER (WHERE f.severity = 3))::numeric * 100.0 / NULLIF(COUNT(f.finding_id), 0), 2) AS percent_high,
    ROUND( (COUNT(f.finding_id) FILTER (WHERE f.severity = 2))::numeric * 100.0 / NULLIF(COUNT(f.finding_id), 0), 2) AS percent_medium,
    ROUND( (COUNT(f.finding_id) FILTER (WHERE f.severity = 1))::numeric * 100.0 / NULLIF(COUNT(f.finding_id), 0), 2) AS percent_low,
    ROUND( (COUNT(f.finding_id) FILTER (WHERE f.severity = 0))::numeric * 100.0 / NULLIF(COUNT(f.finding_id), 0), 2) AS percent_informational
FROM
    tb_finding f
INNER JOIN
    tb_history h ON f.history_id = h.history_id
WHERE
    h.has_completed = true
GROUP BY
    h.user_id;


select * from tb_ tf 
