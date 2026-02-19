# Sprint 6 — UX & Frontend Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implementar as features de UX/Frontend do Sprint 6: sistema de notificações completo (backend + sininho no header), landing page pública, monitor de alvos CRUD, forgot/reset password, suporte/contato, e componentes de polimento (ScoreTimeline, RiskCard, Score na tabela).

**Architecture:** Abordagem B — grupos paralelos por dependência: G0 (backend notificações) → G1+G2 em paralelo (auth/UX + novas páginas) → G3 (componentes de polimento). Backend usa DDD Light + Minimal APIs. Frontend usa Next.js 15 App Router com `nexus-next-js` agent para toda implementação.

**Tech Stack:** C# / .NET 10, Next.js 15, React 19, TypeScript, Tailwind CSS, shadcn/ui, Recharts, React Query, Zod, lucide-react, MCP Playwright (browser tests)

**Design doc:** `docs/plans/2026-02-19-sprint6-ux-frontend-design.md`

**Regras CLAUDE.md:**
- Usar agente `nexus-next-js` para TODA implementação Next.js frontend
- Usar agente `dotnet-backend-expert` para TODA implementação backend C#
- Browser tests OBRIGATÓRIOS após cada grupo frontend (MCP Chrome/Playwright)
- `proxy.ts` (não `middleware.ts`) é o arquivo de proteção de rotas

---

## G0 — Backend: Sistema de Notificações

### Task 1: Entidade Notification + Interface do Repositório

**Agent:** `dotnet-backend-expert`

**Files:**
- Create: `src/HeimdallWeb.Domain/Entities/Notification.cs`
- Create: `src/HeimdallWeb.Domain/Enums/NotificationType.cs`
- Create: `src/HeimdallWeb.Domain/Interfaces/Repositories/INotificationRepository.cs`
- Modify: `src/HeimdallWeb.Domain/Interfaces/IUnitOfWork.cs`

**Step 1: Criar enum `NotificationType`**

```csharp
// src/HeimdallWeb.Domain/Enums/NotificationType.cs
namespace HeimdallWeb.Domain.Enums;

public enum NotificationType
{
    ScanComplete = 1,
    RiskAlert = 2
}
```

**Step 2: Criar entidade `Notification`**

```csharp
// src/HeimdallWeb.Domain/Entities/Notification.cs
namespace HeimdallWeb.Domain.Entities;

public class Notification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core
    private Notification() { }

    public Notification(int userId, string title, string body, NotificationType type)
    {
        UserId = userId;
        Title = title;
        Body = body;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead() => IsRead = true;
}
```

**Step 3: Criar interface `INotificationRepository`**

```csharp
// src/HeimdallWeb.Domain/Interfaces/Repositories/INotificationRepository.cs
namespace HeimdallWeb.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<int> GetUnreadCountAsync(int userId);
    Task<Notification?> GetByIdAsync(int id);
    Task AddAsync(Notification notification);
    Task MarkAllAsReadAsync(int userId);
}
```

**Step 4: Adicionar `Notifications` no `IUnitOfWork`**

No arquivo `IUnitOfWork.cs`, adicionar a propriedade:
```csharp
INotificationRepository Notifications { get; }
```

**Step 5: Build para garantir zero erros**

```bash
dotnet build src/HeimdallWeb.Domain/HeimdallWeb.Domain.csproj
```

Expected: `Build succeeded. 0 Error(s)`

**Step 6: Commit**

```bash
git add src/HeimdallWeb.Domain/
git commit -m "feat(g0): add Notification entity, enum and repository interface"
```

---

### Task 2: EF Core Configuration + Repository + Migration

**Agent:** `dotnet-backend-expert`

**Files:**
- Create: `src/HeimdallWeb.Infrastructure/Data/Configurations/NotificationConfiguration.cs`
- Create: `src/HeimdallWeb.Infrastructure/Repositories/NotificationRepository.cs`
- Modify: `src/HeimdallWeb.Infrastructure/Data/AppDbContext.cs`
- Modify: `src/HeimdallWeb.Infrastructure/UnitOfWork.cs`
- Modify: `src/HeimdallWeb.Infrastructure/DependencyInjection.cs`
- Create: Migration `Sprint6_Notifications`

**Step 1: Criar `NotificationConfiguration`**

```csharp
// src/HeimdallWeb.Infrastructure/Data/Configurations/NotificationConfiguration.cs
namespace HeimdallWeb.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("tb_notification");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("notification_id");
        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(100).IsRequired();
        builder.Property(n => n.Body).HasColumnName("body").HasMaxLength(500).IsRequired();
        builder.Property(n => n.Type).HasColumnName("type").IsRequired();
        builder.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => n.CreatedAt);
    }
}
```

**Step 2: Adicionar `DbSet<Notification>` no `AppDbContext`**

```csharp
public DbSet<Notification> Notifications { get; set; }
```

E no `OnModelCreating`, aplicar a configuração:
```csharp
modelBuilder.ApplyConfiguration(new NotificationConfiguration());
```

**Step 3: Criar `NotificationRepository`**

```csharp
// src/HeimdallWeb.Infrastructure/Repositories/NotificationRepository.cs
namespace HeimdallWeb.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        => await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<Notification?> GetByIdAsync(int id)
        => await _context.Notifications.FindAsync(id);

    public async Task AddAsync(Notification notification)
        => await _context.Notifications.AddAsync(notification);

    public async Task MarkAllAsReadAsync(int userId)
        => await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
}
```

**Step 4: Adicionar no `UnitOfWork`**

```csharp
public INotificationRepository Notifications { get; }

// No construtor, adicionar:
Notifications = new NotificationRepository(context);
```

**Step 5: Registrar no `DependencyInjection.cs` (Infrastructure)**

```csharp
services.AddScoped<INotificationRepository, NotificationRepository>();
```

**Step 6: Criar migration**

