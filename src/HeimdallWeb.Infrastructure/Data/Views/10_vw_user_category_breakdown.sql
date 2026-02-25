-- ============================================================
-- VIEW: vw_user_category_breakdown
-- Descrição: Distribuição de vulnerabilidades por categoria
-- Uso: Dashboard do usuário - gráfico de categorias
-- PostgreSQL compatible version
-- CORRIGIDO: Agora consulta diretamente tb_finding e usa 'type' como categoria.
-- ============================================================
DROP VIEW IF EXISTS vw_user_category_breakdown;

CREATE OR REPLACE VIEW vw_user_category_breakdown AS
SELECT
    h.user_id,
    f.type AS category, -- Using 'type' from tb_finding as the category
    COUNT(f.finding_id) AS category_count,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 4) AS critical_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 3) AS high_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 2) AS medium_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 1) AS low_in_category,
    COUNT(f.finding_id) FILTER (WHERE f.severity = 0) AS informational_in_category
FROM
    tb_finding f
INNER JOIN
    tb_history h ON f.history_id = h.history_id
WHERE
    h.has_completed = true
GROUP BY
    h.user_id, f.type
ORDER BY
    h.user_id, category_count DESC;
