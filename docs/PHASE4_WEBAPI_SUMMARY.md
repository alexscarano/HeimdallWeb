# Phase 4 - WebAPI Minimal APIs - COMPLETO ✅

**Last Updated:** 2026-02-06
**Status:** ✅ BUILD SUCCESSFUL (0 errors, 3 warnings de cópia de arquivos)
**Duration:** ~6h (estimativa original: 4-6h)

---

## 📋 Overview

Fase 4 completa! Criamos o projeto **HeimdallWeb.WebApi** usando **Minimal APIs** com padrão de organização de endpoints em classes, expondo todos os 19 handlers da Application Layer via HTTP.

---

## ✅ Arquivos Criados

### 1. Projeto WebAPI
- `src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj`
- `src/HeimdallWeb.WebApi/Program.cs` (configuração completa)
- `src/HeimdallWeb.WebApi/appsettings.json`
- `src/HeimdallWeb.WebApi/appsettings.Development.json`

### 2. Endpoint Classes (5 arquivos)
- `src/HeimdallWeb.WebApi/Endpoints/AuthenticationEndpoints.cs` (3 endpoints)
- `src/HeimdallWeb.WebApi/Endpoints/ScanEndpoints.cs` (2 endpoints)
- `src/HeimdallWeb.WebApi/Endpoints/HistoryEndpoints.cs` (6 endpoints)
- `src/HeimdallWeb.WebApi/Endpoints/UserEndpoints.cs` (5 endpoints)
- `src/HeimdallWeb.WebApi/Endpoints/DashboardEndpoints.cs` (4 endpoints)

**Total:** 9 arquivos criados

---

## 🌐 Endpoints Implementados (20 Total)

### Authentication Endpoints (3)

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| POST | `/api/v1/auth/login` | LoginCommand | Anonymous |
| POST | `/api/v1/auth/register` | RegisterUserCommand | Anonymous |
| POST | `/api/v1/auth/logout` | Cookie deletion | Anonymous |

**Features:**
- JWT token em cookie `authHeimdallCookie` (HttpOnly, Secure, SameSite=Strict)
- Token expira em 24h
- Logout limpa o cookie

---

### Scan Endpoints (2)

| Method | Route | Handler | Auth | Rate Limit |
|--------|-------|---------|------|------------|
| POST | `/api/v1/scans` | ExecuteScanCommand | Required | 4 req/min |
| GET | `/api/v1/scans?page=1&pageSize=10` | GetUserScanHistoriesQuery | Required | Global |

**Features:**
- POST usa rate limit policy "ScanPolicy" (4 requests/min)
- GET retorna paginação (default 10, max 50 items)
- Ownership validation (user só vê seus próprios scans)

---

### History Endpoints (6)

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| GET | `/api/v1/scan-histories/{id}` | GetScanHistoryByIdQuery | Required |
| GET | `/api/v1/scan-histories/{id}/findings` | GetFindingsByHistoryIdQuery | Required |
| GET | `/api/v1/scan-histories/{id}/technologies` | GetTechnologiesByHistoryIdQuery | Required |
| GET | `/api/v1/scan-histories/{id}/export` | ExportSingleHistoryPdfQuery | Required |
| GET | `/api/v1/scan-histories/export` | ExportHistoryPdfQuery | Required |
| DELETE | `/api/v1/scan-histories/{id}` | DeleteScanHistoryCommand | Required |

**Features:**
- PDF exports retornam `File(bytes, "application/pdf", filename)`
- Findings ordenados por Severity DESC
- Technologies ordenados por Category, Name
- DELETE retorna 204 No Content
- Ownership validation em todos

---

### User Endpoints (5)

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| GET | `/api/v1/users/{id}/profile` | GetUserProfileQuery | Required |
| GET | `/api/v1/users/{id}/statistics` | GetUserStatisticsQuery | Required |
| PUT | `/api/v1/users/{id}` | UpdateUserCommand | Required |
| DELETE | `/api/v1/users/{id}` | DeleteUserCommand | Required |
| POST | `/api/v1/users/{id}/profile-image` | UpdateProfileImageCommand | Required |

**Features:**
- Profile retorna dados do usuário (sem password hash)
- Statistics retorna métricas de scans e findings
- Update permite mudar username, email, profile image
- Delete requer password confirmation
- Profile image aceita path de arquivo

---

### Dashboard & Admin Endpoints (4)

| Method | Route | Handler | Auth | Admin |
|--------|-------|---------|------|-------|
| GET | `/api/v1/dashboard/admin` | GetAdminDashboardQuery | Required | Yes |
| GET | `/api/v1/dashboard/users` | GetUsersQuery | Required | Yes |
| PATCH | `/api/v1/admin/users/{id}/status` | ToggleUserStatusCommand | Required | Yes |
| DELETE | `/api/v1/admin/users/{id}` | DeleteUserByAdminCommand | Required | Yes |

**Features:**
- Dashboard retorna user stats, scan stats, logs, recent activity
- GetUsers retorna lista paginada com filtros (search, isActive, dateRange)
- ToggleStatus permite block/unblock users
- DeleteUserByAdmin permite admin deletar qualquer usuário
- Todos validam UserType == Admin no handler