```bash
cd src/HeimdallWeb.WebApi
dotnet ef migrations add Sprint6_Notifications --project ../HeimdallWeb.Infrastructure --startup-project .
dotnet ef database update --project ../HeimdallWeb.Infrastructure --startup-project .
```

Expected: Migration criada + tabela `tb_notification` no banco.

**Step 7: Commit**

```bash
git add src/HeimdallWeb.Infrastructure/
git commit -m "feat(g0): add NotificationRepository, EF config and migration Sprint6_Notifications"
```

---

### Task 3: DTOs + Query/Command Handlers de Notificações

**Agent:** `dotnet-backend-expert`

**Files:**
- Create: `src/HeimdallWeb.Application/Notifications/DTOs/NotificationResponse.cs`
- Create: `src/HeimdallWeb.Application/Notifications/Queries/GetNotificationsQuery.cs`
- Create: `src/HeimdallWeb.Application/Notifications/Queries/GetUnreadCountQuery.cs`
- Create: `src/HeimdallWeb.Application/Notifications/Commands/MarkNotificationReadCommand.cs`
- Create: `src/HeimdallWeb.Application/Notifications/Commands/MarkAllReadCommand.cs`
- Modify: `src/HeimdallWeb.Application/DependencyInjection.cs`

**Step 1: Criar DTO `NotificationResponse`**

```csharp
// src/HeimdallWeb.Application/Notifications/DTOs/NotificationResponse.cs
namespace HeimdallWeb.Application.Notifications.DTOs;

public record NotificationResponse(
    int Id,
    string Title,
    string Body,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);
```

**Step 2: Criar `GetNotificationsQuery` + Handler**

```csharp
// Query
public record GetNotificationsQuery(int UserId, int Page = 1, int PageSize = 10);

// Handler
public class GetNotificationsQueryHandler
{
    private readonly IUnitOfWork _uow;
    public GetNotificationsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<NotificationResponse>> Handle(GetNotificationsQuery query)
    {
        var notifications = await _uow.Notifications.GetByUserIdAsync(query.UserId, query.Page, query.PageSize);
        return notifications.Select(n => new NotificationResponse(
            n.Id, n.Title, n.Body, n.Type.ToString(), n.IsRead, n.CreatedAt));
    }
}
```

**Step 3: Criar `GetUnreadCountQuery` + Handler**

```csharp
public record GetUnreadCountQuery(int UserId);

public class GetUnreadCountQueryHandler
{
    private readonly IUnitOfWork _uow;
    public GetUnreadCountQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(GetUnreadCountQuery query)
        => await _uow.Notifications.GetUnreadCountAsync(query.UserId);
}
```

**Step 4: Criar `MarkNotificationReadCommand` + Handler**

```csharp
public record MarkNotificationReadCommand(int NotificationId, int UserId);

public class MarkNotificationReadCommandHandler
{
    private readonly IUnitOfWork _uow;
    public MarkNotificationReadCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(MarkNotificationReadCommand cmd)
    {
        var notification = await _uow.Notifications.GetByIdAsync(cmd.NotificationId);
        if (notification is null || notification.UserId != cmd.UserId)
            return false;
        notification.MarkAsRead();
        await _uow.SaveAsync();
        return true;
    }
}
```

**Step 5: Criar `MarkAllReadCommand` + Handler**

```csharp
public record MarkAllReadCommand(int UserId);

public class MarkAllReadCommandHandler
{
    private readonly IUnitOfWork _uow;
    public MarkAllReadCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(MarkAllReadCommand cmd)
    {
        await _uow.Notifications.MarkAllAsReadAsync(cmd.UserId);
        await _uow.SaveAsync();
    }
}
```

**Step 6: Registrar handlers no `DependencyInjection.cs` (Application)**

```csharp
services.AddScoped<GetNotificationsQueryHandler>();
services.AddScoped<GetUnreadCountQueryHandler>();
services.AddScoped<MarkNotificationReadCommandHandler>();
services.AddScoped<MarkAllReadCommandHandler>();
```

**Step 7: Build**

```bash
dotnet build src/HeimdallWeb.Application/HeimdallWeb.Application.csproj
```

Expected: `Build succeeded. 0 Error(s)`

**Step 8: Commit**

```bash
git add src/HeimdallWeb.Application/Notifications/
git commit -m "feat(g0): add notification query/command handlers and DTOs"
```

---

### Task 4: Endpoints de Notificações + Integração nos Handlers Existentes

**Agent:** `dotnet-backend-expert`

**Files:**
- Create: `src/HeimdallWeb.WebApi/Endpoints/NotificationEndpoints.cs`
- Modify: `src/HeimdallWeb.WebApi/Program.cs`
- Modify: `src/HeimdallWeb.Application/Scans/Commands/ExecuteScanCommandHandler.cs`
- Modify: `src/HeimdallWeb.Infrastructure/Services/RiskDeltaService.cs`

**Step 1: Criar `NotificationEndpoints.cs`**

```csharp
// src/HeimdallWeb.WebApi/Endpoints/NotificationEndpoints.cs
namespace HeimdallWeb.WebApi.Endpoints;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/", GetNotifications);
        group.MapGet("/unread-count", GetUnreadCount);
        group.MapPatch("/{id}/read", MarkAsRead);
        group.MapPatch("/read-all", MarkAllAsRead);

        return group;
    }

    private static async Task<IResult> GetNotifications(
        HttpContext context,
        GetNotificationsQueryHandler handler,
        int page = 1, int pageSize = 10)
    {
        var userId = GetUserId(context);
        var result = await handler.Handle(new GetNotificationsQuery(userId, page, pageSize));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUnreadCount(
        HttpContext context,
        GetUnreadCountQueryHandler handler)
    {
        var userId = GetUserId(context);
        var count = await handler.Handle(new GetUnreadCountQuery(userId));
        return Results.Ok(new { count });
    }

    private static async Task<IResult> MarkAsRead(
        int id, HttpContext context,
        MarkNotificationReadCommandHandler handler)
    {
        var userId = GetUserId(context);
        var success = await handler.Handle(new MarkNotificationReadCommand(id, userId));
        return success ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> MarkAllAsRead(
        HttpContext context,
        MarkAllReadCommandHandler handler)
    {
        var userId = GetUserId(context);
        await handler.Handle(new MarkAllReadCommand(userId));
        return Results.NoContent();
    }

    private static int GetUserId(HttpContext context)
        => int.Parse(context.User.FindFirst("sub")?.Value ?? "0");
}
```

