-- ============================================================
-- VIEW: vw_user_category_breakdown
-- Descrição: Distribuição de vulnerabilidades por categoria
-- Uso: Dashboard do usuário - gráfico de categorias
-- ============================================================
DROP VIEW IF EXISTS vw_user_category_breakdown;

CREATE OR REPLACE VIEW vw_user_category_breakdown AS
SELECT 
    h.user_id,
    ia.main_category,
    COUNT(*) AS category_count,
    SUM(ia.findings_critical) AS critical_in_category,
    SUM(ia.findings_high) AS high_in_category,
    SUM(ia.findings_medium) AS medium_in_category,
    SUM(ia.findings_low) AS low_in_category
FROM tb_ia_summary ia
INNER JOIN tb_history h ON ia.history_id = h.history_id
WHERE h.has_completed = 1
  AND ia.main_category IS NOT NULL
GROUP BY h.user_id, ia.main_category
ORDER BY h.user_id, category_count DESC;

