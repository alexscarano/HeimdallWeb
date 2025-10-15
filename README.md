# HeimdallWeb

## 📋 Visão Geral

HeimdallWeb é uma aplicação web de cibersegurança desenvolvida em ASP.NET Core que realiza varreduras automatizadas de segurança em sites e aplicações web. O projeto tem como objetivo identificar vulnerabilidades, configurações inadequadas e possíveis riscos de segurança através de múltiplos scanners especializados, com análise inteligente dos resultados utilizando IA (Google Gemini).

**Problema que resolve:** Muitas organizações e desenvolvedores não possuem ferramentas acessíveis e integradas para avaliar rapidamente a postura de segurança de suas aplicações web. HeimdallWeb simplifica esse processo ao fornecer uma interface web intuitiva que executa diversas verificações de segurança automaticamente e apresenta os resultados de forma clara e acionável.

**Público-alvo:**
- Desenvolvedores web que desejam validar a segurança de suas aplicações
- Profissionais de segurança da informação realizando auditorias
- Equipes de DevSecOps que necessitam integrar verificações de segurança em seus workflows
- Pequenas e médias empresas buscando avaliar a segurança de seus ativos web

## ✨ Principais Funcionalidades

### Scanners Especializados

1. **Scanner de Cabeçalhos HTTP (HeaderScanner)**
   - Verifica a presença e configuração de cabeçalhos de segurança essenciais
   - Analisa: Strict-Transport-Security, Content-Security-Policy, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy, Cache-Control
   - Identifica cabeçalhos ausentes, presentes ou configurados de forma fraca

2. **Scanner de SSL/TLS (SslScanner)**
   - Valida certificados SSL/TLS em portas HTTPS (443)
   - Verifica validade, data de expiração e emissor dos certificados
   - Detecta certificados expirados ou inválidos

3. **Scanner de Portas (PortScanner)**
   - Realiza varredura de portas comuns e críticas
   - Captura banners de serviços para identificação de tecnologias e versões
   - Portas verificadas incluem: HTTP (80), HTTPS (443), FTP (21), SSH (22), SMTP (25), MySQL (3306), PostgreSQL (5432), MongoDB (27017), Redis (6379), entre outras

4. **Scanner de Redirecionamentos HTTP (HttpRedirectScanner)**
   - Verifica se sites HTTP redirecionam adequadamente para HTTPS
   - Identifica configurações inseguras de redirecionamento

### Análise com Inteligência Artificial

- Integração com **Google Gemini AI** para análise avançada dos resultados
- Interpretação inteligente de vulnerabilidades encontradas
- Classificação automática de riscos em categorias: SSL, Headers, Portas, Redirecionamento, Injeção, Outros
- Geração de relatórios detalhados com recomendações de mitigação

### Sistema de Autenticação e Autorização

- Autenticação baseada em JWT (JSON Web Tokens)
- Sistema de roles para controle de acesso
- Dashboard administrativo para gerenciamento de usuários (role nível 2)
- Cookies seguros para manutenção de sessões

### Histórico e Rastreamento

- Armazenamento de resultados de varreduras em banco de dados MySQL
- Histórico completo de scans realizados por usuário
- Rastreamento de achados (findings) de segurança ao longo do tempo

## 💼 Casos de Uso

### 1. Auditoria Rápida de Segurança Web

Um desenvolvedor termina de implementar uma nova aplicação web e deseja verificar se seguiu as melhores práticas de segurança básicas antes do deploy em produção.

**Fluxo:**
1. Acessa HeimdallWeb e faz login
2. Insere a URL da aplicação em desenvolvimento
3. Executa a varredura
4. Recebe relatório detalhado com:
   - Status dos cabeçalhos de segurança
   - Validade do certificado SSL
   - Portas expostas e serviços identificados
   - Análise de IA com recomendações específicas

### 2. Monitoramento Contínuo de Segurança

Uma equipe de DevSecOps precisa monitorar periodicamente a postura de segurança de múltiplas aplicações em produção.