**Step 2: Registrar no `Program.cs`**

Adicionar após os outros `Map*Endpoints()`:
```csharp
app.MapNotificationEndpoints();
```

**Step 3: Integrar no `ExecuteScanCommandHandler`**

Após salvar o scan e calcular score, adicionar criação de notificação:
```csharp
// Após: await _uow.SaveAsync();
var notification = new Notification(
    command.UserId,
    $"Scan concluído: {command.Target}",
    $"Score: {score} ({grade})",
    NotificationType.ScanComplete);
await _uow.Notifications.AddAsync(notification);
await _uow.SaveAsync();
```

**Step 4: Integrar no `RiskDeltaService`**

Após detectar queda ≥ 10 pontos e salvar o AuditLog, adicionar:
```csharp
var notification = new Notification(
    userId,
    $"Alerta de risco: {target}",
    $"Score caiu {delta} pontos ({previousScore} → {currentScore})",
    NotificationType.RiskAlert);
await _uow.Notifications.AddAsync(notification);
await _uow.SaveAsync();
```

**Step 5: Build completo de todos os projetos**

```bash
dotnet build src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj
```

Expected: `Build succeeded. 0 Error(s)`

**Step 6: Testar endpoints com curl (servidor em background)**

```bash
# Start server
dotnet run --project src/HeimdallWeb.WebApi &
sleep 3

# Login para obter token (usar credenciais de dev)
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alexandrescarano@gmail.com","password":"Admin@123"}' \
  -c cookies.txt

# Testar unread-count
curl -X GET http://localhost:5000/api/v1/notifications/unread-count \
  -b cookies.txt
# Expected: {"count": 0}

# Testar listagem
curl -X GET http://localhost:5000/api/v1/notifications \
  -b cookies.txt
# Expected: []
```

**Step 7: Commit**

```bash
git add src/HeimdallWeb.WebApi/ src/HeimdallWeb.Application/ src/HeimdallWeb.Infrastructure/
git commit -m "feat(g0): add NotificationEndpoints, integrate into ExecuteScanCommandHandler and RiskDeltaService"
```

---

## G1 — Frontend: Auth/UX (Forgot Password + Reset Password + Google Login)

> **Agent:** `nexus-next-js` para todas as tasks deste grupo

### Task 5: API Layer — Auth (forgot-password, reset-password, google)

**Files:**
- Modify: `src/HeimdallWeb.Next/src/lib/api/auth.api.ts`
- Modify: `src/HeimdallWeb.Next/src/lib/hooks/use-auth.ts` (se existir hook de auth)

**Step 1: Adicionar funções ao `auth.api.ts`**

Adicionar ao arquivo existente (não apagar o que já existe):

```typescript
export const authApi = {
  // ... funções existentes ...

  forgotPassword: async (email: string) => {
    const { data } = await apiClient.post('/auth/forgot-password', { email });
    return data;
  },

  resetPassword: async (token: string, newPassword: string) => {
    const { data } = await apiClient.post('/auth/reset-password', { token, newPassword });
    return data;
  },

  loginWithGoogle: async (idToken: string) => {
    const { data } = await apiClient.post('/auth/google', { idToken });
    return data;
  },
};
```

**Step 2: Commit**

```bash
git add src/HeimdallWeb.Next/src/lib/api/auth.api.ts
git commit -m "feat(g1): add forgotPassword, resetPassword, loginWithGoogle to auth API"
```

---

### Task 6: Página Forgot Password

**Files:**
- Create: `src/HeimdallWeb.Next/src/app/(auth)/forgot-password/page.tsx`

**Step 1: Implementar página via `nexus-next-js` agent**

Prompt para o agente:
```
Implementar a página de "Esqueci minha senha" em src/app/(auth)/forgot-password/page.tsx.

Usa o layout /(auth) já existente (mesmo visual do login/register).

Componentes/libs disponíveis: shadcn/ui (Button, Card, Input, Form, Label),
react-hook-form + zod, lucide-react, sonner (toasts), authApi.forgotPassword().

Comportamento:
1. Form com campo "Email" (validação Zod: email válido)
2. Submit chama authApi.forgotPassword(email)
3. Estado de sucesso: esconde form, mostra mensagem
   "Se o email estiver cadastrado, você receberá um link em breve."
4. Estado de erro: toast de erro genérico (nunca revelar se email existe)
5. Botão desabilitado durante loading (isPending)
6. Link "Voltar ao login" → /login no rodapé do card

Design System (design-system.json v2.2.0):
- Light: accent = emerald-600, Dark: accent = indigo-400
- Fundo do card: bg-card, texto: text-foreground
- Botão principal: variant="default"
```

**Step 2: Build do Next.js**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Expected: `✓ Compiled successfully` com rota `/forgot-password` listada.

**Step 3: Browser test (MCP Playwright)**

```
1. Navegar para http://localhost:3000/forgot-password (sem estar logado)
2. Screenshot da página
3. Preencher email inválido → verificar erro de validação Zod
4. Preencher email válido → verificar estado de sucesso
5. Verificar link "Voltar ao login" funciona
6. Verificar responsividade em 375px
```

**Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/src/app/\(auth\)/forgot-password/
git commit -m "feat(g1): add forgot-password page"
```

---

### Task 7: Página Reset Password

**Files:**
- Create: `src/HeimdallWeb.Next/src/app/(auth)/reset-password/page.tsx`

**Step 1: Implementar página via `nexus-next-js` agent**

Prompt para o agente:
```
Implementar a página de redefinição de senha em src/app/(auth)/reset-password/page.tsx.

