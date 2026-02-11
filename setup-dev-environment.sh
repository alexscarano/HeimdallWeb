#!/bin/bash

################################################################################
# HeimdallWeb - Script de Setup do Ambiente de Desenvolvimento
# 
# Instala e configura todas as ferramentas necessárias para desenvolver
# o projeto HeimdallWeb em um Linux Mint virgem.
#
# Ferramentas instaladas:
# - Docker & Docker Compose
# - Node.js (LTS) & npm
# - .NET 10 SDK
# - Visual Studio Code
# - GitHub Copilot CLI
# - Google Gemini CLI (via npm aiask)
# - Git
# - Build essentials
#
# Uso: chmod +x setup-dev-environment.sh && ./setup-dev-environment.sh
################################################################################

set -e  # Exit on error
set -u  # Exit on undefined variable

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[✓]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

log_error() {
    echo -e "${RED}[✗]${NC} $1"
}

log_section() {
    echo ""
    echo -e "${GREEN}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN} $1${NC}"
    echo -e "${GREEN}════════════════════════════════════════════════════════════════${NC}"
    echo ""
}

# Check if running with sudo for certain operations
check_sudo() {
    if [ "$EUID" -eq 0 ]; then
        log_error "Please do NOT run this script as root or with sudo"
        log_info "The script will ask for sudo password when needed"
        exit 1
    fi
}

# Update package list
update_system() {
    log_section "Updating System Package List"
    sudo apt update
    log_success "Package list updated"
}

# Install essential build tools
install_essentials() {
    log_section "Installing Essential Build Tools"
    
    if ! dpkg -l | grep -q build-essential; then
        log_info "Installing build-essential, curl, wget, ca-certificates..."
        sudo apt install -y \
            build-essential \
            curl \
            wget \
            ca-certificates \
            gnupg \
            lsb-release \
            apt-transport-https \
            software-properties-common
        log_success "Essential tools installed"
    else
        log_success "Essential tools already installed"
    fi
}

# Install Git
install_git() {
    log_section "Installing Git"
    
    if ! command -v git &> /dev/null; then
        log_info "Installing Git..."
        sudo apt install -y git
        log_success "Git installed"
    else
        log_success "Git already installed: $(git --version)"
    fi
    
    # Configure Git (optional)
    if [ -z "$(git config --global user.name)" ]; then
        log_warning "Git user.name not configured. Please run:"
        echo "  git config --global user.name \"Your Name\""
        echo "  git config --global user.email \"your.email@example.com\""
    fi
}

# Install Docker
install_docker() {
    log_section "Installing Docker"
    
    if ! command -v docker &> /dev/null; then
        log_info "Installing Docker..."
        
        # Add Docker's official GPG key
        sudo mkdir -p /etc/apt/keyrings
        curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
        
        # Set up the repository (Ubuntu-based for Linux Mint)
        echo \
          "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
          $(lsb_release -cs 2>/dev/null || echo jammy) stable" | \
          sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
        
        sudo apt update
        sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
        
        # Add current user to docker group (no sudo needed)
        sudo usermod -aG docker "$USER"
        
        log_success "Docker installed"
        log_warning "You need to log out and back in for Docker group changes to take effect"
    else
        log_success "Docker already installed: $(docker --version)"
    fi
    
    # Verify docker-compose plugin
    if docker compose version &> /dev/null; then
        log_success "Docker Compose plugin installed: $(docker compose version)"
    else
        log_warning "Docker Compose plugin not found"
    fi
}

# Install Node.js (LTS) and npm
install_nodejs() {
    log_section "Installing Node.js (LTS) and npm"
    
    if ! command -v node &> /dev/null; then
        log_info "Installing Node.js via NodeSource repository..."
        
        # Install Node.js 20.x LTS (adjust version as needed)
        curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
        sudo apt install -y nodejs
        
        log_success "Node.js installed: $(node --version)"
        log_success "npm installed: $(npm --version)"
    else
        log_success "Node.js already installed: $(node --version)"
        log_success "npm already installed: $(npm --version)"
    fi
    
    # Update npm to latest
    log_info "Updating npm to latest version..."
    sudo npm install -g npm@latest
    log_success "npm updated to: $(npm --version)"
}

