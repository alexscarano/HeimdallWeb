-- ============================================================
-- VIEW: vw_admin_most_vulnerable_targets
-- Descrição: Targets com mais vulnerabilidades detectadas
-- Uso: Dashboard do Admin - lista de targets mais críticos
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding para maior robustez.
-- ============================================================
DROP VIEW IF EXISTS vw_admin_most_vulnerable_targets;

CREATE OR REPLACE VIEW vw_admin_most_vulnerable_targets AS
SELECT
    h.target,
    COUNT(DISTINCT f.history_id) AS scan_count,
    COUNT(f.finding_id) AS total_findings,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS total_critical,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS total_high,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS total_medium,
    MAX(f.severity) AS highest_risk_level, -- Returns the numeric severity level (e.g., 4 for Critical)
    MAX(h.created_date) AS last_scan_date
FROM
    tb_finding f
INNER JOIN
    tb_history h ON f.history_id = h.history_id
WHERE
    h.has_completed = true
GROUP BY
    h.target
HAVING
    COUNT(f.finding_id) > 0
ORDER BY
    total_critical DESC, total_high DESC, total_findings DESC
LIMIT 20;