Usa o layout /(auth) já existente.

Comportamento:
1. Lê token da query string: useSearchParams() → searchParams.get('token')
2. Se token ausente/vazio: mostra erro "Link inválido ou expirado" + botão
   "Solicitar novo link" → /forgot-password
3. Form com dois campos:
   - "Nova senha" (input type="password")
   - "Confirmar senha" (input type="password")
4. Validação Zod: mínimo 8 chars, senha === confirmação
5. Submit chama authApi.resetPassword(token, newPassword)
6. Sucesso: toast "Senha alterada com sucesso!" + redirect para /login após 2s
7. Erro 400/404 (token inválido/expirado): mensagem inline + link para /forgot-password
8. Botão desabilitado durante loading
```

**Step 2: Adicionar `/reset-password` como rota pública no `proxy.ts`**

```typescript
// src/HeimdallWeb.Next/src/proxy.ts
const publicPaths = ["/login", "/register", "/forgot-password", "/reset-password"];
```

**Step 3: Build + Browser test**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Teste MCP:
```
1. Navegar para /reset-password?token=fake-token
2. Preencher senhas que não batem → verificar erro
3. Preencher senhas válidas → verificar comportamento (erro 400 esperado do backend com token fake)
4. Navegar para /reset-password (sem token) → verificar estado de erro
```

**Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/src/app/\(auth\)/reset-password/ src/HeimdallWeb.Next/src/proxy.ts
git commit -m "feat(g1): add reset-password page and update proxy.ts public routes"
```

---

### Task 8: Login Page — Adicionar Google Button + Link "Esqueci Senha"

**Files:**
- Modify: `src/HeimdallWeb.Next/src/app/(auth)/login/page.tsx`

**Step 1: Ler arquivo atual antes de modificar**

```bash
cat src/HeimdallWeb.Next/src/app/\(auth\)/login/page.tsx
```

**Step 2: Modificar via `nexus-next-js` agent**

Prompt para o agente:
```
Modificar src/app/(auth)/login/page.tsx para adicionar:

1. Link "Esqueci minha senha" → /forgot-password
   - Posição: abaixo do campo de senha, alinhado à direita
   - Estilo: text-sm text-muted-foreground hover:text-foreground

2. Separador visual "ou" entre form e botão Google

3. Botão "Continuar com Google":
   - Usa Google Identity Services (GSI)
   - Adicionar script: <Script src="https://accounts.google.com/gsi/client" />
   - Ao clicar, inicializa google.accounts.id.prompt() ou usa botão renderizado pelo GSI
   - Callback recebe credential (id_token) → chama authApi.loginWithGoogle(idToken)
   - Sucesso: mesmo fluxo do login normal (redirect para /scan)
   - NEXT_PUBLIC_GOOGLE_CLIENT_ID vem de .env.local
   - Se NEXT_PUBLIC_GOOGLE_CLIENT_ID não estiver configurado, esconde o botão

Não alterar o form de login existente — apenas adicionar os novos elementos.
```

**Step 3: Adicionar env var ao `.env.local`**

```bash
echo "NEXT_PUBLIC_GOOGLE_CLIENT_ID=seu_client_id_aqui" >> src/HeimdallWeb.Next/.env.local
```

**Step 4: Build + Browser test**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Teste MCP:
```
1. Navegar para /login
2. Screenshot mostrando: form + link "Esqueci senha" + separador + botão Google
3. Clicar "Esqueci minha senha" → verificar redirect para /forgot-password
4. Verificar console sem erros
```

**Step 5: Commit**

```bash
git add src/HeimdallWeb.Next/src/app/\(auth\)/login/
git commit -m "feat(g1): add forgot-password link and Google OAuth button to login page"
```

---

## G2 — Frontend: Novas Páginas (Landing + Monitor + Support + Notification Bell)

> **Agent:** `nexus-next-js` para todas as tasks deste grupo

### Task 9: proxy.ts + Route Group Público + Scan Form Move

**Files:**
- Modify: `src/HeimdallWeb.Next/src/proxy.ts`
- Create: `src/HeimdallWeb.Next/src/app/(public)/layout.tsx`
- Move: `src/HeimdallWeb.Next/src/app/(app)/page.tsx` → `src/HeimdallWeb.Next/src/app/(app)/scan/page.tsx`

**Step 1: Ler `proxy.ts` atual**

Arquivo atual (`src/HeimdallWeb.Next/src/proxy.ts`):
```typescript
const publicPaths = ["/login", "/register"];
```

**Step 2: Atualizar `proxy.ts` via `nexus-next-js` agent**

```typescript
// Paths que NÃO requerem autenticação
const publicPaths = [
  "/login",
  "/register",
  "/forgot-password",
  "/reset-password",
  "/support",
];

// Paths que são completamente públicos (landing page)
// A rota "/" é pública mas redireciona usuários logados para /scan
export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get("authHeimdallCookie")?.value;

  // Rota raiz: pública mas redireciona autenticados para /scan
  if (pathname === "/") {
    if (token) {
      return NextResponse.redirect(new URL("/scan", request.url));
    }
    return NextResponse.next();
  }

  const isPublicPath = publicPaths.some((path) => pathname.startsWith(path));

  if (!isPublicPath && !token) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("redirect", pathname);
    return NextResponse.redirect(loginUrl);
  }

  if (isPublicPath && token) {
    const isForced = request.nextUrl.searchParams.get("force") === "1";
    if (!isForced) {
      return NextResponse.redirect(new URL("/scan", request.url));
    }
  }

  return NextResponse.next();
}
```

**Step 3: Criar layout público `/(public)/layout.tsx`**

Layout minimalista sem sidebar/header do app:
```tsx
// src/app/(public)/layout.tsx
export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-background">
      {/* Header minimalista */}
      <header className="border-b border-border">
        <div className="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between">
          <span className="font-bold text-lg">Heimdall</span>
          <div className="flex gap-2">
            <a href="/login"><Button variant="ghost">Entrar</Button></a>
            <a href="/register"><Button>Começar</Button></a>
          </div>
        </div>
      </header>
      {children}
    </div>
  );
}
```

