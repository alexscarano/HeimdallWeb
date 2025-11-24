# HeimdallWeb

## 📌 Visão Geral do Projeto

HeimdallWeb é uma aplicação web desenvolvida em ASP.NET Core especializada em escaneamento e auditoria de segurança de aplicações web. O sistema oferece uma plataforma robusta para identificação de vulnerabilidades básicas, análise de configurações de segurança e geração de relatórios detalhados.

**Principais capacidades:**
- **Escaneamento automatizado** de aplicações web com múltiplos scanners especializados
- **Dashboard administrativo** com métricas consolidadas e visualização de dados
- **Sistema de logs estruturados** baseado em enumeradores padronizados
- **Exibição amigável de JSON** com syntax highlighting usando Prism.js
- **Arquitetura limpa** utilizando padrões Repository + Services
- **EF Core Views** para consultas otimizadas e mapeamento SQL
- **Análise com IA** integrada ao Google Gemini para interpretação avançada de resultados
- **Sistema de autenticação** baseado em JWT com controle de acesso por roles

O projeto segue princípios de arquitetura limpa, separação de responsabilidades e boas práticas de desenvolvimento, proporcionando uma base sólida para auditoria contínua de segurança web.

---

## ⚙️ Funcionalidades Principais

### 🔍 Scanners Especializados

O HeimdallWeb possui **6 scanners especializados** que trabalham em conjunto para fornecer uma análise completa de segurança:

#### 1. **HeaderScanner** - Análise de Cabeçalhos HTTP
Verifica a presença e configuração adequada de cabeçalhos de segurança essenciais:
- `Strict-Transport-Security` (HSTS)
- `Content-Security-Policy` (CSP)
- `X-Frame-Options`
- `X-Content-Type-Options`
- `Referrer-Policy`
- `Permissions-Policy`
- `Cache-Control`

**Funcionalidades adicionais:**
- Análise de cookies de sessão (flags `HttpOnly`, `Secure`, `SameSite`)
- Identificação de cookies de frameworks comuns (ASP.NET, PHP, JSP)
- Detecção de cabeçalhos fracos ou mal configurados
- Classificação de severidade por cabeçalho

#### 2. **SslScanner** - Validação de Certificados SSL/TLS
- Valida certificados SSL/TLS em portas HTTPS (443)
- Verifica validade temporal, emissor e cadeia de confiança
- Detecta certificados expirados, auto-assinados ou inválidos
- Identifica algoritmos de assinatura fracos (SHA-1, MD5)
- Analisa tamanho de chaves RSA/DSA/ECDSA
- Calcula dias restantes até expiração
- Classifica severidade automaticamente (Crítico, Alto, Médio, Baixo)

#### 3. **PortScanner** - Varredura e Identificação de Serviços
- Realiza scanning paralelo de portas comuns e críticas
- Captura banners de serviços para fingerprinting
- Identifica tecnologias e versões de software expostas
- Suporta até **30 conexões paralelas** para performance otimizada

**Portas monitoradas (25 portas):**
- **Web:** 80 (HTTP), 443 (HTTPS), 8080, 8443
- **FTP/SSH:** 20, 21 (FTP), 22 (SSH/SFTP)
- **Email:** 25 (SMTP), 465 (SMTPS), 587, 110 (POP3), 995, 143 (IMAP), 993
- **DNS:** 53
- **Bancos de dados:** 3306 (MySQL), 5432 (PostgreSQL), 27017 (MongoDB), 1433 (SQL Server), 1521 (Oracle)
- **Cache/Session:** 6379 (Redis), 11211 (Memcached)
- **Painéis:** 2082/2083 (cPanel), 2095/2096 (Webmail)
- **Remoto:** 3389 (RDP)

#### 4. **HttpRedirectScanner** - Verificação de Redirecionamentos
- Testa se sites HTTP redirecionam adequadamente para HTTPS
- Identifica configurações inseguras de redirecionamento
- Valida códigos de status HTTP apropriados (301, 302, 307, 308)
- Verifica se o cabeçalho `Location` aponta corretamente para HTTPS
- Suporta scanning paralelo de múltiplos IPs
- Timeout configurável para conexões (3s por padrão)

#### 5. **RobotsScanner** - Análise de Robots.txt e Sitemap
Scanner inteligente que analisa o arquivo `robots.txt` e identifica potenciais problemas de segurança:

