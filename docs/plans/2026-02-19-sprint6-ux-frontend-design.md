# Sprint 6 — UX & Frontend Design
**Data:** 2026-02-19
**Status:** Aprovado
**Contexto:** Implementation Plan Sprint 6 — continuação após Sprints 1–5 do backend (todos completos)

---

## Resumo Executivo

Sprint 6 implementa as features de UX/Frontend que consomem os novos endpoints criados nos Sprints 1–5 do backend (Score/Grade, Profiles, Monitoring, Google Auth, Email/Password Reset, Notificações). A implementação segue a Abordagem B: grupos paralelos ordenados por dependência.

---

## Decisões de Design

### Rota Raiz `/`
- `/` vira landing page pública (route group `/(public)`)
- Scan form se move para `/(app)/scan/page.tsx` (rota `/scan`)
- Usuário autenticado que acessa `/` → redirect para `/scan`
- `proxy.ts` atualizado: `/` e `/support` são rotas públicas

### Notificações
- Sistema completo com `tb_notification` (persistência de lido/não lido)
- Polling leve a cada 30s para `unread-count`
- Dropdown no Header com últimas 10 notificações

---

## Ordem de Execução

```
G0 (Backend Notificações)
    ↓
G1 (Auth/UX) ←→ G2 (Novas Páginas)   [paralelo após G0]
    ↓
G3 (Componentes de Polimento)
```

---

## Grupo 0 — Backend: Sistema de Notificações

### Entidade `Notification` (`tb_notification`)

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `int` | PK |
| `UserId` | `int` | FK → `tb_user` |
| `Title` | `string(100)` | Título curto |
| `Body` | `string(500)` | Corpo da notificação |
| `Type` | `enum` | `ScanComplete` / `RiskAlert` |
| `IsRead` | `bool` | Default `false` |
| `CreatedAt` | `DateTime` | UTC |

### Novos Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `GET` | `/api/v1/notifications` | JWT | Lista paginada, unread primeiro |
| `GET` | `/api/v1/notifications/unread-count` | JWT | Contagem de não lidos |
| `PATCH` | `/api/v1/notifications/{id}/read` | JWT | Marca uma como lida |
| `PATCH` | `/api/v1/notifications/read-all` | JWT | Marca todas como lidas |

### Integração em Handlers Existentes

- **`ExecuteScanCommandHandler`**: após persistir scan, cria `Notification(ScanComplete)` com título "Scan concluído: {target}" e body "Score: {score} ({grade})"
- **`RiskDeltaService`**: ao detectar queda ≥ 10 pontos, cria `Notification(RiskAlert)` com título "Alerta de risco: {target}" e body "Score caiu {delta} pontos"

### Arquivos Novos (Backend)

```
src/HeimdallWeb.Domain/
  Entities/Notification.cs
  Enums/NotificationType.cs
  Interfaces/Repositories/INotificationRepository.cs

src/HeimdallWeb.Infrastructure/
  Data/Configurations/NotificationConfiguration.cs
  Repositories/NotificationRepository.cs
  Migrations/Sprint6_Notifications

src/HeimdallWeb.Application/
  Notifications/
    Queries/GetNotificationsQuery.cs + Handler
    Queries/GetUnreadCountQuery.cs + Handler
    Commands/MarkNotificationReadCommand.cs + Handler
    Commands/MarkAllReadCommand.cs + Handler
  DTOs/NotificationResponse.cs

src/HeimdallWeb.WebApi/
  Endpoints/NotificationEndpoints.cs
```

---

## Grupo 1 — Auth/UX: Forgot Password + Reset Password + Google Login

### Páginas Novas

#### `/auth/forgot-password/page.tsx`
- Input de email + botão "Enviar link de recuperação"
- Chama `POST /api/v1/auth/forgot-password`
- Estado de sucesso: "Se o email existir, você receberá um link em breve" (sem revelar existência)
- Usa o mesmo layout `/(auth)` do login/register
- Link "Voltar ao login" no rodapé