**Step 4: Mover scan form de `/` para `/scan`**

```bash
mkdir -p src/HeimdallWeb.Next/src/app/\(app\)/scan
cp src/HeimdallWeb.Next/src/app/\(app\)/page.tsx src/HeimdallWeb.Next/src/app/\(app\)/scan/page.tsx
```

Verificar que `/scan/page.tsx` funciona igual ao antigo `/page.tsx`.

**Step 5: Build**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Expected: rotas `/scan`, `/` (landing placeholder), `/support` listadas. 0 errors.

**Step 6: Commit**

```bash
git add src/HeimdallWeb.Next/src/
git commit -m "feat(g2): restructure routes - public route group, move scan form to /scan, update proxy.ts"
```

---

### Task 10: Landing Page

**Files:**
- Create: `src/HeimdallWeb.Next/src/app/(public)/page.tsx`

**Step 1: Implementar via `nexus-next-js` agent com frontend-design skill**

Prompt para o agente (incluir instrução de usar `frontend-design` skill):

```
Implementar a landing page pública em src/app/(public)/page.tsx.
Use a frontend-design skill para alta qualidade visual.

Usa o layout /(public) (header minimalista com logo + Login + Começar).

Design System: design-system.json v2.2.0
- Light accent: emerald-600, Dark accent: indigo-400
- Fundo: bg-background (branco/preto), texto: text-foreground
- Primário: #18181B, cards: bg-card com borda border

Seções (em ordem):

1. HERO
   - Headline grande: "Escaneie. Analise. Proteja."
   - Subtitle: "Detecte vulnerabilidades, analise TLS, headers, portas e muito mais.
     Com IA integrada para análise de riscos."
   - CTA primário: "Começar gratuitamente" → /register (Button verde/indigo)
   - CTA secundário: "Ver como funciona" → scroll para #features (Button outline)
   - Background sutil com gradiente ou grid pattern

2. FEATURES (id="features")
   - Grid 2x2 ou 3+1 de cards
   - Cada card: ícone lucide + título + descrição curta
   - Cards: SSL/TLS Analysis, Security Headers, CSP Analyzer, Port Scanner,
     Domain Age, Subdomain Discovery, AI Risk Analysis
   - Usar bg-card com hover elevação

3. SCORE PREVIEW
   - Headline: "Score de segurança em segundos"
   - Mostrar ScoreGauge animado (importar de @/components/scan/score-gauge)
     com score=87, grade="B" como demonstração estática
   - Texto explicativo das grades A-F

4. CTA FINAL
   - Fundo destacado (bg-muted ou gradient sutil)
   - "Pronto para proteger seu site?"
   - Botão "Criar conta gratuita" → /register

Não usar imagens externas. Ícones: apenas lucide-react.
Responsivo: mobile first.
```

**Step 2: Build + Browser test**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Teste MCP:
```
1. Navegar para http://localhost:3000 (sem estar logado)
2. Screenshot full-page (desktop 1280px)
3. Screenshot mobile (375px)
4. Verificar ScoreGauge animado visível
5. Verificar CTAs redirecionam corretamente
6. Verificar sem console errors
```

**Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/src/app/\(public\)/
git commit -m "feat(g2): add public landing page with hero, features, score preview and CTA"
```

---

### Task 11: Monitor API + Hooks

**Files:**
- Create: `src/HeimdallWeb.Next/src/lib/api/monitor.api.ts`
- Create: `src/HeimdallWeb.Next/src/lib/hooks/use-monitor.ts`

**Step 1: Implementar via `nexus-next-js` agent**

```
Criar src/lib/api/monitor.api.ts e src/lib/hooks/use-monitor.ts.

Endpoints do backend (todos requerem JWT cookie):
- GET    /api/v1/monitor            → lista MonitoredTarget[]
- POST   /api/v1/monitor            → criar { url: string, frequency: string }
- DELETE /api/v1/monitor/{id}       → deletar alvo
- GET    /api/v1/monitor/{id}/history → lista ScanHistory[] do alvo

Types necessários (criar em src/types/ se não existirem):
interface MonitoredTarget {
  id: number;
  url: string;
  frequency: 'Daily' | 'Weekly' | 'Monthly';
  lastCheck: string | null;
  nextCheck: string | null;
  isActive: boolean;
}

monitor.api.ts:
- getMonitoredTargets(): Promise<MonitoredTarget[]>
- createMonitoredTarget(url: string, frequency: string): Promise<MonitoredTarget>
- deleteMonitoredTarget(id: number): Promise<void>
- getMonitorHistory(id: number): Promise<ScanHistory[]>

use-monitor.ts (React Query):
- useMonitoredTargets(): useQuery
- useCreateMonitor(): useMutation com invalidateQueries(['monitor'])
- useDeleteMonitor(): useMutation com invalidateQueries(['monitor'])
- useMonitorHistory(id): useQuery
```

**Step 2: Build**

```bash
cd src/HeimdallWeb.Next && npm run build
```

**Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/src/lib/api/monitor.api.ts src/HeimdallWeb.Next/src/lib/hooks/use-monitor.ts
git commit -m "feat(g2): add monitor API client and React Query hooks"
```

---

### Task 12: Monitor Page

**Files:**
- Create: `src/HeimdallWeb.Next/src/app/(app)/monitor/page.tsx`
- Modify: `src/HeimdallWeb.Next/src/components/layout/Sidebar.tsx`

**Step 1: Implementar via `nexus-next-js` agent**

