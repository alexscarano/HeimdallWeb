-- ============================================================
-- VIEW: vw_dashboard_user_registration_trend
-- Descrição: Tendência de registros de usuários (últimos 30 dias)
-- Uso: Gráfico de barra (Chart.js)
-- Cache: 30 segundos
-- PostgreSQL compatible version
-- ============================================================
DROP VIEW IF EXISTS vw_dashboard_user_registration_trend;

CREATE OR REPLACE VIEW vw_dashboard_user_registration_trend AS
SELECT
    DATE(created_at) AS registration_date,
    COUNT(*) AS new_users
FROM tb_user
WHERE created_at >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY DATE(created_at)
ORDER BY registration_date DESC;
