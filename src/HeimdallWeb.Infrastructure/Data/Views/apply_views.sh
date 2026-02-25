#!/usr/bin/env bash
# apply_views.sh — Aplica as 14 SQL Views do HeimdallWeb no PostgreSQL
#
# Idempotente: pode ser executado múltiplas vezes sem efeitos colaterais.
# Cada arquivo .sql usa CREATE OR REPLACE VIEW, portanto views existentes
# são substituídas em vez de gerar erro.
#
# Variáveis de ambiente aceitas (todas com defaults seguros):
#   PG_HOST           — host do PostgreSQL          (default: postgres)
#   PG_PORT           — porta do PostgreSQL          (default: 5432)
#   POSTGRES_DB       — nome do banco de dados       (default: db_heimdall)
#   POSTGRES_USER     — usuário do PostgreSQL        (default: postgres)
#   POSTGRES_PASSWORD — senha do PostgreSQL          (default: postgres)

set -uo pipefail
# Nota: NÃO usamos -e aqui intencionalmente — queremos continuar após
# falhas individuais e reportar o resultado consolidado no final.

# ─── Cores ANSI ───────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

# ─── Variáveis de conexão (com defaults seguros) ─────────────────────────────
PG_HOST="${PG_HOST:-postgres}"
PG_PORT="${PG_PORT:-5432}"
POSTGRES_DB="${POSTGRES_DB:-db_heimdall}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}"

# Exporta PGPASSWORD para que psql não solicite senha interativamente
export PGPASSWORD="$POSTGRES_PASSWORD"

# ─── Localiza o diretório dos arquivos .sql ───────────────────────────────────
# O script vive junto com os .sql files — usa o próprio diretório como base.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ─── Lista ordenada das 14 views (ordem importa: sem dependências cruzadas) ───
VIEW_FILES=(
    "01_vw_dashboard_user_stats.sql"
    "02_vw_dashboard_scan_stats.sql"
    "03_vw_dashboard_logs_overview.sql"
    "04_vw_dashboard_recent_activity.sql"
    "05_vw_dashboard_scan_trend_daily.sql"
    "06_vw_dashboard_user_registration_trend.sql"
    "07_vw_user_scan_summary.sql"
    "08_vw_user_findings_summary.sql"
    "09_vw_user_risk_trend.sql"
    "10_vw_user_category_breakdown.sql"
    "11_vw_admin_ia_summary_stats.sql"
    "12_vw_admin_risk_distribution_daily.sql"
    "13_vw_admin_top_categories.sql"
    "14_vw_admin_most_vulnerable_targets.sql"
)

# ─── Contadores ───────────────────────────────────────────────────────────────
success_count=0
fail_count=0
declare -a failed_views=()

# ─── Header ───────────────────────────────────────────────────────────────────
printf "\n${BOLD}${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n"
printf "${BOLD}${CYAN} HeimdallWeb — Aplicando SQL Views${RESET}\n"
printf "${CYAN} Host: ${POSTGRES_USER}@${PG_HOST}:${PG_PORT}/${POSTGRES_DB}${RESET}\n"
printf "${BOLD}${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n\n"

# ─── Aplicação das views ──────────────────────────────────────────────────────
total=${#VIEW_FILES[@]}

for sql_file in "${VIEW_FILES[@]}"; do
    full_path="${SCRIPT_DIR}/${sql_file}"

    # Extrai o nome da view do nome do arquivo (remove prefixo numérico e extensão)
    view_name="${sql_file#*_}"          # remove "01_"
    view_name="${view_name%.sql}"       # remove ".sql"

    printf "  [%02d/%d] ${CYAN}%-45s${RESET} " \
        "$(( success_count + fail_count + 1 ))" "$total" "$view_name"

    # Verifica se o arquivo existe
    if [[ ! -f "$full_path" ]]; then
        printf "${RED}ERRO — arquivo não encontrado: %s${RESET}\n" "$full_path"
        (( fail_count++ )) || true
        failed_views+=("$view_name (arquivo ausente)")
        continue
    fi

    # Executa o SQL via psql
    # --no-psqlrc   : ignora ~/.psqlrc para execução determinística
    # --set=ON_ERROR_STOP=1 : aborta o arquivo ao primeiro erro SQL,
    #                         mas o script shell continua (capturamos o exit code)
    if psql \
        --host="$PG_HOST" \
        --port="$PG_PORT" \
        --username="$POSTGRES_USER" \
        --dbname="$POSTGRES_DB" \
        --no-psqlrc \
        --set=ON_ERROR_STOP=1 \
        --file="$full_path" \
        > /dev/null 2>&1
    then
        printf "${GREEN}OK${RESET}\n"
        (( success_count++ )) || true
    else
        # Captura a mensagem de erro para diagnóstico
        error_msg=$(psql \
            --host="$PG_HOST" \
            --port="$PG_PORT" \
            --username="$POSTGRES_USER" \
            --dbname="$POSTGRES_DB" \
            --no-psqlrc \
            --set=ON_ERROR_STOP=1 \
            --file="$full_path" \
            2>&1 | head -3)

        printf "${RED}FALHOU${RESET}\n"
        printf "         ${RED}└─ %s${RESET}\n" "$error_msg"
        (( fail_count++ )) || true
        failed_views+=("$view_name")
    fi
done

# ─── Relatório final ──────────────────────────────────────────────────────────
printf "\n${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n"
printf " Resultado: ${GREEN}${success_count}/${total} views criadas com sucesso${RESET}"

if [[ $fail_count -gt 0 ]]; then
    printf " | ${RED}${fail_count} falha(s)${RESET}\n"
    printf "\n ${YELLOW}Views com falha:${RESET}\n"
    for failed in "${failed_views[@]}"; do
        printf "   ${RED}✗${RESET} %s\n" "$failed"
    done
    printf "${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n\n"
    # Retorna código de saída não-zero para sinalizar falha ao chamador
    exit 1
else
    printf "\n${BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n\n"
    exit 0
fi
