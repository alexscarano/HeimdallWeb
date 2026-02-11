#!/bin/bash

################################################################################
# HeimdallWeb - Comandos de Desenvolvimento (Quick Reference)
# 
# Este arquivo contém todos os comandos úteis para desenvolvimento do projeto.
# Você pode sourcer este arquivo ou copiar os comandos conforme necessário.
#
# Uso: cat COMMANDS.sh (para visualizar)
#      source COMMANDS.sh (para carregar aliases no shell)
################################################################################

# Cores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}   HeimdallWeb - Comandos de Desenvolvimento${NC}"
echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo ""

# ============================================================================
# BACKEND (.NET)
# ============================================================================

echo -e "${BLUE}📦 BACKEND (.NET)${NC}"
echo ""

# Restaurar dependências
alias dotnet-restore='dotnet restore'
echo "  dotnet restore                              # Restaurar dependências"

# Build do projeto
alias dotnet-build='dotnet build HeimdallWeb.sln'
echo "  dotnet build                                # Build completo da solution"
echo "  dotnet build --no-restore                   # Build sem restaurar dependências"

# Rodar projeto legado (MVC)
alias dotnet-run-legacy='dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj'
echo "  dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj"
echo "                                              # Rodar projeto legado (MVC)"

# Rodar novo projeto (WebAPI - após migração)
alias dotnet-run-api='dotnet run --project src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj'
echo "  dotnet run --project src/HeimdallWeb.WebApi/"
echo "                                              # Rodar nova WebAPI (após migração)"

# Rodar em modo watch (hot reload)
alias dotnet-watch='dotnet watch --project HeimdallWebOld/HeimdallWebOld.csproj'
echo "  dotnet watch --project HeimdallWebOld/      # Hot reload (desenvolvimento)"

# Testes
alias dotnet-test='dotnet test'
echo "  dotnet test                                 # Rodar todos os testes"
echo "  dotnet test --filter FullyQualifiedName~ClassName.TestMethod"
echo "                                              # Rodar teste específico"
echo "  dotnet test --logger \"console;verbosity=detailed\""
echo "                                              # Testes com output detalhado"

# Limpar build artifacts
alias dotnet-clean='dotnet clean'
echo "  dotnet clean                                # Limpar artifacts de build"

# Entity Framework Core
echo ""
echo -e "${BLUE}🗄️  ENTITY FRAMEWORK CORE (Migrações)${NC}"
echo ""

# Migrations - Projeto Legado
echo "  # Projeto Legado (HeimdallWebOld - MySQL):"
echo "  dotnet ef database update --project HeimdallWebOld"
echo "                                              # Aplicar migrações ao banco"
echo "  dotnet ef migrations add NomeDaMigracao --project HeimdallWebOld"
echo "                                              # Criar nova migração"
echo "  dotnet ef migrations list --project HeimdallWebOld"
echo "                                              # Listar migrações"
echo "  dotnet ef migrations remove --project HeimdallWebOld"
echo "                                              # Remover última migração"
echo "  dotnet ef database drop --project HeimdallWebOld"
echo "                                              # Dropar banco de dados"
echo ""

# Migrations - Novo Projeto
echo "  # Novo Projeto (Infrastructure - PostgreSQL):"
echo "  dotnet ef database update --project src/HeimdallWeb.Infrastructure --startup-project src/HeimdallWeb.WebApi"
echo "                                              # Aplicar migrações (novo projeto)"
echo "  dotnet ef migrations add NomeDaMigracao --project src/HeimdallWeb.Infrastructure --startup-project src/HeimdallWeb.WebApi"
echo "                                              # Criar nova migração (novo projeto)"
echo ""

# ============================================================================
# FRONTEND (Next.js)
# ============================================================================

echo -e "${BLUE}⚛️  FRONTEND (Next.js 15)${NC}"
echo ""

# Navegar para o frontend
echo "  cd src/HeimdallWeb.Next                     # Ir para o diretório do frontend"
echo ""

# npm install
echo "  npm install                                 # Instalar dependências"
echo "  npm install --legacy-peer-deps              # Instalar com deps legadas (se necessário)"

# Desenvolvimento
echo "  npm run dev                                 # Servidor de desenvolvimento (porta 3000)"
echo "  npm run dev -- --turbo                      # Dev server com Turbopack (mais rápido)"

# Build de produção
echo "  npm run build                               # Build de produção"
echo "  npm start                                   # Iniciar servidor de produção (após build)"

# Lint e formatação
echo "  npm run lint                                # ESLint (verificar erros)"
echo "  npm run lint -- --fix                       # ESLint (corrigir automaticamente)"

