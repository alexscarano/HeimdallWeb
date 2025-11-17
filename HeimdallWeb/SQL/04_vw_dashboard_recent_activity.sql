-- ============================================================
-- VIEW: vw_dashboard_recent_activity
-- Descrição: Atividades recentes do sistema (últimos 50 logs)
-- Cache: 5 segundos (muda frequentemente)
-- ============================================================
DROP VIEW IF EXISTS vw_dashboard_recent_activity;

CREATE OR REPLACE VIEW vw_dashboard_recent_activity AS
SELECT 
    l.timestamp,
    l.user_id,
    l.level,
    l.message,
    l.source,
    l.remote_ip,
    u.username
FROM tb_log l
LEFT JOIN tb_user u ON l.user_id = u.user_id
ORDER BY l.timestamp DESC
LIMIT 50;
