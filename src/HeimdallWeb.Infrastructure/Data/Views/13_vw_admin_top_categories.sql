-- ============================================================
-- VIEW: vw_admin_top_categories
-- Descrição: Categorias mais frequentes de vulnerabilidades
-- Uso: Dashboard do Admin - gráfico de categorias mais comuns
-- PostgreSQL compatible version
-- ============================================================
DROP VIEW IF EXISTS vw_admin_top_categories;

CREATE OR REPLACE VIEW vw_admin_top_categories AS
SELECT
    ia.main_category,
    COUNT(*) AS category_occurrences,
    SUM(ia.total_findings) AS total_findings_in_category,
    SUM(ia.findings_critical) AS critical_in_category,
    SUM(ia.findings_high) AS high_in_category,
    ROUND(COUNT(*)::numeric * 100.0 / (SELECT COUNT(*) FROM tb_ia_summary WHERE main_category IS NOT NULL), 2) AS percentage_of_total
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = true
  AND ia.main_category IS NOT NULL
GROUP BY ia.main_category
ORDER BY category_occurrences DESC
LIMIT 10;
