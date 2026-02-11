# 🛠️ HeimdallWeb - Guia de Setup do Ambiente de Desenvolvimento

Este documento descreve como configurar um ambiente Linux Mint virgem para desenvolver o **HeimdallWeb**.

---

## 📋 Pré-requisitos

- **Sistema Operacional:** Linux Mint 21+ (baseado em Ubuntu 22.04+)
- **Privilégios:** Usuário com acesso sudo
- **Internet:** Conexão estável para downloads

---

## 🚀 Instalação Rápida (Recomendado)

Execute o script automatizado de setup:

```bash
# Clone o repositório (se ainda não tiver)
git clone https://github.com/seu-usuario/HeimdallWeb.git
cd HeimdallWeb

# Tornar o script executável (se ainda não estiver)
chmod +x setup-dev-environment.sh

# Executar o script de setup
./setup-dev-environment.sh
```

**O script instala automaticamente:**
- ✅ Docker & Docker Compose
- ✅ Node.js 20 LTS & npm
- ✅ .NET 10 SDK
- ✅ Visual Studio Code + extensões
- ✅ GitHub CLI & Copilot CLI
- ✅ Google Gemini CLI (aiask)
- ✅ Git & Build essentials
- ✅ MySQL Client (opcional)

**Tempo estimado:** 10-15 minutos (depende da velocidade da internet)

---

## 📖 Instalação Manual (Passo a Passo)

Se preferir instalar manualmente, siga estas etapas:

### 1. Atualizar Sistema

```bash
sudo apt update
sudo apt upgrade -y
```

### 2. Instalar Ferramentas Essenciais

```bash
sudo apt install -y \
    build-essential \
    curl \
    wget \
    ca-certificates \
    gnupg \
    lsb-release \
    apt-transport-https \
    software-properties-common
```

### 3. Instalar Git

```bash
sudo apt install -y git

# Configurar Git
git config --global user.name "Seu Nome"
git config --global user.email "seu.email@example.com"
```

### 4. Instalar Docker

```bash
# Adicionar chave GPG do Docker
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Adicionar repositório Docker
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  jammy stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Instalar Docker
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Adicionar usuário ao grupo docker
sudo usermod -aG docker $USER

# IMPORTANTE: Fazer logout e login novamente para aplicar mudanças
```

### 5. Instalar Node.js (LTS)

```bash
# Instalar Node.js 20.x via NodeSource
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Atualizar npm para a última versão
sudo npm install -g npm@latest

# Verificar instalação
node --version  # v20.x.x
npm --version   # 10.x.x
```

### 6. Instalar .NET 10 SDK

```bash
# Adicionar repositório Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar .NET SDK
sudo apt update
sudo apt install -y dotnet-sdk-10.0

# Verificar instalação
dotnet --version  # 10.x.x
```

### 7. Instalar Visual Studio Code

```bash
# Adicionar repositório Microsoft
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > packages.microsoft.gpg
sudo install -D -o root -g root -m 644 packages.microsoft.gpg /etc/apt/keyrings/packages.microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main" > /etc/apt/sources.list.d/vscode.list'
rm -f packages.microsoft.gpg

# Instalar VS Code
sudo apt update
sudo apt install -y code

# Instalar extensões recomendadas
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension bradlc.vscode-tailwindcss
code --install-extension dbaeumer.vscode-eslint
code --install-extension esbenp.prettier-vscode
code --install-extension GitHub.copilot
code --install-extension GitHub.copilot-chat
```

### 8. Instalar GitHub CLI & Copilot CLI

```bash
# Adicionar repositório GitHub CLI
curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null

# Instalar GitHub CLI
sudo apt update
sudo apt install -y gh

# Autenticar GitHub CLI
gh auth login

# Instalar extensão Copilot
gh extension install github/gh-copilot
```

### 9. Instalar Google Gemini CLI (aiask)

```bash
# Instalar via npm
sudo npm install -g @google/generative-ai-cli

# Configurar API key (adicione ao ~/.bashrc)
echo 'export GEMINI_API_KEY="sua-chave-api-aqui"' >> ~/.bashrc
source ~/.bashrc
```

### 10. Configurar Variáveis de Ambiente

Adicione ao `~/.bashrc`:

```bash
# .NET Configuration
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# npm global bin
export PATH=$PATH:$(npm config get prefix)/bin

# Google Gemini API Key
export GEMINI_API_KEY="sua-chave-api-aqui"
```

Recarregar configurações:

```bash
source ~/.bashrc
```

---

## ✅ Verificar Instalações

Execute os seguintes comandos para verificar se tudo foi instalado corretamente:

```bash
git --version           # git version 2.x.x
docker --version        # Docker version 24.x.x
docker compose version  # Docker Compose version v2.x.x
node --version          # v20.x.x
npm --version           # 10.x.x
dotnet --version        # 10.x.x
code --version          # 1.x.x
gh --version            # gh version 2.x.x
```

---

## 🏗️ Configurar o Projeto

Após instalar todas as ferramentas, configure o projeto:

### 1. Clonar o Repositório

```bash
git clone https://github.com/seu-usuario/HeimdallWeb.git
cd HeimdallWeb
```

### 2. Restaurar Dependências do Backend

```bash
dotnet restore
dotnet build
```

### 3. Restaurar Dependências do Frontend

