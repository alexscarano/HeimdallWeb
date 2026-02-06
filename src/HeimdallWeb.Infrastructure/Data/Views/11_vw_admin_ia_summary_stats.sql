-- ============================================================
-- VIEW: vw_admin_ia_summary_stats
-- Descrição: Estatísticas gerais de findings e riscos (agregadas)
-- Uso: Dashboard do Admin - estatísticas globais de vulnerabilidades
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding para maior robustez e deriva riscos.
-- ============================================================
DROP VIEW IF EXISTS vw_admin_ia_summary_stats;

CREATE OR REPLACE VIEW vw_admin_ia_summary_stats AS
WITH ScanSummaries AS (
    -- Summarize findings per scan history
    SELECT
        h.history_id,
        h.created_date,
        COUNT(f.finding_id) AS scan_total_findings,
        MAX(f.severity) AS scan_max_severity,
        COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS scan_critical_count,
        COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS scan_high_count,
        COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS scan_medium_count,
        COUNT(f.finding_id) FILTER (WHERE f.severity = 1) AS scan_low_count,
        COUNT(f.finding_id) FILTER (WHERE f.severity = 0) AS scan_informational_count
    FROM
        tb_history h
    INNER JOIN
        tb_finding f ON h.history_id = f.history_id
    WHERE
        h.has_completed = true
    GROUP BY
        h.history_id, h.created_date
)
SELECT
    SUM(ss.scan_total_findings)::numeric AS total_findings_all_scans,
    SUM(ss.scan_critical_count)::numeric AS total_critical,
    SUM(ss.scan_high_count)::numeric AS total_high,
    SUM(ss.scan_medium_count)::numeric AS total_medium,
    SUM(ss.scan_low_count)::numeric AS total_low,
    COALESCE(AVG(ss.scan_total_findings), 0)::numeric AS avg_findings_per_scan,
    -- Riscos predominantes (based on highest finding severity in scan)
    COUNT(DISTINCT CASE WHEN ss.scan_max_severity = 4 THEN ss.history_id ELSE NULL END)::numeric AS scans_critical_risk,
    COUNT(DISTINCT CASE WHEN ss.scan_max_severity = 3 THEN ss.history_id ELSE NULL END)::numeric AS scans_high_risk,
    COUNT(DISTINCT CASE WHEN ss.scan_max_severity = 2 THEN ss.history_id ELSE NULL END)::numeric AS scans_medium_risk,
    COUNT(DISTINCT CASE WHEN ss.scan_max_severity = 1 THEN ss.history_id ELSE NULL END)::numeric AS scans_low_risk,
    COUNT(DISTINCT CASE WHEN ss.scan_max_severity = 0 THEN ss.history_id ELSE NULL END)::numeric AS scans_informational_risk,
    -- Últimas 24h (total findings per severity in last 24h)
    SUM(CASE WHEN ss.created_date >= NOW() - INTERVAL '24 hours' THEN ss.scan_critical_count ELSE 0 END)::numeric AS critical_last_24h,
    SUM(CASE WHEN ss.created_date >= NOW() - INTERVAL '24 hours' THEN ss.scan_high_count ELSE 0 END)::numeric AS high_last_24h,
    SUM(CASE WHEN ss.created_date >= NOW() - INTERVAL '24 hours' THEN ss.scan_medium_count ELSE 0 END)::numeric AS medium_last_24h,
    SUM(CASE WHEN ss.created_date >= NOW() - INTERVAL '24 hours' THEN ss.scan_low_count ELSE 0 END)::numeric AS low_last_24h,
    SUM(CASE WHEN ss.created_date >= NOW() - INTERVAL '24 hours' THEN ss.scan_informational_count ELSE 0 END)::numeric AS informational_last_24h
FROM
    ScanSummaries ss;