```
Implementar src/app/(app)/monitor/page.tsx e adicionar link no Sidebar.

Componentes disponíveis: shadcn/ui (Table, Button, Dialog, Sheet, Badge, Select,
DropdownMenu, AlertDialog), lucide-react, use-monitor.ts hooks, date-fns.

Layout da página:
- Header: título "Alvos Monitorados" + botão "Adicionar alvo" (abre Dialog)
- Tabela responsiva com colunas:
  - URL (truncada com tooltip)
  - Frequência (badge: Daily=blue, Weekly=purple, Monthly=orange)
  - Último Check (date-fns formatDistanceToNow ou "Nunca")
  - Próximo Check (formatDistanceToNow ou "—")
  - Status (Badge: Ativo=green, Inativo=zinc)
  - Ações (ver abaixo)

Ações por linha (DropdownMenu com ícone "..."):
- "Ver histórico" → abre Sheet lateral com tabela de scans (useMonitorHistory)
- "Ativar/Desativar" → não há endpoint de toggle no design, omitir por ora
- "Excluir" → abre AlertDialog de confirmação → useDeleteMonitor()

Dialog "Adicionar alvo":
- Campo URL (validação Zod: url válida)
- Select Frequência: Daily / Weekly / Monthly
- Botão "Adicionar" (submit) → useCreateMonitor() → toast + fechar dialog

Sheet "Histórico do alvo":
- Título: "Scans de {url}"
- Tabela simples: Data, Score, Grade, Status
- Usa useMonitorHistory(id)

Estado vazio: mensagem "Nenhum alvo monitorado" + botão "Adicionar primeiro alvo"

Sidebar: adicionar item "Monitor" com ícone Radar (lucide) entre History e Profile.

Loading: Skeleton para tabela (5 linhas fantasmas)
```

**Step 2: Build + Browser test**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Teste MCP:
```
1. Navegar para /monitor (logado)
2. Screenshot do estado vazio
3. Abrir Dialog "Adicionar alvo", preencher URL + frequência
4. Screenshot com Dialog aberto
5. Submit → verificar toast de sucesso (se backend rodando)
6. Screenshot mobile 375px
7. Verificar item "Monitor" no Sidebar
```

**Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/src/app/\(app\)/monitor/ src/HeimdallWeb.Next/src/components/layout/Sidebar.tsx
git commit -m "feat(g2): add monitor CRUD page with history sheet and sidebar link"
```

---

### Task 13: Support Page

**Files:**
- Create: `src/HeimdallWeb.Next/src/app/support/page.tsx`

**Step 1: Verificar se endpoint existe**

```bash
grep -r "support/contact" src/HeimdallWeb.WebApi/Endpoints/
```

Expected: encontrar o endpoint em `SupportEndpoints.cs` (criado no Sprint 5).

**Step 2: Implementar via `nexus-next-js` agent**

```
Implementar src/app/support/page.tsx (rota pública, sem layout do app).

Usa layout raiz (root layout).

Componentes: shadcn/ui (Card, Button, Input, Textarea, Form, Select, Label),
react-hook-form + zod, lucide-react (MessageSquare, Send), sonner.

Layout:
- Header simples com logo + link "Voltar" (← para /)
- Card centralizado (max-w-lg)
- Título: "Fale conosco" + subtítulo

Formulário (Zod):
- Nome (string, min 2)
- Email (email válido)
- Assunto (Select): "Dúvida técnica" | "Bug/Problema" | "Sugestão" | "Outro"
- Mensagem (Textarea, min 20 chars)

Submit → POST /api/v1/support/contact com { name, email, subject, message }
Sucesso: toast "Mensagem enviada!" + limpar formulário + mostrar confirmação
Erro: toast de erro genérico

Botão desabilitado durante loading.
```

**Step 3: Build + Browser test**

Teste MCP:
```
1. Navegar para /support (sem estar logado — deve funcionar)
2. Screenshot do formulário
3. Preencher com dados inválidos → verificar erros Zod
4. Preencher com dados válidos → verificar estado de sucesso
5. Screenshot mobile
```

**Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/src/app/support/
git commit -m "feat(g2): add support/contact public page"
```

---

### Task 14: Notification Bell no Header

**Files:**
- Create: `src/HeimdallWeb.Next/src/lib/api/notifications.api.ts`
- Create: `src/HeimdallWeb.Next/src/lib/hooks/use-notifications.ts`
- Create: `src/HeimdallWeb.Next/src/components/layout/notification-bell.tsx`
- Modify: `src/HeimdallWeb.Next/src/components/layout/Header.tsx`

**Step 1: Ler Header atual**

```bash
cat src/HeimdallWeb.Next/src/components/layout/Header.tsx
```

**Step 2: Implementar via `nexus-next-js` agent**

```
Implementar o sistema de notificações no header.

1. Criar src/lib/api/notifications.api.ts:
   - getNotifications(page?, pageSize?): Promise<NotificationResponse[]>
   - getUnreadCount(): Promise<{ count: number }>
   - markAsRead(id: number): Promise<void>
   - markAllAsRead(): Promise<void>

   interface NotificationResponse {
     id: number;
     title: string;
     body: string;
     type: 'ScanComplete' | 'RiskAlert';
     isRead: boolean;
     createdAt: string;
   }

2. Criar src/lib/hooks/use-notifications.ts:
   - useUnreadCount(): useQuery com refetchInterval: 30000 (30s polling)
   - useNotifications(): useQuery
   - useMarkAsRead(): useMutation com invalidateQueries(['notifications'])
   - useMarkAllAsRead(): useMutation com invalidateQueries(['notifications'])

3. Criar src/components/layout/notification-bell.tsx:
   - Ícone Bell (lucide-react)
   - Badge vermelho com count se unreadCount > 0 (máx "9+" para count > 9)
   - Popover que abre ao clicar no ícone
   - Lista das últimas 10 notificações no Popover:
     * Ícone por tipo: CheckCircle (ScanComplete, verde) | AlertTriangle (RiskAlert, laranja)
     * Título em font-medium
     * Body em text-sm text-muted-foreground
     * Tempo relativo (formatDistanceToNow do date-fns)
     * Fundo levemente destacado se !isRead (bg-muted/50)
     * Clicar no item → markAsRead(id)
   - Footer do Popover: botão "Marcar tudo como lido" → useMarkAllAsRead()
   - Estado vazio: "Nenhuma notificação"

4. Modificar Header.tsx:
   - Importar <NotificationBell />
   - Posicionar ao lado do ThemeToggle (antes dele)
   - Manter o restante do Header intacto

Design: seguir design-system.json v2.2.0
```

