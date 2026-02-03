-- ============================================================
-- VIEW: vw_admin_most_vulnerable_targets
-- Descrição: Targets com mais vulnerabilidades detectadas
-- Uso: Dashboard do Admin - lista de targets mais críticos
-- ============================================================
DROP VIEW IF EXISTS vw_admin_most_vulnerable_targets;

CREATE OR REPLACE VIEW vw_admin_most_vulnerable_targets AS
SELECT 
    h.target,
    COUNT(DISTINCT h.history_id) AS scan_count,
    SUM(ia.total_findings) AS total_findings,
    SUM(ia.findings_critical) AS total_critical,
    SUM(ia.findings_high) AS total_high,
    MAX(ia.overall_risk) AS highest_risk_level,
    MAX(ia.created_date) AS last_scan_date
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
GROUP BY h.target
HAVING SUM(ia.total_findings) > 0
ORDER BY total_critical DESC, total_high DESC, total_findings DESC
LIMIT 20;