#### `/auth/reset-password/page.tsx`
- Recebe `?token=` via query string
- Inputs: nova senha + confirmação de senha
- Validação Zod: mínimo 8 chars, deve conter maiúscula + número
- Chama `POST /api/v1/auth/reset-password`
- Sucesso → redirect para `/auth/login` com toast "Senha alterada com sucesso"
- Token inválido/expirado → mensagem de erro + link para solicitar novo

### Mudanças em Páginas Existentes

#### `/(auth)/login/page.tsx`
- Adicionar link "Esqueci minha senha" → `/auth/forgot-password`
- Adicionar botão "Continuar com Google" (ícone Google + texto)
  - Usa Google Identity Services (`accounts.google.com/gsi/client`)
  - Callback recebe `credential` (id_token) → chama `POST /api/v1/auth/google`
  - Mesmo fluxo de login: seta cookie JWT + redirect para `/scan`

### Arquivos Novos (Frontend)

```
src/app/(auth)/
  forgot-password/page.tsx
  reset-password/page.tsx
```

### Mudanças em Arquivos Existentes

```
src/lib/api/auth.api.ts          — adicionar forgotPassword(), resetPassword(), loginWithGoogle()
src/lib/hooks/use-auth.ts        — adicionar mutações correspondentes
src/app/(auth)/login/page.tsx    — link "Esqueci senha" + botão Google
```

---

## Grupo 2 — Novas Páginas: Landing + Monitor + Support + Notification Bell

### Landing Page `/(public)/page.tsx`

**Estrutura do route group:**
```
src/app/(public)/
  layout.tsx    ← header minimalista (logo + Login + Começar)
  page.tsx      ← landing page
```

**Seções da landing:**
1. **Hero**: Headline + subtitle + CTA "Começar gratuitamente" (→ `/register`) + CTA secundário "Ver demo" (scroll para preview)
2. **Features**: Grid 3–4 cards com ícones dos scanners principais (SSL, Headers, CSP, Ports)
3. **Preview**: ScoreGauge animado + print do dashboard com dados mock
4. **CTA Final**: "Crie sua conta grátis" + link para login

**Comportamento:**
- Usuário autenticado: redirect para `/scan`
- `proxy.ts`: adicionar `/` como rota pública

### Monitor Page `/(app)/monitor/page.tsx`

**Layout:**
- Header da página: título + botão "Adicionar alvo"
- Tabela responsiva com colunas: URL, Frequência, Último Check, Próximo Check, Status, Ações

**Ações por linha:**
- "Ver histórico": abre Sheet lateral com tabela de scans históricos do alvo
- Menu "..." com: Ativar/Desativar toggle + Deletar (com confirm Dialog)