**Verificações realizadas:**
- ✅ Presença do arquivo `robots.txt`
- ✅ Detecção automática de URL do sitemap
- ✅ Identificação de diretórios sensíveis expostos (`/admin`, `/backup`)
- ✅ Análise de diretivas `Disallow`, `Allow`, `Crawl-delay`
- ✅ Detecção de configurações restritivas (`Disallow: /`)
- ✅ Identificação de referências a arquivos sensíveis (`.sql`, `.env`, `/dump`)
- ✅ Análise de tamanho (muito pequeno ou muito grande)
- ✅ Detecção de padrões específicos de WordPress, Joomla, Drupal

**Classificação de alertas:**
- **Alto:** Exposição de diretórios administrativos ou backups
- **Médio:** Bloqueio total de rastreadores
- **Baixo:** Configurações subótimas
- **Informativo:** Detalhes técnicos

#### 6. **SensitivePathsScanner** - Detecção de Caminhos Sensíveis
Scanner avançado que busca por arquivos e diretórios sensíveis com **heurísticas inteligentes** para reduzir falsos positivos:

**Paths verificados (35+ caminhos):**
- **Painéis administrativos:** `/admin`, `/administrator`, `/wp-admin`, `/typo3`, `/joomla/administrator`
- **Arquivos de configuração:** `/.env`, `/config.php`, `/web.config`, `/WEB-INF/web.xml`
- **Controle de versão:** `/.git`, `/.git/config`, `/.svn`, `/.gitignore`
- **Arquivos de informação:** `/phpinfo.php`, `/info.php`, `/test.php`
- **Backups:** `/backup.zip`, `/backup.sql`, `/db.sql`, `/dump.sql`
- **Ferramentas de gerenciamento:** `/phpmyadmin`, `/adminer.php`, `/solr/admin`
- **Monitoramento:** `/server-status`, `/actuator`, `/actuator/health`
- **Frameworks:** WordPress, Joomla, Drupal, Typo3

**Técnicas anti-falso-positivo:**
- Comparação com conteúdo da homepage
- Detecção de páginas 404 customizadas
- Análise de tamanho de resposta
- Verificação de padrões de erro
- Timeout configurável (5s conexão + 8s leitura)

### 🤖 Análise com Inteligência Artificial

- **Integração com Google Gemini AI** para análise contextual e interpretação inteligente
- **Classificação automática de riscos** em categorias: SSL, Headers, Portas, Redirecionamento, Injeção, Outros
- **Geração de relatórios detalhados** com recomendações específicas de mitigação
- **Interpretação semântica** de vulnerabilidades encontradas

### 📊 Dashboard Administrativo

- **Dashboard principal** com métricas consolidadas de todos os scans
- **Mini dashboard por usuário** com estatísticas individualizadas
- **Visualização de estatísticas em tempo real** usando AdminLTE
- **Repositório dedicado** (`DashboardRepository`) com caching via `MemoryCache`
- **Views SQL otimizadas** mapeadas no EF Core para consultas performáticas
- **Gráficos e indicadores** de vulnerabilidades, scans realizados e tendências

### 🎨 Exibição de JSON Estruturado

- **Página dedicada** para visualização de resultados JSON de scans
- **Syntax highlighting** com Prism.js para melhor legibilidade
- **Modal opcional** para detalhes expandidos
- **DTOs estruturados** representando dados de scan de forma organizada
- **Formatação automática** e identação de objetos JSON complexos
- **Suporte a temas** para visualização clara de estruturas aninhadas

### 🔐 Sistema de Autenticação e Autorização

- **Autenticação JWT** (JSON Web Tokens) segura
- **Sistema de roles hierárquico** para controle de acesso granular
- **Dashboard administrativo** restrito (role nível 2)
- **Cookies seguros** com HttpOnly e Secure flags
- **Proteção contra CSRF** e session hijacking

### 📚 Histórico e Rastreamento

- **Armazenamento persistente** de todos os scans em MySQL
- **Histórico completo** por usuário com filtros e buscas
- **Rastreamento temporal** de findings de segurança
- **Comparação de resultados** entre scans diferentes
- **Auditoria completa** de todas as operações realizadas

---

## 🗂️ Arquitetura e Organização do Projeto

O HeimdallWeb segue uma arquitetura em camadas com separação clara de responsabilidades:

### **Controllers**
Camada de apresentação responsável por receber requisições HTTP e orquestrar a lógica de negócio:
- `ScanController` - Gerenciamento de scans e exibição de resultados
- `AuthController` - Autenticação, registro e gerenciamento de sessões
- `AdminController` - Funcionalidades administrativas e dashboard
- `DashboardController` - Estatísticas e métricas consolidadas