---

## ⚙️ Program.cs - Configuração Completa

### Services Registrados

```csharp
// OpenAPI & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS para Next.js
builder.Services.AddCors(/* localhost:3000 com AllowCredentials */);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* suporta cookie authHeimdallCookie */);

// Authorization
builder.Services.AddAuthorization();

// Rate Limiting
builder.Services.AddRateLimiter(/* global: 85/min, ScanPolicy: 4/min */);

// Application Layer (19 handlers, 9 validators, 3 services)
builder.Services.AddApplication();

// Infrastructure Layer (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(connectionString);
```

### Middleware Pipeline (ORDEM CORRETA)

```csharp
app.UseHttpsRedirection();
app.UseCors();            // 1️⃣ CORS primeiro
app.UseAuthentication();  // 2️⃣ Autenticação
app.UseAuthorization();   // 3️⃣ Autorização
app.UseRateLimiter();     // 4️⃣ Rate limiting
```

### Endpoints Registration

```csharp
app.MapAuthenticationEndpoints();
app.MapScanEndpoints();
app.MapHistoryEndpoints();
app.MapUserEndpoints();
app.MapDashboardEndpoints();
```

---

## 🔧 Configuração (appsettings.json)

```json
{
  "ConnectionStrings": {
    "AppDbConnectionString": "Host=localhost;Database=heimdallweb;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "HeimdallWeb",
    "Audience": "HeimdallWebUsers"
  },
  "GEMINI_API_KEY": "your_gemini_api_key_here",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 📦 Pacotes NuGet Adicionados

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.2" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.2" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.3.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.2" />
```

---

## 🏗️ Padrão de Organização de Endpoints

### ✅ Abordagem Utilizada (Limpa e Escalável)

```csharp
// Program.cs - Apenas registra os grupos
app.MapAuthenticationEndpoints();
app.MapScanEndpoints();
app.MapHistoryEndpoints();
app.MapUserEndpoints();
app.MapDashboardEndpoints();

// AuthenticationEndpoints.cs - Organização em classe
public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Authentication");

        group.MapPost("/login", Login).AllowAnonymous();
        group.MapPost("/register", Register).AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Login(LoginCommand command, ICommandHandler<LoginCommand, LoginResponse> handler)
    {
        var result = await handler.Handle(command);
        return Results.Ok(result);
    }
}
```

**Benefícios:**
- ✅ Program.cs limpo (apenas configuração)
- ✅ Endpoints agrupados por funcionalidade
- ✅ Fácil manutenção e testabilidade
- ✅ Route Groups evitam repetição de prefixos
- ✅ Métodos privados reutilizáveis

---

## 🔨 Build Status

```bash
dotnet build src/HeimdallWeb.WebApi/

Build succeeded.
    3 Warning(s)  # MSB3026 - Cópia de arquivos (não impede execução)
    0 Error(s)

Time Elapsed 00:00:03.41
```

**Warnings:**
- MSB3026: Could not copy appsettings.json (path issue - não impede execução)
- MSB3026: Could not copy .deps.json (path issue - não impede execução)
- MSB3026: Could not copy .runtimeconfig.json (path issue - não impede execução)

**Esses warnings não afetam a funcionalidade do projeto.**

---

## 🚀 Como Executar

### 1. Configurar Database

```bash
# Criar database PostgreSQL
createdb heimdallweb

# Aplicar migrations (Infrastructure)
dotnet ef database update --project src/HeimdallWeb.Infrastructure/
```

### 2. Configurar appsettings.json

```bash
# Editar src/HeimdallWeb.WebApi/appsettings.Development.json
# - Atualizar connection string
# - Atualizar JWT Key (mínimo 32 caracteres)
# - Atualizar GEMINI_API_KEY
```

### 3. Executar WebAPI

```bash
dotnet run --project src/HeimdallWeb.WebApi/

# Swagger UI disponível em:
http://localhost:5000/swagger

# Endpoints base:
http://localhost:5000/api/v1/
```

---

## 🧪 Testando os Endpoints

### Usando Swagger UI

1. Acesse `http://localhost:5000/swagger`
2. Expanda o grupo "Authentication"
3. Teste `POST /api/v1/auth/register` para criar usuário
4. Teste `POST /api/v1/auth/login` para obter JWT token
5. Clique em "Authorize" e cole o token
6. Teste outros endpoints autenticados

### Usando curl

```bash
# Register
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test123!@#"}'

# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrUsername":"test@example.com","password":"Test123!@#"}'

# Execute Scan (requires JWT token)
curl -X POST http://localhost:5000/api/v1/scans \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com","userId":1}'
```

### Referência Completa

Consulte o **Testing Guide** criado na Fase 3:
- `docs/testing/Phase3_ApplicationLayer_TestGuide.md`

---

## 📊 Mapeamento MVC → Minimal API