**Dialog "Adicionar alvo":**
- Campo URL (validação: deve ser URL válida com https://)
- Select de Frequência: Daily / Weekly / Monthly
- Submit → `POST /api/v1/monitor` → toast de sucesso + refetch

**Arquivos Novos:**
```
src/app/(app)/monitor/page.tsx
src/lib/api/monitor.api.ts         ← GET/POST/DELETE + GET /{id}/history
src/lib/hooks/use-monitor.ts       ← React Query hooks
```

### Support Page `/support/page.tsx`

- Fora de route groups (rota pública sem auth obrigatório)
- Formulário: Nome, Email, Assunto (Select), Mensagem (Textarea)
- Submit → `POST /api/v1/support/contact`
- Sucesso: toast + limpar formulário + mensagem "Mensagem enviada com sucesso!"
- Adicionada no sidebar com ícone MessageSquare

**Arquivo Novo:**
```
src/app/support/page.tsx
```

### Notification Bell (Header)

**Componente `NotificationBell`** em `Header.tsx`:
- Ícone `Bell` do lucide-react com badge de contagem (vermelho) se `unreadCount > 0`
- Posicionado ao lado do `ThemeToggle`
- Polling: `useQuery` com `refetchInterval: 30000` para `/api/v1/notifications/unread-count`

**Dropdown (Popover):**
- Lista das últimas 10 notificações
- Cada item: ícone por tipo (CheckCircle=scan, AlertTriangle=risco) + título + body + tempo relativo (`date-fns/formatDistanceToNow`)
- Notificações não lidas: fundo levemente destacado + ponto azul
- Botão "Marcar tudo como lido" → `PATCH /api/v1/notifications/read-all` → refetch

**Arquivos Novos:**
```
src/components/layout/notification-bell.tsx
src/lib/api/notifications.api.ts
src/lib/hooks/use-notifications.ts
```

**Mudança em arquivo existente:**
```
src/components/layout/Header.tsx   — adicionar <NotificationBell /> ao lado de <ThemeToggle />
```

### `proxy.ts` Updates

Adicionar como rotas públicas:
- `/` (landing)
- `/support`
- `/auth/forgot-password`
- `/auth/reset-password`

---

## Grupo 3 — Componentes de Polimento

### `ScoreTimeline` component

**Local:** novo tab "Evolução" na página `/history/[id]`

**Comportamento:**
- Busca últimos 10 scans do mesmo `target` via `GET /api/v1/history?target={target}&pageSize=10`
- Gráfico de linha (Recharts `LineChart`): eixo X = datas, eixo Y = 0–100
- Pontos coloridos por grade: A=emerald, B=indigo, C=yellow, D/F=red
- Clique num ponto → navega para `/history/{id}` do scan correspondente
- Loading: Skeleton retangular no lugar do gráfico

**Arquivo Novo:**
```
src/components/history/score-timeline.tsx
```

**Mudança em arquivo existente:**
```
src/app/(app)/history/[id]/page.tsx   — adicionar tab "Evolução" com <ScoreTimeline />
```

### `RiskCard` component

**Local:** modo "Simples" (toggle `isAdvancedView = false`) na página de detalhes

**Comportamento:**
- Agrupa findings por severidade (Critical, High, Medium, Low, Info)
- Um card por severidade com borda e fundo colorido:
  - Critical: `border-red-500 bg-red-500/10`
  - High: `border-orange-500 bg-orange-500/10`
  - Medium: `border-yellow-500 bg-yellow-500/10`
  - Low: `border-blue-500 bg-blue-500/10`
  - Info: `border-zinc-400 bg-zinc-400/10`
- Header do card: ícone + nome da severidade + contagem (badge)
- Body: lista simples de findings (título + recomendação curta)
- Substitui `FindingsList` quando `isAdvancedView = false`

**Arquivo Novo:**
```
src/components/history/risk-cards.tsx
```

**Mudança em arquivo existente:**
```
src/app/(app)/history/[id]/page.tsx   — condicional: <RiskCards> vs <FindingsList>
```

### Score + Grade na Tabela de Histórico

**Mudança em arquivo existente:**
```
src/components/history/scan-table.tsx
```
- Adicionar coluna "Score" com `<GradeBadge grade={scan.grade} score={scan.score} />`
- `GradeBadge` já existe em `score-gauge.tsx`, só importar

---

## Checklist de Browser Tests (MCP Chrome)

Após cada grupo:

| Grupo | Páginas a testar |
|---|---|
| G1 | `/auth/login` (link + botão Google), `/auth/forgot-password`, `/auth/reset-password` |
| G2 | `/` (landing pública), `/scan` (form movido), `/monitor`, `/support`, Header (sininho) |
| G3 | `/history/[id]` (tab Evolução, toggle Simple/Advanced), `/history` (coluna Score) |

---

## Dependências e Restrições

- G0 (backend) deve estar completo antes de G1/G2 (notificação bell depende dos endpoints)
- G3 depende que G2 esteja pronto (usa o Header atualizado)
- Todos os componentes seguem o Design System `design-system.json` v2.2.0
- Stack: Next.js 15 + React 19 + shadcn/ui + Tailwind + Recharts + React Query
- `nexus-next-js` agent para toda implementação frontend
- Browser tests obrigatórios após cada grupo (MCP Chrome/Playwright)