### **Services**
Camada de lógica de negócio contendo as regras e processamento:
- `ScanService` - Orquestração de scans e coordenação de scanners
- `AuthService` - Lógica de autenticação, geração de tokens JWT
- `HeaderScannerService` - Análise de cabeçalhos HTTP
- `SslScannerService` - Validação de certificados SSL/TLS
- `PortScannerService` - Scanning de portas e identificação de serviços
- `GeminiService` - Integração com Google Gemini AI
- `LogService` - Gerenciamento centralizado de logs estruturados

### **Repositories**
Camada de acesso a dados, abstração do banco de dados:
- `ScanRepository` - CRUD de scans e findings
- `UserRepository` - Gerenciamento de usuários
- `DashboardRepository` - Consultas otimizadas para dashboard com caching

### **DTOs / ViewModels / Entities**
- **DTOs** (`Data Transfer Objects`) - Transferência de dados entre camadas
- **ViewModels** - Modelos específicos para views do MVC
- **Entities** - Mapeamento direto das tabelas do banco de dados

**Principais entidades:**
- `User` - Usuários do sistema
- `Scan` - Registro de scans realizados
- `Finding` - Vulnerabilidades e achados de segurança
- `DashboardStats` - View SQL para estatísticas (EF Core View)

### **Views (UI)**
Interface do usuário construída com Razor Pages:
- **Dashboard** - `Views/Dashboard/` - Painel administrativo principal
- **Scan Results** - `Views/Scan/` - Exibição de resultados de escaneamento
- **Admin Panel** - `Views/Admin/` - Gerenciamento de usuários e sistema
- **JSON Viewer** - Componente reutilizável com Prism.js para visualização de JSON

### **Padrão Arquitetural**
O projeto segue consistentemente:
- **Repository Pattern** para abstração de dados
- **Service Layer Pattern** para lógica de negócio
- **Dependency Injection** nativo do ASP.NET Core
- **DTO Pattern** para transferência de dados
- **Entity Framework Core** com Code-First Migrations
- **EF Core Views** para consultas SQL otimizadas

---

## 🧩 Diagramas

### Diagrama do Banco de Dados

