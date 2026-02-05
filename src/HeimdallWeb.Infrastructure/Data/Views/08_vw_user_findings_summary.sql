-- ============================================================
-- VIEW: vw_user_findings_summary
-- Descrição: Resumo de findings por severidade para um usuário
-- Uso: Dashboard do usuário - contagem de vulnerabilidades
-- PostgreSQL compatible version
-- ============================================================
DROP VIEW IF EXISTS vw_user_findings_summary;

CREATE OR REPLACE VIEW vw_user_findings_summary AS
SELECT
    h.user_id,
    SUM(ia.total_findings) AS total_findings,
    SUM(ia.findings_critical) AS total_critical,
    SUM(ia.findings_high) AS total_high,
    SUM(ia.findings_medium) AS total_medium,
    SUM(ia.findings_low) AS total_low,
    -- Porcentagens
    ROUND(SUM(ia.findings_critical)::numeric * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_critical,
    ROUND(SUM(ia.findings_high)::numeric * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_high,
    ROUND(SUM(ia.findings_medium)::numeric * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_medium,
    ROUND(SUM(ia.findings_low)::numeric * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_low
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = true
GROUP BY h.user_id;