# TypeScript
echo "  npx tsc                                     # Compilar TypeScript"
echo "  npx tsc --watch                             # Compilar TypeScript em watch mode"
echo "  npx tsc --noEmit                            # Verificar tipos sem gerar arquivos"

# Limpar cache do Next.js
echo "  rm -rf .next                                # Limpar build cache do Next.js"
echo "  rm -rf node_modules package-lock.json && npm install"
echo "                                              # Reinstalar dependências do zero"
echo ""

# ============================================================================
# DATABASE (Docker - PostgreSQL)
# ============================================================================

echo -e "${BLUE}🐳 DOCKER - PostgreSQL (Desenvolvimento)${NC}"
echo ""

# Container de desenvolvimento
echo "  # Container local de desenvolvimento (criado pelo setup-dev-environment.sh):"
echo "  docker start heimdall-postgres              # Iniciar container PostgreSQL"
echo "  docker stop heimdall-postgres               # Parar container PostgreSQL"
echo "  docker restart heimdall-postgres            # Reiniciar container"
echo "  docker logs -f heimdall-postgres            # Ver logs em tempo real"
echo "  docker exec -it heimdall-postgres psql -U heimdall -d heimdallweb"
echo "                                              # Conectar ao PostgreSQL via CLI"
echo ""

# Criar container manualmente
echo "  # Criar container PostgreSQL manualmente:"
echo "  docker run -d --name heimdall-postgres \\"
echo "    -e POSTGRES_PASSWORD=heimdall123 \\"
echo "    -e POSTGRES_USER=heimdall \\"
echo "    -e POSTGRES_DB=heimdallweb \\"
echo "    -p 5432:5432 \\"
echo "    -v heimdall-pgdata:/var/lib/postgresql/data \\"
echo "    --restart unless-stopped \\"
echo "    postgres:16"
echo ""

# Limpar dados
echo "  docker rm -f heimdall-postgres              # Remover container"
echo "  docker volume rm heimdall-pgdata            # Remover volume (apaga dados!)"
echo ""

# ============================================================================
# DOCKER COMPOSE (Produção - NÃO usar para desenvolvimento)
# ============================================================================

echo -e "${YELLOW}⚠️  DOCKER COMPOSE (PRODUÇÃO APENAS - NÃO USE PARA DEV)${NC}"
echo ""

echo "  docker compose up -d                        # Iniciar todos os serviços"
echo "  docker compose down                         # Parar todos os serviços"
echo "  docker compose logs -f                      # Ver logs de todos os containers"
echo "  docker compose logs -f webapi               # Ver logs de um serviço específico"
echo "  docker compose ps                           # Listar containers rodando"
echo "  docker compose restart                      # Reiniciar todos os serviços"
echo "  docker compose down -v                      # Parar e remover volumes (CUIDADO!)"
echo "  docker compose build --no-cache             # Rebuild sem cache"
echo "  docker compose up -d --build                # Rebuild e iniciar"
echo ""

# ============================================================================
# GIT
# ============================================================================

echo -e "${BLUE}🔧 GIT${NC}"
echo ""

echo "  git status                                  # Ver status do repositório"
echo "  git add .                                   # Adicionar todas as mudanças"
echo "  git commit -m \"mensagem\"                    # Commit com mensagem"
echo "  git push origin migracao                    # Push para branch migracao"
echo "  git pull origin migracao                    # Pull da branch migracao"
echo "  git checkout -b nova-branch                 # Criar nova branch"
echo "  git log --oneline -10                       # Ver últimos 10 commits"
echo "  git diff                                    # Ver mudanças não staged"
echo "  git diff --staged                           # Ver mudanças staged"
echo "  git stash                                   # Guardar mudanças temporariamente"
echo "  git stash pop                               # Restaurar mudanças guardadas"
echo ""

# ============================================================================
# TESTES MANUAIS (Endpoints)
# ============================================================================

echo -e "${BLUE}🧪 TESTES MANUAIS (Endpoints API)${NC}"
echo ""

echo "  # Backend deve estar rodando (porta 5000 ou 5001)"
echo ""
echo "  # Registrar usuário:"
echo "  curl -X POST http://localhost:5000/api/v1/auth/register \\"
echo "    -H 'Content-Type: application/json' \\"
echo "    -d '{\"username\":\"test\",\"email\":\"test@example.com\",\"password\":\"Test123!@#\"}'"
echo ""
echo "  # Login:"
echo "  curl -X POST http://localhost:5000/api/v1/auth/login \\"
echo "    -H 'Content-Type: application/json' \\"
echo "    -d '{\"email\":\"test@example.com\",\"password\":\"Test123!@#\"}' \\"
echo "    -c cookies.txt"
echo ""
echo "  # Executar scan:"
echo "  curl -X POST http://localhost:5000/api/v1/scan \\"
echo "    -H 'Content-Type: application/json' \\"
echo "    -b cookies.txt \\"
echo "    -d '{\"target\":\"https://example.com\"}'"
echo ""
echo "  # Ver histórico de scans:"
echo "  curl http://localhost:5000/api/v1/scan-histories?page=1&pageSize=10 \\"
echo "    -b cookies.txt"
echo ""