**Fluxo:**
1. Realiza varreduras periódicas das aplicações
2. Compara resultados históricos para identificar mudanças
3. Detecta novos riscos ou vulnerabilidades introduzidos
4. Gera relatórios para compliance e auditoria

### 3. Avaliação de Fornecedores

Uma empresa precisa avaliar a segurança de aplicações web de fornecedores terceiros antes de integração.

**Fluxo:**
1. Insere URLs das aplicações dos fornecedores
2. Executa varreduras para identificar riscos potenciais
3. Revisa relatórios de IA para entender implicações de segurança
4. Toma decisões informadas sobre integrações e parcerias

### 4. Educação e Conscientização

Um instrutor de segurança web utiliza a ferramenta para demonstrar vulnerabilidades comuns em ambientes de treinamento.

**Fluxo:**
1. Configura aplicações intencionalmente vulneráveis
2. Executa varreduras e mostra resultados em tempo real
3. Demonstra impacto de configurações inadequadas
4. Ensina boas práticas baseadas nas recomendações da IA

## 🚀 Configuração e Instalação

### Pré-requisitos

- **.NET 8.0 SDK** ou superior ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **MySQL Server** 5.7 ou superior
- **Chave de API do Google Gemini** ([Obter chave](https://makersuite.google.com/app/apikey))
- Sistema operacional: Windows, Linux ou macOS

### Dependências do Projeto

O projeto utiliza os seguintes pacotes NuGet:

- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0) - Autenticação JWT
- `Microsoft.EntityFrameworkCore` (9.0.8) - ORM para acesso a dados
- `Microsoft.EntityFrameworkCore.Design` (9.0.8) - Ferramentas de design do EF Core
- `Microsoft.EntityFrameworkCore.Tools` (9.0.8) - Ferramentas CLI do EF Core
- `Newtonsoft.Json` (13.0.3) - Manipulação de JSON
- `Pomelo.EntityFrameworkCore.MySql` (9.0.0) - Provider MySQL para EF Core

### Passo a Passo para Instalação

#### 1. Clonar o Repositório

```bash
git clone https://github.com/alexscarano/HeimdallWeb.git
cd HeimdallWeb
```

#### 2. Configurar o Banco de Dados MySQL

Crie um banco de dados MySQL para a aplicação:

```sql
CREATE DATABASE heimdallweb;
CREATE USER 'heimdall_user'@'localhost' IDENTIFIED BY 'sua_senha_segura';
GRANT ALL PRIVILEGES ON heimdallweb.* TO 'heimdall_user'@'localhost';
FLUSH PRIVILEGES;
```

#### 3. Configurar o arquivo `appsettings.json`

Crie ou edite o arquivo `HeimdallWeb/appsettings.json` com suas configurações:

```json
{
  "ConnectionStrings": {
    "AppDbConnectionString": "Server=localhost;Database=heimdallweb;User=heimdall_user;Password=sua_senha_segura;"
  },
  "Jwt": {
    "Key": "sua_chave_secreta_jwt_com_pelo_menos_32_caracteres",
    "Issuer": "HeimdallWeb",
    "Audience": "HeimdallWebUsers"
  },
  "GEMINI_API_KEY": "sua_chave_api_gemini_aqui",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Notas importantes:**
- A chave JWT deve ser uma string segura com pelo menos 32 caracteres
- Nunca commite o arquivo `appsettings.json` com credenciais reais (ele está no `.gitignore`)
- Para obter uma chave da API Gemini, acesse: https://makersuite.google.com/app/apikey

#### 4. Restaurar Dependências

```bash
cd HeimdallWeb
dotnet restore
```

#### 5. Aplicar Migrações do Banco de Dados

As migrações serão aplicadas automaticamente na primeira execução da aplicação. Alternativamente, você pode aplicá-las manualmente:

```bash
dotnet ef database update
```

Se o comando acima não funcionar, instale a ferramenta EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

#### 6. Compilar o Projeto

```bash
dotnet build
```

#### 7. Executar a Aplicação

**Modo de Desenvolvimento:**

```bash
dotnet run
```

A aplicação estará disponível em:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

**Modo de Produção:**

```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet HeimdallWeb.dll
```

### Configuração Avançada

#### Configurar HTTPS em Produção

Para ambientes de produção, configure um certificado SSL válido:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Para produção real, utilize certificados de autoridades certificadoras confiáveis.

#### Variáveis de Ambiente

Alternativamente, você pode configurar usando variáveis de ambiente:

```bash
# Linux/macOS
export ConnectionStrings__AppDbConnectionString="Server=localhost;Database=heimdallweb;User=heimdall_user;Password=senha;"
export Jwt__Key="sua_chave_jwt"
export GEMINI_API_KEY="sua_chave_gemini"

# Windows (PowerShell)
$env:ConnectionStrings__AppDbConnectionString="Server=localhost;Database=heimdallweb;User=heimdall_user;Password=senha;"
$env:Jwt__Key="sua_chave_jwt"
$env:GEMINI_API_KEY="sua_chave_gemini"
```

#### Docker (Opcional)

Para executar com Docker, você pode criar um `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HeimdallWeb/HeimdallWeb.csproj", "HeimdallWeb/"]
RUN dotnet restore "HeimdallWeb/HeimdallWeb.csproj"
COPY . .
WORKDIR "/src/HeimdallWeb"
RUN dotnet build "HeimdallWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HeimdallWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HeimdallWeb.dll"]
```

Executar com Docker Compose:

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ConnectionStrings__AppDbConnectionString=Server=db;Database=heimdallweb;User=root;Password=senha;
      - GEMINI_API_KEY=sua_chave
      - Jwt__Key=sua_chave_jwt
    depends_on:
      - db
  
  db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: senha
      MYSQL_DATABASE: heimdallweb
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

volumes:
  mysql_data:
```