```bash
cd src/HeimdallWeb.Next
npm install
npm run build
cd ../..
```

### 4. Configurar Banco de Dados (MySQL)

**Opção 1: Docker (apenas para banco de dados, não para desenvolvimento):**

```bash
docker compose up -d mysql
```

**Opção 2: MySQL local:**

```bash
sudo apt install -y mysql-server
sudo systemctl start mysql
sudo mysql_secure_installation
```

### 5. Aplicar Migrações

```bash
# Para o projeto legado (HeimdallWebOld)
dotnet ef database update --project HeimdallWebOld

# Para o novo projeto (quando migrar)
dotnet ef database update --project src/HeimdallWeb.Infrastructure --startup-project src/HeimdallWeb.WebApi
```

### 6. Executar o Projeto

**Backend (Desenvolvimento - NÃO use Docker):**

```bash
# Projeto legado
dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj

# Novo projeto WebAPI (após migração)
dotnet run --project src/HeimdallWeb.WebApi
```

**Frontend (Next.js):**

```bash
cd src/HeimdallWeb.Next
npm run dev
```

**Acesse:** `http://localhost:3000`

---

## 📚 Comandos Úteis

### Backend (.NET)

```bash
# Restaurar dependências
dotnet restore

# Compilar o projeto
dotnet build

# Executar o projeto
dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj

# Executar testes
dotnet test

# Criar migração
dotnet ef migrations add NomeDaMigracao --project HeimdallWebOld

# Aplicar migrações
dotnet ef database update --project HeimdallWebOld

# Limpar build
dotnet clean
```

### Frontend (Next.js)

```bash
# Instalar dependências
npm install

# Servidor de desenvolvimento
npm run dev

# Build de produção
npm run build

# Iniciar servidor de produção
npm start

# Lint
npm run lint

# Compilar TypeScript
npx tsc
```

### Docker (Apenas Produção)

```bash
# Iniciar serviços
docker compose up -d

# Parar serviços
docker compose down

# Ver logs
docker compose logs -f

# Rebuild containers
docker compose up -d --build

# Limpar containers e volumes
docker compose down -v
```

---

## ⚠️ Notas Importantes

### ❌ O que NÃO fazer:

1. **NÃO use Docker para desenvolvimento** - Use `dotnet run` e `npm run dev`
2. **NÃO faça alterações sem ler `CLAUDE.md`** - Contém regras SEVERAS
3. **NÃO pule testes** - Teste todos os endpoints após alterações no backend
4. **NÃO improvise design** - Consulte o agente de design primeiro
5. **NÃO esqueça de marcar tarefas** em `plano_migracao.md`

### ✅ O que fazer:

1. **Sempre marque tarefas concluídas** em `plano_migracao.md`
2. **Use browser automation** após mudanças no frontend (MCP Chrome/Puppeteer)
3. **Teste TODOS os endpoints** após sprints de backend e crie guia de testes
4. **Siga o plano de migração** em `plano_migracao.md`
5. **Consulte o designer** antes de implementar qualquer UI

---

## 🆘 Solução de Problemas

### Docker: "permission denied"

```bash
# Adicionar usuário ao grupo docker
sudo usermod -aG docker $USER

# Fazer logout e login novamente
# OU reiniciar o sistema
```

### Node.js: "EACCES" ao instalar pacotes globais

```bash
# Configurar diretório npm global
mkdir ~/.npm-global
npm config set prefix '~/.npm-global'
echo 'export PATH=~/.npm-global/bin:$PATH' >> ~/.bashrc
source ~/.bashrc
```

### .NET: "SDK not found"

```bash
# Verificar se o SDK está instalado
dotnet --list-sdks

# Se não estiver, reinstalar
sudo apt install -y dotnet-sdk-10.0
```

### VS Code: Extensões não instalam

```bash
# Limpar cache de extensões
rm -rf ~/.vscode/extensions/*

# Reinstalar extensões
code --install-extension ms-dotnettools.csharp --force
```

### MySQL: Conexão recusada

```bash
# Verificar se o serviço está rodando
sudo systemctl status mysql

# Iniciar o serviço
sudo systemctl start mysql

# Habilitar na inicialização
sudo systemctl enable mysql
```

---

## 📖 Documentação Adicional

- **CLAUDE.md** - Regras SEVERAS para desenvolvimento (LEIA PRIMEIRO!)
- **plano_migracao.md** - Plano de migração detalhado (fases, tarefas, anti-patterns)
- **README.md** - Visão geral do projeto
- **docs/** - Documentação técnica e guias de teste

---

## 🤝 Contribuindo

Antes de contribuir:

1. Leia `CLAUDE.md` completo
2. Consulte `plano_migracao.md` para a fase atual
3. Siga o padrão de commits do projeto
4. Marque tarefas concluídas no plano de migração
5. Crie guias de teste após implementações

---

## 📞 Suporte

Para dúvidas sobre:
- **Ferramentas de desenvolvimento:** Consulte a documentação oficial de cada ferramenta
- **Projeto HeimdallWeb:** Leia `CLAUDE.md` e `plano_migracao.md`
- **Erros específicos:** Verifique a seção "Solução de Problemas" acima

---

## 📝 Licença

Este projeto segue a licença definida no repositório principal.

---

**Boa codificação! 🚀**