# Install .NET SDK 10
install_dotnet() {
    log_section "Installing .NET 10 SDK"
    
    if ! command -v dotnet &> /dev/null; then
        log_info "Installing .NET 10 SDK..."
        
        # Add Microsoft package repository
        wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        sudo dpkg -i packages-microsoft-prod.deb
        rm packages-microsoft-prod.deb
        
        sudo apt update
        sudo apt install -y dotnet-sdk-10.0
        
        log_success ".NET SDK installed: $(dotnet --version)"
    else
        log_success ".NET SDK already installed: $(dotnet --version)"
    fi
    
    # Verify ASP.NET Core runtime
    if dotnet --list-runtimes | grep -q "Microsoft.AspNetCore.App"; then
        log_success "ASP.NET Core runtime installed"
    else
        log_warning "ASP.NET Core runtime not found"
    fi
}

# Install Visual Studio Code
install_vscode() {
    log_section "Installing Visual Studio Code"
    
    if ! command -v code &> /dev/null; then
        log_info "Installing VS Code..."
        
        # Add Microsoft GPG key and repository
        wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > packages.microsoft.gpg
        sudo install -D -o root -g root -m 644 packages.microsoft.gpg /etc/apt/keyrings/packages.microsoft.gpg
        sudo sh -c 'echo "deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main" > /etc/apt/sources.list.d/vscode.list'
        rm -f packages.microsoft.gpg
        
        sudo apt update
        sudo apt install -y code
        
        log_success "VS Code installed: $(code --version | head -n1)"
    else
        log_success "VS Code already installed: $(code --version | head -n1)"
    fi
    
    # Install recommended VS Code extensions
    log_info "Installing recommended VS Code extensions..."
    code --install-extension ms-dotnettools.csharp --force
    code --install-extension ms-dotnettools.csdevkit --force
    code --install-extension bradlc.vscode-tailwindcss --force
    code --install-extension dbaeumer.vscode-eslint --force
    code --install-extension esbenp.prettier-vscode --force
    code --install-extension GitHub.copilot --force
    code --install-extension GitHub.copilot-chat --force
    log_success "VS Code extensions installed"
}

# Install GitHub Copilot CLI
install_copilot_cli() {
    log_section "Installing GitHub Copilot CLI"
    
    if ! command -v gh &> /dev/null; then
        log_info "Installing GitHub CLI first..."
        
        # Add GitHub CLI repository
        curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
        sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
        echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null
        
        sudo apt update
        sudo apt install -y gh
        
        log_success "GitHub CLI installed: $(gh --version | head -n1)"
    else
        log_success "GitHub CLI already installed: $(gh --version | head -n1)"
    fi
    
    log_info "To authenticate GitHub CLI, run: gh auth login"
    log_info "To install Copilot extension, run: gh extension install github/gh-copilot"
}

# Install Google Gemini CLI (aiask via npm)
install_gemini_cli() {
    log_section "Installing Google Gemini CLI (aiask)"
    
    if ! command -v aiask &> /dev/null; then
        log_info "Installing aiask (Gemini CLI) globally via npm..."
        sudo npm install -g @google/generative-ai-cli
        
        log_success "Gemini CLI installed"
    else
        log_success "aiask already installed"
    fi
    
    log_warning "To use Gemini CLI, set your API key:"
    echo "  export GEMINI_API_KEY='your-api-key-here'"
    echo "  # Add to ~/.bashrc or ~/.zshrc for persistence"
}

# Install MySQL Client (optional, for database management)
install_mysql_client() {
    log_section "Installing MySQL Client (Optional)"
    
    read -p "Do you want to install MySQL client for database management? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        sudo apt install -y mysql-client
        log_success "MySQL client installed"
    else
        log_info "Skipping MySQL client installation"
    fi
}

# Pull PostgreSQL 16 Docker image
pull_postgres_image() {
    log_section "Pulling PostgreSQL 16 Docker Image"
    
    if ! command -v docker &> /dev/null; then
        log_warning "Docker not installed, skipping PostgreSQL image pull"
        log_info "Run this script again after logging back in to pull the image"
        return
    fi
    
    log_info "Pulling postgres:16 Docker image..."
    log_info "This may take a few minutes depending on your internet connection"
    
    # Check if user is in docker group (may need re-login)
    if groups | grep -q docker; then
        if docker pull postgres:16; then
            log_success "PostgreSQL 16 image pulled successfully"
            
            # Show image info
            IMAGE_SIZE=$(docker images postgres:16 --format "{{.Size}}")
            log_info "Image size: $IMAGE_SIZE"
            
            # Optionally create a dev container
            read -p "Do you want to create a PostgreSQL container for development? (y/N): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                create_postgres_container
            else
                log_info "You can create a container later with:"
                echo "  docker run -d --name heimdall-postgres \\"
                echo "    -e POSTGRES_PASSWORD=heimdall123 \\"
                echo "    -e POSTGRES_USER=heimdall \\"
                echo "    -e POSTGRES_DB=heimdallweb \\"
                echo "    -p 5432:5432 \\"
                echo "    -v heimdall-pgdata:/var/lib/postgresql/data \\"
                echo "    postgres:16"
            fi
        else
            log_error "Failed to pull PostgreSQL image"
            log_info "You may need to log out and back in for Docker permissions"
        fi
    else
        log_warning "User not in docker group yet - log out and back in first"
        log_info "After re-login, pull the image manually with:"
        echo "  docker pull postgres:16"
    fi
}