### Primeiro Acesso

1. Acesse a aplicação em `https://localhost:5001`
2. Você será redirecionado para a página de login
3. Crie um novo usuário através da interface de registro
4. O primeiro usuário criado pode ser promovido a administrador diretamente no banco de dados:

```sql
UPDATE Users SET Role = 2 WHERE UserId = 1;
```

### Solução de Problemas

#### Erro de Conexão com MySQL

Verifique se o MySQL está rodando:

```bash
# Linux
sudo systemctl status mysql

# Windows
# Verifique o serviço MySQL no Gerenciador de Serviços

# macOS
brew services list | grep mysql
```

#### Erro de Migração do Banco de Dados

Limpe e recrie as migrações:

```bash
dotnet ef database drop
dotnet ef database update
```

#### Erro de Autenticação JWT

Certifique-se de que a chave JWT em `appsettings.json` tem pelo menos 32 caracteres e é a mesma em todas as instâncias da aplicação.

#### Timeout na API Gemini

Verifique:
1. Se a chave API está correta
2. Se há conectividade com a internet
3. Se não há limites de taxa (rate limiting) sendo aplicados

## 📄 Licença

Este projeto está licenciado sob a **GNU General Public License v3.0 (GPL-3.0)**.

Isso significa que você pode:
- ✅ Usar comercialmente
- ✅ Modificar o código
- ✅ Distribuir o software
- ✅ Usar para fins privados

Sob as seguintes condições:
- 📋 Divulgar o código fonte
- 📋 Manter a mesma licença
- 📋 Informar sobre mudanças
- 📋 Incluir aviso de copyright e licença

Para mais detalhes, consulte o arquivo [LICENSE](LICENSE) ou visite: https://www.gnu.org/licenses/gpl-3.0.html

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fazer fork do projeto
2. Criar uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abrir um Pull Request

## 📞 Suporte

Para questões, sugestões ou problemas, por favor:
- Abra uma [issue no GitHub](https://github.com/alexscarano/HeimdallWeb/issues)
- Entre em contato através do perfil do GitHub

## 🔒 Segurança

Se você descobrir uma vulnerabilidade de segurança, por favor **NÃO** abra uma issue pública. Entre em contato diretamente através do GitHub para que possamos endereçar o problema de forma responsável.

---

**Desenvolvido com ❤️ para tornar a web mais segura**