**Step 3: Build + Browser test**

Teste MCP:
```
1. Navegar para qualquer página do app (logado)
2. Screenshot do header mostrando: NotificationBell + ThemeToggle
3. Clicar no sininho → verificar dropdown abre
4. Screenshot do dropdown (vazio ou com notificações)
5. Verificar badge some após marcar tudo como lido
6. Verificar sem console errors (polling 30s não causa spam de erros)
```

**Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/src/lib/api/notifications.api.ts \
        src/HeimdallWeb.Next/src/lib/hooks/use-notifications.ts \
        src/HeimdallWeb.Next/src/components/layout/notification-bell.tsx \
        src/HeimdallWeb.Next/src/components/layout/Header.tsx
git commit -m "feat(g2): add notification bell with polling, dropdown and mark-all-read"
```

---

## G3 — Componentes de Polimento

> **Agent:** `nexus-next-js` para todas as tasks deste grupo

### Task 15: Score + Grade na Tabela de Histórico

**Files:**
- Modify: `src/HeimdallWeb.Next/src/components/history/scan-table.tsx`

**Step 1: Ler arquivo atual**

```bash
cat src/HeimdallWeb.Next/src/components/history/scan-table.tsx | head -80
```

**Step 2: Modificar via `nexus-next-js` agent**

```
Modificar src/components/history/scan-table.tsx para adicionar coluna "Score".

GradeBadge já existe em src/components/scan/score-gauge.tsx — importar e usar.

Adicionar coluna entre "Status" e "Ações":
- Header: "Score"
- Cell: <GradeBadge grade={scan.grade} score={scan.score} /> se score existir,
         senão "—" em text-muted-foreground

Verificar que os tipos ScanHistory em src/types/ incluem score: number | null e grade: string | null.
Se não incluírem, adicionar.

Não alterar nenhuma outra coluna ou lógica existente.
```

**Step 3: Build + Browser test**

Teste MCP:
```
1. Navegar para /history (logado com scans existentes)
2. Screenshot mostrando coluna Score com GradeBadge colorido
3. Verificar responsividade mobile (coluna pode ser ocultada em telas pequenas)
```

**Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/src/components/history/scan-table.tsx \
        src/HeimdallWeb.Next/src/types/
git commit -m "feat(g3): add Score/Grade column to history scan table"
```

---

### Task 16: RiskCard Component (modo Simples na página de detalhes)

**Files:**
- Create: `src/HeimdallWeb.Next/src/components/history/risk-cards.tsx`
- Modify: `src/HeimdallWeb.Next/src/app/(app)/history/[id]/page.tsx`

**Step 1: Implementar via `nexus-next-js` agent**

```
Criar src/components/history/risk-cards.tsx e integrar na página de detalhes.

Props do componente:
interface RiskCardsProps {
  findings: Finding[];
}

Comportamento:
- Agrupa findings por severity: Critical, High, Medium, Low, Info
- Renderiza um card por grupo (ordem: Critical → High → Medium → Low → Info)
- Card vazio (sem findings naquela severity) é omitido
- Cada card:
  * Borda colorida à esquerda (border-l-4) + fundo translúcido:
    - Critical: border-red-500 bg-red-500/10
    - High: border-orange-500 bg-orange-500/10
    - Medium: border-yellow-500 bg-yellow-500/10
    - Low: border-blue-500 bg-blue-500/10
    - Info: border-zinc-400 bg-zinc-400/5
  * Header: ícone + nome severity + badge com contagem
  * Lista dos findings (accordion ou lista simples):
    - Título do finding em font-medium
    - Recomendação em text-sm text-muted-foreground (truncada em 2 linhas)
    - Clicar expande detalhes completos (description + evidence)

Integração em src/app/(app)/history/[id]/page.tsx:
- Já existe toggle isAdvancedView (ToggleLeft/ToggleRight)
- Quando isAdvancedView = false (modo Simples): mostrar <RiskCards findings={findings} />
- Quando isAdvancedView = true (modo Avançado): mostrar <FindingsList /> existente
- O toggle já existe na página — só trocar o conteúdo condicionalmente
```

**Step 2: Build + Browser test**

Teste MCP:
```
1. Navegar para /history/{id} com scan que tem findings
2. Screenshot com toggle no modo Simples (RiskCards visíveis)
3. Clicar no toggle → modo Avançado (FindingsList)
4. Screenshot modo Avançado
5. Verificar animação/transição entre modos
6. Verificar mobile
```

**Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/src/components/history/risk-cards.tsx \
        src/HeimdallWeb.Next/src/app/\(app\)/history/\[id\]/page.tsx
git commit -m "feat(g3): add RiskCards component for simple view in scan details"
```

---

### Task 17: ScoreTimeline Component (tab Evolução na página de detalhes)

**Files:**
- Create: `src/HeimdallWeb.Next/src/components/history/score-timeline.tsx`
- Modify: `src/HeimdallWeb.Next/src/app/(app)/history/[id]/page.tsx`
- Modify: `src/HeimdallWeb.Next/src/lib/hooks/use-history.ts`

**Step 1: Adicionar hook para histórico por target**

```
Modificar src/lib/hooks/use-history.ts para adicionar:

useHistoryByTarget(target: string, enabled: boolean):
  useQuery para GET /api/v1/history?target={target}&pageSize=10&pageNumber=1
  enabled: !!target && enabled (não busca se target vazio)

Verificar se GET /api/v1/history aceita query param ?target= para filtragem.
Se não aceitar, usar GET /api/v1/history?pageSize=10 e filtrar client-side pelo campo target.
```

**Step 2: Implementar via `nexus-next-js` agent**

```
Criar src/components/history/score-timeline.tsx.

