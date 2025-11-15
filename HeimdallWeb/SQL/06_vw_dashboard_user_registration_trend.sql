-- ============================================================
-- VIEW: vw_dashboard_user_registration_trend
-- Descrição: Tendência de registros de usuários (últimos 30 dias)
-- Uso: Gráfico de barra (Chart.js)
-- Cache: 30 segundos
-- ============================================================

USE heimdall_db;

DROP VIEW IF EXISTS vw_dashboard_user_registration_trend;

CREATE VIEW vw_dashboard_user_registration_trend AS
SELECT 
    DATE(created_at) AS registration_date,
    COUNT(*) AS new_users
FROM tb_user
WHERE created_at >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY DATE(created_at)
ORDER BY registration_date DESC;

-- Verificar VIEW criada
SELECT * FROM vw_dashboard_user_registration_trend LIMIT 10;
