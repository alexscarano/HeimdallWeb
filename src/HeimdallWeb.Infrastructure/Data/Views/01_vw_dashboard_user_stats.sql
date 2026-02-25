-- ============================================================
-- VIEW: vw_dashboard_user_stats
-- Descrição: Estatísticas gerais de usuários
-- Cache: 30 segundos (dados mudam lentamente)
-- PostgreSQL compatible version
-- ============================================================
DROP VIEW IF EXISTS vw_dashboard_user_stats;

CREATE OR REPLACE VIEW vw_dashboard_user_stats AS
SELECT
    COUNT(*) AS total_users,
    SUM(CASE WHEN is_active = true THEN 1 ELSE 0 END) AS active_users,
    SUM(CASE WHEN is_active = false THEN 1 ELSE 0 END) AS blocked_users,
    SUM(CASE WHEN created_at >= NOW() - INTERVAL '7 days' THEN 1 ELSE 0 END) AS new_users_last_7_days,
    SUM(CASE WHEN created_at >= NOW() - INTERVAL '30 days' THEN 1 ELSE 0 END) AS new_users_last_30_days
FROM tb_user;
