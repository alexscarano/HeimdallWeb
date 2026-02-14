#!/usr/bin/env bash
# entrypoint.sh — Inicializa o backend HeimdallWeb.WebApi
#
# Responsabilidades:
#   1. Aguardar o PostgreSQL aceitar conexões
#   2. Compilar o projeto
#   3. Aplicar migrations EF Core pendentes
#   4. Criar as 14 SQL Views (idempotente)
#   5. Iniciar dotnet watch para hot reload em desenvolvimento
#
# Executado como usuário não-privilegiado "appuser" (definido no Dockerfile).

set -euo pipefail

# ─── Cores ANSI ───────────────────────────────────────────────────────────────
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

info()    { printf "${YELLOW}[INFO]${RESET}  %s\n" "$*"; }
success() { printf "${GREEN}[OK]${RESET}    %s\n" "$*"; }
error()   { printf "${RED}[ERRO]${RESET}  %s\n" "$*" >&2; }
step()    { printf "\n${BOLD}${CYAN}%s${RESET}\n" "$*"; }

# ─── Caminhos dos projetos ────────────────────────────────────────────────────
WEBAPI_PROJECT="/workspace/src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj"
INFRA_PROJECT="/workspace/src/HeimdallWeb.Infrastructure/HeimdallWeb.Infrastructure.csproj"

# ─── Variáveis do PostgreSQL (com defaults seguros) ──────────────────────────
PG_HOST="${POSTGRES_HOST:-postgres}"
PG_PORT="${POSTGRES_PORT:-5432}"

# ─── Header ───────────────────────────────────────────────────────────────────
printf "\n${BOLD}══════════════════════════════════════════════${RESET}\n"
printf "${BOLD} HeimdallWeb API — Entrypoint${RESET}\n"
printf " Environment: ${CYAN}${ASPNETCORE_ENVIRONMENT:-Development}${RESET}\n"
printf "${BOLD}══════════════════════════════════════════════${RESET}\n\n"

# ─── [1/5] Aguardar PostgreSQL ────────────────────────────────────────────────
step "[1/5] Aguardando PostgreSQL ficar disponível..."
info "Host: ${PG_HOST}  Porta: ${PG_PORT}"

MAX_RETRIES=30
RETRY_COUNT=0
RETRY_INTERVAL=2

until pg_isready -h "$PG_HOST" -p "$PG_PORT" -q 2>/dev/null; do
  RETRY_COUNT=$(( RETRY_COUNT + 1 ))

  if [[ $RETRY_COUNT -ge $MAX_RETRIES ]]; then
    error "PostgreSQL não ficou disponível após $(( MAX_RETRIES * RETRY_INTERVAL ))s (${MAX_RETRIES} tentativas)."
    error "Verifique se o container 'postgres' está saudável e acessível em ${PG_HOST}:${PG_PORT}."
    exit 1
  fi

  info "PostgreSQL ainda não disponível — tentativa ${RETRY_COUNT}/${MAX_RETRIES}. Aguardando ${RETRY_INTERVAL}s..."
  sleep "$RETRY_INTERVAL"
done

success "PostgreSQL disponível em ${PG_HOST}:${PG_PORT}."

# ─── [2/5] Build do Projeto ───────────────────────────────────────────────────
step "[2/5] Compilando projeto..."
info "Startup project: HeimdallWeb.WebApi"

dotnet build "$WEBAPI_PROJECT" \
  --configuration Debug \
  --verbosity minimal

success "Build concluído sem erros."

# ─── [3/5] Migrations EF Core ─────────────────────────────────────────────────
step "[3/5] Aplicando migrations EF Core..."
info "--project       → HeimdallWeb.Infrastructure  (onde vivem as Migrations)"
info "--startup-project → HeimdallWeb.WebApi        (onde o DbContext é configurado)"
info "--no-build      → reutiliza o artefato do passo anterior"

dotnet ef database update \
  --project "$INFRA_PROJECT" \
  --startup-project "$WEBAPI_PROJECT" \
  --no-build \
  --connection "Host=${PG_HOST};Port=${PG_PORT};Database=${POSTGRES_DB:-db_heimdall};Username=${POSTGRES_USER:-postgres};Password=${POSTGRES_PASSWORD:-postgres};Pooling=true;"

success "Migrations aplicadas com sucesso."

# ─── [4/5] SQL Views ──────────────────────────────────────────────────────────
step "[4/5] Criando SQL Views (idempotente)..."
info "Script: /workspace/src/HeimdallWeb.Infrastructure/Data/Views/apply_views.sh"

VIEWS_SCRIPT="/workspace/src/HeimdallWeb.Infrastructure/Data/Views/apply_views.sh"

if [[ ! -f "$VIEWS_SCRIPT" ]]; then
  error "Script de views não encontrado em: ${VIEWS_SCRIPT}"
  exit 1
fi

PG_HOST="$PG_HOST" \
PG_PORT="$PG_PORT" \
POSTGRES_DB="${POSTGRES_DB:-db_heimdall}" \
POSTGRES_USER="${POSTGRES_USER:-postgres}" \
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}" \
bash "$VIEWS_SCRIPT"

success "SQL Views aplicadas com sucesso."

# ─── [5/5] Informações ao desenvolvedor + Hot Reload ─────────────────────────
step "[5/5] Iniciando dotnet watch (hot reload ativo)..."

printf "\n"
printf "${GREEN}${BOLD}╔══════════════════════════════════════════╗${RESET}\n"
printf "${GREEN}${BOLD}║  HeimdallWeb API rodando!                ║${RESET}\n"
printf "${GREEN}${BOLD}║                                          ║${RESET}\n"
printf "${GREEN}${BOLD}║  API:     ${RESET}http://localhost:5110          ${GREEN}${BOLD}║${RESET}\n"
printf "${GREEN}${BOLD}║  Swagger: ${RESET}http://localhost:5110/swagger  ${GREEN}${BOLD}║${RESET}\n"
printf "${GREEN}${BOLD}║  Env:     ${RESET}${ASPNETCORE_ENVIRONMENT:-Development}                   ${GREEN}${BOLD}║${RESET}\n"
printf "${GREEN}${BOLD}╚══════════════════════════════════════════╝${RESET}\n"
printf "\n"

exec dotnet watch run \
  --project "$WEBAPI_PROJECT" \
  --no-launch-profile