# Create PostgreSQL development container
create_postgres_container() {
    log_info "Creating PostgreSQL development container..."
    
    # Check if container already exists
    if docker ps -a --format '{{.Names}}' | grep -q "^heimdall-postgres$"; then
        log_warning "Container 'heimdall-postgres' already exists"
        read -p "Do you want to remove and recreate it? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            docker rm -f heimdall-postgres
            docker volume rm heimdall-pgdata 2>/dev/null || true
        else
            log_info "Keeping existing container"
            return
        fi
    fi
    
    # Create container
    if docker run -d \
        --name heimdall-postgres \
        -e POSTGRES_PASSWORD=heimdall123 \
        -e POSTGRES_USER=heimdall \
        -e POSTGRES_DB=heimdallweb \
        -p 5432:5432 \
        -v heimdall-pgdata:/var/lib/postgresql/data \
        --restart unless-stopped \
        postgres:16; then
        
        log_success "PostgreSQL container created and started"
        log_info "Connection details:"
        echo "  Host: localhost"
        echo "  Port: 5432"
        echo "  Database: heimdallweb"
        echo "  User: heimdall"
        echo "  Password: heimdall123"
        echo ""
        log_info "Connection string for appsettings.json:"
        echo '  "ConnectionStrings": {'
        echo '    "AppDbConnectionString": "Host=localhost;Port=5432;Database=heimdallweb;Username=heimdall;Password=heimdall123"'
        echo '  }'
        echo ""
        log_warning "This is a DEVELOPMENT container with a weak password"
        log_warning "DO NOT use this configuration in production"
    else
        log_error "Failed to create PostgreSQL container"
    fi
}

# Configure environment variables
configure_environment() {
    log_section "Configuring Environment Variables"
    
    BASHRC="$HOME/.bashrc"
    
    # Add .NET tools to PATH if not already present
    if ! grep -q "DOTNET_ROOT" "$BASHRC"; then
        log_info "Adding .NET environment variables to ~/.bashrc..."
        cat >> "$BASHRC" << 'EOF'

# .NET Configuration
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
EOF
        log_success ".NET environment variables added"
    fi
    
    # Add npm global bin to PATH
    if ! grep -q "npm global bin" "$BASHRC"; then
        log_info "Adding npm global bin to PATH..."
        cat >> "$BASHRC" << 'EOF'

# npm global bin
export PATH=$PATH:$(npm config get prefix)/bin
EOF
        log_success "npm global bin added to PATH"
    fi
    
    log_info "Reload your shell or run: source ~/.bashrc"
}

# Create COMMANDS.sh quick reference file
create_commands_reference() {
    log_section "Creating Commands Quick Reference"
    
    # Detect if we're in the project directory or ask for it
    if [ -f "HeimdallWeb.sln" ]; then
        PROJECT_DIR="$(pwd)"
    else
        log_info "HeimdallWeb project not found in current directory"
        log_info "The COMMANDS.sh file will be created when you clone the repository"
        return
    fi
    
    COMMANDS_FILE="$PROJECT_DIR/COMMANDS.sh"
    
    if [ -f "$COMMANDS_FILE" ]; then
        log_success "COMMANDS.sh already exists at $COMMANDS_FILE"
        return
    fi
    
    log_info "Creating COMMANDS.sh quick reference file..."
    
    cat > "$COMMANDS_FILE" << 'COMMANDSEOF'
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
COMMANDSEOF

    chmod +x "$COMMANDS_FILE"
    
    log_success "COMMANDS.sh created at $COMMANDS_FILE"
    log_info "View commands: cat $COMMANDS_FILE"
    log_info "Load aliases: source $COMMANDS_FILE"
}