Props:
interface ScoreTimelineProps {
  currentScanId: string;
  target: string;
}

Comportamento:
- Busca últimos 10 scans do mesmo target via useHistoryByTarget(target)
- Recharts LineChart responsivo (ResponsiveContainer width="100%" height={200})
- Eixo X: datas formatadas (dd/MM)
- Eixo Y: 0-100, linhas de grade em 25/50/75
- Linha principal: cor baseada no grade do ponto mais recente
- Dot customizado: colorido por grade (A=emerald, B=indigo, C=yellow, D/F=red)
  * Dot do scan atual: maior (r=8) e com ring
  * Outros dots: r=4
- Tooltip customizado: "Score: {score} ({grade})\n{data formatada}"
- Clicar num dot: se id !== currentScanId, navegar para /history/{id}
- Loading: <Skeleton className="h-[200px] w-full rounded-lg" />
- Estado vazio (só 1 scan): "Apenas um scan deste alvo. Execute novos scans para ver a evolução."

Integração em src/app/(app)/history/[id]/page.tsx:
- Adicionar tab "Evolução" após as tabs existentes (Findings, Tecnologias, IA, JSON)
- <TabsTrigger value="timeline">Evolução</TabsTrigger>
- <TabsContent value="timeline">
    <ScoreTimeline currentScanId={scanId} target={scan.target} />
  </TabsContent>
- Importar target do scan data
```

**Step 3: Build + Browser test**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Teste MCP:
```
1. Navegar para /history/{id} de um scan que tem outros scans do mesmo target
2. Clicar na tab "Evolução"
3. Screenshot do gráfico de linha com pontos coloridos por grade
4. Hover em um ponto → verificar tooltip
5. Screenshot mobile
6. Verificar sem console errors (Recharts responsivo)
```

**Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/src/components/history/score-timeline.tsx \
        src/HeimdallWeb.Next/src/lib/hooks/use-history.ts \
        src/HeimdallWeb.Next/src/app/\(app\)/history/\[id\]/page.tsx
git commit -m "feat(g3): add ScoreTimeline component with Evolution tab in scan details"
```

---

## Verificação Final

### Task 18: Build Final + Browser Tests Completos

**Step 1: Build backend**

```bash
dotnet build src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj
```

Expected: `Build succeeded. 0 Error(s)`

**Step 2: Build frontend**

```bash
cd src/HeimdallWeb.Next && npm run build
```

Expected: Compilação bem-sucedida. Listar todas as rotas e verificar que incluem:
- `/` (landing pública)
- `/scan` (scan form, antes era `/`)
- `/monitor`
- `/support`
- `/auth/forgot-password`
- `/auth/reset-password`

**Step 3: Checklist de Browser Tests (MCP Chrome)**

Executar sequencialmente (servidor backend + frontend rodando):

```bash
# Backend
dotnet run --project src/HeimdallWeb.WebApi &

# Frontend
cd src/HeimdallWeb.Next && npm run dev &
```

| Página | Testar |
|---|---|
| `/` (landing) | Screenshot desktop + mobile, CTAs funcionam |
| `/auth/login` | Link "Esqueci senha", botão Google visível |
| `/auth/forgot-password` | Form + estado de sucesso |
| `/auth/reset-password?token=test` | Estado de token inválido |
| `/scan` | Scan form funciona igual antes |
| `/monitor` | CRUD de alvos, dialog, sheet histórico |
| `/support` | Form + submit |
| Header (qualquer página) | Sininho ao lado do tema toggle, dropdown abre |
| `/history` | Coluna Score/Grade visível |
| `/history/{id}` | Tab Evolução, toggle Simples/Avançado, RiskCards |

**Step 4: Atualizar `plano_migracao.md`**

Marcar Implementation Sprint 6 como completo:

```markdown
### Implementation Sprint 6 — UX & Frontend (Next.js) ✅ COMPLETO (2026-02-XX)
- [x] Landing page: `/(public)/page.tsx` — Hero, Features, ScoreGauge Preview
- [x] Rota `/scan` — Scan form movido de `/` para `/scan`
- [x] Página monitor: `/(app)/monitor/page.tsx` — CRUD completo com histórico
- [x] Página esqueci senha: `/(auth)/forgot-password/page.tsx`
- [x] Página reset senha: `/(auth)/reset-password/page.tsx`
- [x] Botão Google OAuth na página de login
- [x] Página suporte/contato: `/support/page.tsx`
- [x] Backend: `tb_notification` + 4 endpoints + integração em handlers
- [x] `NotificationBell` no Header com polling 30s + dropdown + mark-all-read
- [x] `ScoreTimeline` component — tab "Evolução" em `/history/[id]`
- [x] `RiskCard` component — modo Simples no `/history/[id]`
- [x] Coluna Score/Grade na tabela de histórico `/history`
- [x] `proxy.ts` atualizado com todas as rotas públicas
- [x] Browser tests: todas as novas páginas testadas via MCP Chrome
- [x] Build frontend: 0 erros
- [x] Build backend: 0 erros
```

**Step 5: Commit final**

```bash
git add docs/plano_migracao.md
git commit -m "docs(sprint6): mark Implementation Sprint 6 as complete in migration plan"
```

---

## Referências

- **Design doc:** `docs/plans/2026-02-19-sprint6-ux-frontend-design.md`
- **Design system:** `design-system.json` (v2.2.0)
- **Proxy de rotas:** `src/HeimdallWeb.Next/src/proxy.ts`
- **API client base:** `src/HeimdallWeb.Next/src/lib/api/client.ts`
- **Tipos existentes:** `src/HeimdallWeb.Next/src/types/`
- **Componentes existentes:** `src/HeimdallWeb.Next/src/components/`
- **ScoreGauge + GradeBadge:** `src/HeimdallWeb.Next/src/components/scan/score-gauge.tsx`
- **shadcn/ui docs:** https://ui.shadcn.com/docs
- **Recharts docs:** https://recharts.org/en-US/api