# ============================================================================
# VARIÁVEIS DE AMBIENTE
# ============================================================================

echo -e "${BLUE}🔐 VARIÁVEIS DE AMBIENTE${NC}"
echo ""

echo "  # Configurar no ~/.bashrc ou ~/.zshrc:"
echo "  export GEMINI_API_KEY=\"sua-chave-api-aqui\""
echo "  export ASPNETCORE_ENVIRONMENT=\"Development\""
echo "  export DOTNET_ROOT=\$HOME/.dotnet"
echo "  export PATH=\$PATH:\$DOTNET_ROOT:\$DOTNET_ROOT/tools"
echo ""
echo "  # Recarregar shell:"
echo "  source ~/.bashrc"
echo ""

# ============================================================================
# TROUBLESHOOTING
# ============================================================================

echo -e "${BLUE}🔍 TROUBLESHOOTING${NC}"
echo ""

echo "  # Limpar tudo e recomeçar:"
echo "  dotnet clean                                # Limpar backend"
echo "  rm -rf src/HeimdallWeb.Next/.next           # Limpar cache Next.js"
echo "  rm -rf src/HeimdallWeb.Next/node_modules    # Limpar node_modules"
echo "  dotnet restore                              # Restaurar backend"
echo "  cd src/HeimdallWeb.Next && npm install      # Reinstalar frontend"
echo ""

echo "  # Verificar portas em uso:"
echo "  sudo lsof -i :5000                          # Backend .NET"
echo "  sudo lsof -i :3000                          # Frontend Next.js"
echo "  sudo lsof -i :5432                          # PostgreSQL"
echo ""

echo "  # Matar processo na porta:"
echo "  sudo kill -9 \$(lsof -t -i:5000)             # Matar processo na porta 5000"
echo ""

echo "  # Verificar logs do backend:"
echo "  tail -f logs/app.log                        # (se houver logging em arquivo)"
echo ""

echo "  # Verificar versões instaladas:"
echo "  dotnet --version                            # .NET SDK"
echo "  node --version                              # Node.js"
echo "  npm --version                               # npm"
echo "  docker --version                            # Docker"
echo ""

# ============================================================================
# WORKFLOW RECOMENDADO (CLAUDE.md)
# ============================================================================

echo -e "${YELLOW}⚠️  WORKFLOW RECOMENDADO (Regras do CLAUDE.md)${NC}"
echo ""

echo "  1. SEMPRE marque tarefas concluídas em plano_migracao.md"
echo "  2. NÃO use Docker para desenvolvimento - use 'dotnet run'"
echo "  3. SEMPRE teste endpoints após mudanças no backend"
echo "  4. SEMPRE use browser automation após mudanças no frontend"
echo "  5. SEMPRE consulte o designer antes de implementar UI"
echo "  6. Siga o plano de migração em plano_migracao.md"
echo ""

echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}   Para mais informações, leia CLAUDE.md e README-SETUP.md${NC}"
echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo ""

# ============================================================================
# ALIASES ÚTEIS (source este arquivo para usar)
# ============================================================================

# Backend
alias heimdall-restore='dotnet restore'
alias heimdall-build='dotnet build'
alias heimdall-run='dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj'
alias heimdall-test='dotnet test'
alias heimdall-clean='dotnet clean'

# Frontend
alias heimdall-frontend='cd src/HeimdallWeb.Next'
alias heimdall-dev='cd src/HeimdallWeb.Next && npm run dev'
alias heimdall-build-frontend='cd src/HeimdallWeb.Next && npm run build'

# Database
alias heimdall-db-up='docker start heimdall-postgres'
alias heimdall-db-down='docker stop heimdall-postgres'
alias heimdall-db-logs='docker logs -f heimdall-postgres'
alias heimdall-db-shell='docker exec -it heimdall-postgres psql -U heimdall -d heimdallweb'

# Full stack
alias heimdall-full-clean='dotnet clean && cd src/HeimdallWeb.Next && rm -rf .next node_modules && cd ../..'

echo ""
echo "✅ Aliases carregados! Use 'heimdall-' e pressione TAB para ver todos os comandos disponíveis"
echo ""
