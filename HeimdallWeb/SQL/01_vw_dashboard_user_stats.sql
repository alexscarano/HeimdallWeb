-- ============================================================
-- VIEW: vw_dashboard_user_stats
-- Descrição: Estatísticas gerais de usuários
-- Cache: 30 segundos (dados mudam lentamente)
-- ============================================================

USE heimdall_db;

DROP VIEW IF EXISTS vw_dashboard_user_stats;

CREATE VIEW vw_dashboard_user_stats AS
SELECT 
    COUNT(*) AS total_users,
    SUM(CASE WHEN is_active = 1 THEN 1 ELSE 0 END) AS active_users,
    SUM(CASE WHEN is_active = 0 THEN 1 ELSE 0 END) AS blocked_users,
    SUM(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) AS new_users_last_7_days,
    SUM(CASE WHEN created_at >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) AS new_users_last_30_days
FROM tb_user;

-- Verificar VIEW criada
SELECT * FROM vw_dashboard_user_stats;
