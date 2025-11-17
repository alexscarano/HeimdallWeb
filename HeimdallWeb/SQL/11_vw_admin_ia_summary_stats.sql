-- ============================================================
-- VIEW: vw_admin_ia_summary_stats
-- Descrição: Estatísticas gerais de findings e riscos (agregadas)
-- Uso: Dashboard do Admin - estatísticas globais de vulnerabilidades
-- ============================================================
DROP VIEW IF EXISTS vw_admin_ia_summary_stats;

CREATE OR REPLACE VIEW vw_admin_ia_summary_stats AS
SELECT 
    SUM(ia.total_findings) AS total_findings_all_scans,
    SUM(ia.findings_critical) AS total_critical,
    SUM(ia.findings_high) AS total_high,
    SUM(ia.findings_medium) AS total_medium,
    SUM(ia.findings_low) AS total_low,
    COALESCE(AVG(ia.total_findings), 0) AS avg_findings_per_scan,
    -- Riscos predominantes
    SUM(CASE WHEN ia.overall_risk = 'Critico' THEN 1 ELSE 0 END) AS scans_critical_risk,
    SUM(CASE WHEN ia.overall_risk = 'Alto' THEN 1 ELSE 0 END) AS scans_high_risk,
    SUM(CASE WHEN ia.overall_risk = 'Medio' THEN 1 ELSE 0 END) AS scans_medium_risk,
    SUM(CASE WHEN ia.overall_risk = 'Baixo' THEN 1 ELSE 0 END) AS scans_low_risk,
    -- Últimas 24h
    SUM(CASE WHEN ia.created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN ia.findings_critical ELSE 0 END) AS critical_last_24h,
    SUM(CASE WHEN ia.created_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) THEN ia.findings_high ELSE 0 END) AS high_last_24h
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1;