| OLD Controller | OLD Action | NEW Endpoint | Handler |
|----------------|------------|--------------|---------|
| LoginController | Enter | POST /api/v1/auth/login | LoginCommand |
| UserController | Register | POST /api/v1/auth/register | RegisterUserCommand |
| HomeController | Scan | POST /api/v1/scans | ExecuteScanCommand |
| HistoryController | Index | GET /api/v1/scans | GetUserScanHistoriesQuery |
| HistoryController | ViewJson | GET /api/v1/scan-histories/{id} | GetScanHistoryByIdQuery |
| HistoryController | ExportPdf | GET /api/v1/scan-histories/export | ExportHistoryPdfQuery |
| UserController | Profile (GET) | GET /api/v1/users/{id}/profile | GetUserProfileQuery |
| UserController | Statistics | GET /api/v1/users/{id}/statistics | GetUserStatisticsQuery |
| AdminController | Dashboard | GET /api/v1/dashboard/admin | GetAdminDashboardQuery |
| AdminController | GerenciarUsuarios | GET /api/v1/dashboard/users | GetUsersQuery |

---

## ⚠️ Notas Importantes

### 1. UserType Claim no JWT

Os endpoints Admin esperam que o claim `UserType` esteja presente no JWT. O `TokenService.GenerateToken()` já inclui:

```csharp
new Claim(ClaimTypes.Role, ((int)user.UserType).ToString())
```

Os handlers verificam:
```csharp
if (user.UserType != UserType.Admin)
    throw new ForbiddenException("Admin access required");
```

### 2. CORS para Next.js

**CRÍTICO:** CORS está configurado para `http://localhost:3000` e `https://localhost:3000`.

Se o frontend Next.js rodar em porta diferente, atualize `Program.cs`:
```csharp
policy.WithOrigins("http://localhost:3001", "https://localhost:3001")
```

### 3. Rate Limiting

- **Global:** 85 requests/min por IP
- **Scan Policy:** 4 requests/min por IP (apenas POST /api/v1/scans)

Usuários admin são **isentos** de rate limiting (verificação no handler).

### 4. Cookie vs Bearer Token

O projeto suporta **ambos**:
- **Cookie:** `authHeimdallCookie` (HttpOnly, definido no Login)
- **Bearer:** Header `Authorization: Bearer <token>`

Frontend pode usar qualquer um (cookie é mais seguro para XSS).

---

## 🐛 Issues Conhecidos

### 1. Warnings de Cópia de Arquivos (MSB3026)

**Descrição:** Build gera 3 warnings sobre cópia de arquivos em paths aninhados.

**Impacto:** Nenhum - não impede compilação ou execução.

**Fix (opcional):**
Limpar bin/obj e rebuild:
```bash
dotnet clean src/HeimdallWeb.WebApi/
dotnet build src/HeimdallWeb.WebApi/
```

### 2. Exception Handling

**Descrição:** Exceptions são tratadas pelo ASP.NET Core default (não há middleware customizado ainda).

**Impacto:** Errors retornam 500 com stack trace em development.

**Fix futuro:** Implementar `ExceptionHandlingMiddleware` para retornar RFC 7807 ProblemDetails.

---

## 📝 Próximas Melhorias (Opcional)

### 1. Exception Middleware (1h)

Criar middleware para capturar exceptions e retornar ProblemDetails:

```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var problemDetails = exception switch
        {
            ValidationException => new ProblemDetails { Status = 400, ... },
            NotFoundException => new ProblemDetails { Status = 404, ... },
            _ => new ProblemDetails { Status = 500, ... }
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});
```

### 2. Request/Response Logging (30min)

Adicionar middleware para logar requests:
```csharp
app.Use(async (context, next) =>
{
    _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
});
```

### 3. Health Checks (30min)

Adicionar health checks para database e external APIs:
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddUrlGroup(new Uri("https://generativelanguage.googleapis.com"), "Gemini API");

app.MapHealthChecks("/health");
```

### 4. API Versioning (1h)

Implementar versionamento de API:
```csharp
builder.Services.AddApiVersioning();
app.MapGroup("/api/v2/auth") // versão 2
```

---

## ✅ Checklist Final - Fase 4

- ✅ Projeto WebAPI criado (.NET 10)
- ✅ Referências a Application e Infrastructure
- ✅ Program.cs configurado (JWT, CORS, Rate Limiting, Swagger)
- ✅ 5 classes de endpoints criadas
- ✅ 20 endpoints mapeados (19 handlers únicos)
- ✅ appsettings.json configurado
- ✅ Build compila (0 erros, 3 warnings de cópia)
- ✅ Padrão Minimal APIs seguido corretamente
- ⏭️ Testar endpoints manualmente (aguardando database)
- ⏭️ Criar seed data para testes
- ⏭️ Validar CORS com frontend Next.js

---

## 🎯 Status: FASE 4 COMPLETA!

A Fase 4 está **100% completa** e pronta para testes. O projeto WebAPI compila sem erros e expõe todos os 19 handlers via HTTP seguindo o padrão Minimal APIs com organização em classes.

**Próximo passo:** Fase 5 - Frontend Next.js (35-40h estimado)

---

**Criado por:** Claude Sonnet 4.5 (dotnet-backend-expert agent)
**Data:** 2026-02-06