# Verify installations
verify_installations() {
    log_section "Verifying Installations"
    
    declare -A tools=(
        ["git"]="git --version"
        ["docker"]="docker --version"
        ["node"]="node --version"
        ["npm"]="npm --version"
        ["dotnet"]="dotnet --version"
        ["code"]="code --version | head -n1"
        ["gh"]="gh --version | head -n1"
    )
    
    ALL_OK=true
    
    for tool in "${!tools[@]}"; do
        if command -v "$tool" &> /dev/null; then
            VERSION=$(eval "${tools[$tool]}" 2>&1)
            log_success "$tool: $VERSION"
        else
            log_error "$tool: NOT FOUND"
            ALL_OK=false
        fi
    done
    
    if [ "$ALL_OK" = true ]; then
        echo ""
        log_success "All tools installed successfully!"
    else
        echo ""
        log_warning "Some tools are missing. Please review the output above."
    fi
}

# Display post-installation instructions
post_install_instructions() {
    log_section "Post-Installation Instructions"
    
    cat << 'EOF'
╔══════════════════════════════════════════════════════════════════════╗
║                    🎉 INSTALLATION COMPLETE!                         ║
╚══════════════════════════════════════════════════════════════════════╝

📋 NEXT STEPS:

1. Reload your shell configuration:
   $ source ~/.bashrc
   
2. Log out and back in (for Docker group changes to take effect)

3. Authenticate GitHub CLI:
   $ gh auth login
   
4. Install GitHub Copilot CLI extension:
   $ gh extension install github/gh-copilot
   
5. Configure Git (if not already done):
   $ git config --global user.name "Your Name"
   $ git config --global user.email "your.email@example.com"
   
6. Set up Gemini API key in ~/.bashrc:
   $ echo 'export GEMINI_API_KEY="your-api-key-here"' >> ~/.bashrc
   $ source ~/.bashrc

7. Navigate to the project and restore dependencies:
   $ cd /path/to/HeimdallWeb
   $ dotnet restore
   $ cd src/HeimdallWeb.Next
   $ npm install

8. Verify Docker is working (after re-login):
   $ docker run hello-world

9. Pull PostgreSQL image (if not done automatically):
   $ docker pull postgres:16

10. Build the project:
    Backend:  $ dotnet build
    Frontend: $ cd src/HeimdallWeb.Next && npm run build

11. View quick reference commands:
    $ cat COMMANDS.sh (or ./COMMANDS.sh)
    $ source COMMANDS.sh (to load aliases)

12. Read CLAUDE.md for project-specific workflow rules

═══════════════════════════════════════════════════════════════════════

📚 USEFUL COMMANDS:

Backend:
  dotnet restore                     # Restore dependencies
  dotnet build                       # Build solution
  dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj
  dotnet test                        # Run tests
  dotnet ef database update --project HeimdallWebOld

Frontend:
  npm install                        # Install dependencies
  npm run dev                        # Development server
  npm run build                      # Production build
  npm run lint                       # Lint code

Docker (Database & Production):
  # PostgreSQL development container (if created)
  docker start heimdall-postgres     # Start PostgreSQL
  docker stop heimdall-postgres      # Stop PostgreSQL
  docker logs -f heimdall-postgres   # View PostgreSQL logs
  
  # Production (full stack - not for development)
  docker compose up -d               # Start all services
  docker compose down                # Stop services
  docker compose logs -f             # View logs

═══════════════════════════════════════════════════════════════════════

⚠️  IMPORTANT NOTES:

- DO NOT use Docker for development (use dotnet run)
- Always follow CLAUDE.md and plano_migracao.md
- Mark completed tasks in plano_migracao.md
- Test endpoints after backend changes
- Use browser automation after frontend changes

═══════════════════════════════════════════════════════════════════════
EOF
}

# Main execution
main() {
    log_section "HeimdallWeb Development Environment Setup"
    log_info "This script will install all necessary tools for development"
    log_info "Press Ctrl+C to cancel at any time"
    echo ""
    
    check_sudo
    
    read -p "Continue with installation? (Y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Nn]$ ]]; then
        log_info "Installation cancelled"
        exit 0
    fi
    
    # Execute installation steps
    update_system
    install_essentials
    install_git
    install_docker
    install_nodejs
    install_dotnet
    install_vscode
    install_copilot_cli
    install_gemini_cli
    install_mysql_client
    pull_postgres_image
    configure_environment
    create_commands_reference
    
    # Verification and final instructions
    verify_installations
    post_install_instructions
    
    log_success "Setup script completed!"
}

# Run main function
main
