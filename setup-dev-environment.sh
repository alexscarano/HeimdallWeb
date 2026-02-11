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

9. Build the project:
   Backend:  $ dotnet build
   Frontend: $ cd src/HeimdallWeb.Next && npm run build

10. Read CLAUDE.md for project-specific workflow rules

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

Docker (Production Only - not for development):
  docker compose up -d               # Start services
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
    configure_environment
    
    # Verification and final instructions
    verify_installations
    post_install_instructions
    
    log_success "Setup script completed!"
}

# Run main function
main
