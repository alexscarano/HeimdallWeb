-- ============================================================
-- VIEW: vw_admin_top_categories
-- Descrição: Categorias mais frequentes de vulnerabilidades
-- Uso: Dashboard do Admin - gráfico de categorias mais comuns
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding e usa 'type' como categoria.
-- ============================================================
DROP VIEW IF EXISTS vw_admin_top_categories;

CREATE OR REPLACE VIEW vw_admin_top_categories AS
SELECT
    f.type AS category,
    COUNT(f.finding_id) AS total_findings_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS critical_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS high_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS medium_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 1) AS low_in_category,
    ROUND(
        COUNT(f.finding_id)::numeric * 100.0 /
        (SELECT COUNT(*) FROM tb_finding),
    2) AS percentage_of_total
FROM
    tb_finding f
INNER JOIN
    tb_history h ON f.history_id = h.history_id
WHERE
    h.has_completed = true
AND
    f.type IS NOT NULL
GROUP BY
    f.type
ORDER BY
    total_findings_in_category DESC
LIMIT 10;