![Database Diagram](https://github.com/alexscarano/HeimdallWeb/blob/main/Diagrama_Banco_Heimdall.jpg)

**Estrutura do banco de dados:**

O banco de dados é composto por 6 tabelas principais:

1. **`tb_user`** - Armazena informações dos usuários
   - Campos: `user_id`, `username`, `email`, `password`, `user_type`, `created_at`, `updated_at`, `is_active`, `profile_image`

2. **`tb_history`** - Registra todos os scans realizados
   - Campos: `history_id`, `target`, `raw_json_result`, `created_date`, `user_id`, `summary`, `duration`, `has_completed`
   - Relacionamento: `FK → tb_user.user_id`

3. **`tb_finding`** - Armazena vulnerabilidades encontradas
   - Campos: `finding_id`, `type`, `description`, `severity`, `evidence`, `created_at`, `recommendation`, `history_id`
   - Relacionamento: `FK → tb_history.history_id`

4. **`tb_technology`** - Identifica tecnologias detectadas nos scans
   - Campos: `technology_id`, `technology_name`, `version`, `created_at`, `history_id`, `technology_category`, `technology_description`
   - Relacionamento: `FK → tb_history.history_id`

5. **`tb_log`** - Sistema de logging estruturado
   - Campos: `log_id`, `timestamp`, `level`, `source`, `message`, `details`, `user_id`, `history_id`, `remote_ip`, `code`
   - Relacionamentos: `FK → tb_user.user_id`, `FK → tb_history.history_id`

6. **`tb_ia_summary`** - Armazena análises geradas pela IA
   - Campos: `ia_summary_id`, `main_category`, `created_date`, `history_id`, `overall_risk`, `summary_text`, `findings_critical`, `findings_high`, `findings_low`, `findings_medium`, `ia_notes`, `total_findings`
   - Relacionamento: `FK → tb_history.history_id`

7. **`tb_user_usage`** - Controle de rate limiting e uso
   - Campos: `user_usage_id`, `date`, `request_counts`, `user_id`
   - Relacionamento: `FK → tb_user.user_id`

### Diagrama de Classes
<!-- Insira aqui a imagem do diagrama de classes -->
```
[Espaço reservado para o diagrama de classes]
Exemplo: ![Class Diagram](docs/images/class-diagram.png)
```

---

## 🧪 Tecnologias Utilizadas

### **Backend**
- **ASP.NET Core 8.0** - Framework web moderno e performático
- **Entity Framework Core 9.0.8** - ORM para acesso a dados
- **Pomelo.EntityFrameworkCore.MySql 9.0.0** - Provider MySQL para EF Core
- **EF Core Views** - Consultas SQL mapeadas como entidades
- **MemoryCache** - Sistema de caching em memória para otimização
- **Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0** - Autenticação JWT

### **Frontend**
- **Razor Pages** - View engine do ASP.NET Core
- **AdminLTE** (via CDN) - Template administrativo responsivo
- **Bootstrap 5** - Framework CSS para layout responsivo
- **Prism.js** - Syntax highlighting para JSON e código
- **jQuery** - Manipulação DOM e AJAX
- **Chart.js** - Gráficos e visualizações de dados

### **Database**
- **MySQL 5.7+** - Banco de dados relacional
- **EF Core Migrations** - Versionamento de schema

### **Integrações Externas**
- **Google Gemini AI API** - Análise inteligente de vulnerabilidades
- **Newtonsoft.Json 13.0.3** - Serialização/deserialização JSON

---

## 📊 Dashboard Administrativo

### Visão SQL Otimizada
O dashboard utiliza uma **View SQL customizada** mapeada no EF Core para agregação eficiente de dados:

```sql
-- Exemplo conceitual da view
CREATE VIEW DashboardStatsView AS
SELECT 
    u.UserId,
    u.Username,
    COUNT(s.ScanId) as TotalScans,
    COUNT(CASE WHEN f.Severity = 'High' THEN 1 END) as HighVulnerabilities,
    COUNT(CASE WHEN f.Severity = 'Medium' THEN 1 END) as MediumVulnerabilities,
    COUNT(CASE WHEN f.Severity = 'Low' THEN 1 END) as LowVulnerabilities
FROM Users u
LEFT JOIN Scans s ON u.UserId = s.UserId
LEFT JOIN Findings f ON s.ScanId = f.ScanId
GROUP BY u.UserId, u.Username;
```

### Mapeamento EF Core
```csharp
modelBuilder.Entity<DashboardStats>()
    .ToView("DashboardStatsView")
    .HasNoKey();
```

### Repositório com Caching
O `DashboardRepository` implementa caching inteligente usando `MemoryCache`:

```csharp
public class DashboardRepository : IDashboardRepository
{
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _context;
    
    public async Task<DashboardStats> GetStatsAsync()
    {
        return await _cache.GetOrCreateAsync("DashboardStats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _context.DashboardStats.FirstOrDefaultAsync();
        });
    }
}
```

### Funcionalidades do Dashboard
- **Métricas consolidadas** de todos os usuários
- **Estatísticas em tempo real** com auto-refresh configurável
- **Gráficos interativos** de tendências e distribuição de vulnerabilidades
- **Mini dashboards individuais** por usuário
- **Performance otimizada** com caching de 5 minutos

---

```

### Lista de Mensagens Padronizadas

Cada tipo de log possui uma mensagem estruturada pré-definida:

| Tipo | Mensagem | Nível |
|------|----------|-------|
| `ScanStarted` | "Scan iniciado para URL: {url} pelo usuário {userId}" | Information |
| `ScanCompleted` | "Scan {scanId} concluído com sucesso. {findingsCount} findings encontrados" | Information |
| `ScanFailed` | "Falha no scan {scanId}: {errorMessage}" | Error |
| `UserLogin` | "Login bem-sucedido: usuário {username}" | Information |
| `UnauthorizedAccess` | "Tentativa de acesso não autorizado por {ipAddress}" | Warning |
| `DatabaseError` | "Erro de banco de dados: {errorDetails}" | Error |
| `ExternalApiError` | "Falha na API externa {apiName}: {errorMessage}" | Error |

### Como Registrar Logs no Código

```csharp
public class ScanService
{
    private readonly ILogService _logService;
    
    public async Task<Scan> PerformScanAsync(string url, int userId)
    {
        _logService.Log(LogMessageType.ScanStarted, url, userId);
        
        try
        {
            // Lógica do scan
            var scan = await ExecuteScan(url);
            
            _logService.Log(LogMessageType.ScanCompleted, scan.ScanId, scan.Findings.Count);
            return scan;
        }
        catch (Exception ex)
        {
            _logService.Log(LogMessageType.ScanFailed, url, ex.Message);
            throw;
        }
    }
}
```

---

## 🧾 Exibição de JSON Estruturado

### Nova Rota para Visualização
O sistema oferece uma página dedicada para visualização amigável de resultados JSON:

**Rota:** `/Scan/ViewJson/{scanId}`

### Modal Opcional
Além da página completa, há um modal reutilizável que pode ser invocado de qualquer view:

```javascript
function showJsonModal(scanId) {
    $.ajax({
        url: `/api/scan/${scanId}/json`,
        success: function(data) {
            $('#jsonViewer').html(Prism.highlight(
                JSON.stringify(data, null, 2),
                Prism.languages.json,
                'json'
            ));
            $('#jsonModal').modal('show');
        }
    });
}
```

### DTO para Dados Estruturados

```csharp
public class ScanResultDto
{
    public int ScanId { get; set; }
    public string TargetUrl { get; set; }
    public DateTime ScanDate { get; set; }
    public string Status { get; set; }
    
    public HeaderScanResultDto Headers { get; set; }
    public SslScanResultDto SslInfo { get; set; }
    public PortScanResultDto Ports { get; set; }
    public List<FindingDto> Findings { get; set; }
    public AiAnalysisDto AiAnalysis { get; set; }
}

public class FindingDto
{
    public string Type { get; set; }
    public string Severity { get; set; }
    public string Description { get; set; }
    public string Recommendation { get; set; }
}
```

### Uso do Prism.js

O projeto inclui Prism.js para syntax highlighting:

```html
<!-- Inclusão via CDN -->
<link href="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism-tomorrow.min.css" rel="stylesheet" />
<script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/prism.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/components/prism-json.min.js"></script>
```

**Temas disponíveis:**
- `prism-tomorrow.css` - Tema escuro (padrão)
- `prism-okaidia.css` - Tema alternativo
- `prism.css` - Tema claro

---

## 🚀 Como Executar o Projeto

### Pré-requisitos

- **.NET 8.0 SDK** ou superior ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **MySQL Server** 5.7 ou superior
- **Chave de API do Google Gemini** ([Obter chave](https://makersuite.google.com/app/apikey))
- Sistema operacional: Windows, Linux ou macOS

### Passo a Passo

#### 1. Clonar o Repositório

```bash
git clone https://github.com/alexscarano/HeimdallWeb.git
cd HeimdallWeb
```

#### 2. Configurar o Banco de Dados MySQL

```sql
CREATE DATABASE heimdallweb;
CREATE USER 'heimdall_user'@'localhost' IDENTIFIED BY 'sua_senha_segura';
GRANT ALL PRIVILEGES ON heimdallweb.* TO 'heimdall_user'@'localhost';
FLUSH PRIVILEGES;
```

#### 3. Configurar `appsettings.json`

Crie ou edite `HeimdallWeb/appsettings.json`:

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

**⚠️ Importante:** Nunca commite o arquivo `appsettings.json` com credenciais reais.

#### 4. Restaurar Dependências

```bash
cd HeimdallWeb
dotnet restore
```

#### 5. Aplicar Migrações

```bash
dotnet ef database update
```

Caso necessário, instale a ferramenta EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

#### 6. Compilar o Projeto

```bash
dotnet build
```

#### 7. Executar a Aplicação

**Modo Desenvolvimento:**
```bash
dotnet run
```

A aplicação estará disponível em:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

**Modo Produção:**
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet HeimdallWeb.dll
```

### Primeiro Acesso

1. Acesse `https://localhost:5001`
2. Crie um novo usuário através da interface de registro
3. Para promover o primeiro usuário a administrador:

```sql
UPDATE Users SET Role = 2 WHERE UserId = 1;
```

### Solução de Problemas

#### Erro de Conexão com MySQL
```bash
# Linux
sudo systemctl status mysql

# Windows - Verifique o serviço MySQL no Gerenciador de Serviços

# macOS
brew services list | grep mysql
```

#### Erro de Migração
```bash
dotnet ef database drop
dotnet ef database update
```

#### Erro de Autenticação JWT
Certifique-se de que a chave JWT tem pelo menos 32 caracteres.

---

## 📞 Suporte

Para questões técnicas ou problemas:
- Abra uma [issue no GitHub](https://github.com/alexscarano/HeimdallWeb/issues)
- Entre em contato através do perfil do GitHub

## 🔒 Segurança

Se você descobrir uma vulnerabilidade de segurança, **NÃO** abra uma issue pública. Entre em contato diretamente através do GitHub para que possamos endereçar o problema de forma responsável.

---

**Desenvolvido para auditoria e segurança de aplicações web corporativas**
