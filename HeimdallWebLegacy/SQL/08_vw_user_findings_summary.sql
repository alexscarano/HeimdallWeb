-- ============================================================
-- VIEW: vw_user_findings_summary
-- Descrição: Resumo de findings por severidade para um usuário
-- Uso: Dashboard do usuário - contagem de vulnerabilidades
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
    ROUND(SUM(ia.findings_critical) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_critical,
    ROUND(SUM(ia.findings_high) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_high,
    ROUND(SUM(ia.findings_medium) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_medium,
    ROUND(SUM(ia.findings_low) * 100.0 / NULLIF(SUM(ia.total_findings), 0), 2) AS percent_low
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
GROUP BY h.user_id;
