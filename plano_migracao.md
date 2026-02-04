# Plano de Migração: HeimdallWeb MVC → DDD Light + Minimal APIs + Next.js

## 📋 Resumo Executivo

**Objetivo**: Migrar HeimdallWeb de ASP.NET Core MVC monolítico para arquitetura moderna com DDD Light, Minimal APIs e Next.js.

**Estado Atual**:
- ASP.NET Core 8.0 MVC + EF Core 9 + MySQL + Bootstrap + jQuery
- ~20.384 linhas de código C#
- 7 tabelas principais, 14 SQL VIEWs, 52 migrations
- 5 controllers, 9 repositories, 3 serviços principais
- Integrações: Gemini AI, 7 scanners de segurança

**Arquitetura Alvo**:
```
HeimdallWeb/
├── src/
│   ├── HeimdallWeb.Domain/          (Entidades, VOs, interfaces)
│   ├── HeimdallWeb.Application/     (Use cases, DTOs, validações)
│   ├── HeimdallWeb.Infrastructure/  (EF Core, repos, APIs externas)
│   ├── HeimdallWeb.WebApi/         (Minimal APIs, JWT, middlewares)
│   └── HeimdallWeb.Contracts/      (DTOs compartilhados)
└── frontend/
    └── heimdall-nextjs/            (Next.js + TailwindCSS + shadcn/ui)
```

**Stack Alvo**:
- Backend: ASP.NET (latest) + PostgreSQL + Minimal APIs + EF Core
- Frontend: Next.js 15 + React 19 + TailwindCSS + shadcn/ui

**Duração**: 6-7 semanas (~60-70h totais com 2h/dia)
**Disponibilidade**: 2h/dia (10h/semana)

---

## 💡 Realidade da Migração

**Backend é Rápido** (2.5 semanas = 25-30h):
- Muito código já existe e funciona
- Repositories, scanners, lógica de negócio → copiar e adaptar
- Eu gero 80-90% automaticamente, você valida

**Frontend é o Gargalo** (3.5-4 semanas = 35-40h):
- 11 páginas para criar do zero em Next.js
- Componentes, integração API, charts
- Aqui é onde vai o tempo real

**Testing** (1 semana = 10h):
- Testes básicos + validação manual E2E

---

## 🎯 Princípios da Migração

1. **Incremental**: Cada fase deve ser deployável independentemente
2. **Testável**: Validação contínua a cada etapa
3. **Pragmático**: DDD Light, sem overengineering
4. **Reuso**: Aproveitar padrões existentes (Repository, DI)
5. **Zero Breaking Changes**: Não mudar regras de negócio

---

## 🚀 Estratégia de Execução (Otimizada para 2h/dia)

### Backend: RÁPIDO (Copia & Cola + Geração Automática)

**O que já existe e funciona:**
- ✅ 9 Repositories implementados
- ✅ 7 Scanners de segurança funcionais
- ✅ Integração Gemini AI testada
- ✅ Lógica de negócio em ScanService
- ✅ JWT, rate limiting, middlewares
- ✅ 52 migrations, 14 SQL VIEWs

**Estratégia:**
1. **Eu gero** Domain entities (extraindo dos Models)
2. **Eu copio** repositories existentes (adapto para interfaces do Domain)
3. **Eu copio** scanners (sem mudanças)
4. **Eu gero** handlers (baseado em ScanService + Controllers)
5. **Eu gero** Minimal APIs endpoints (mapeando MVC routes)
6. **Você valida** e testa cada etapa (2-3h por fase)

**Resultado**: Backend em 2 semanas (20h suas)

---

### Frontend: DEMORADO (Criar do Zero)

**11 Páginas para implementar:**
1. Login
2. Register
3. Home (scan form)
4. History (lista)
5. History details (JSON viewer)
6. Admin dashboard
7. User dashboard
8. User statistics
9. Profile
10. Admin user management
11. (Layouts e componentes compartilhados)

**Estratégia:**
1. **Eu gero** estrutura base + API client + layout
2. **Eu gero** cada página (80% funcional)
3. **Você ajusta** UI/UX, testa, corrige bugs
4. **Iteramos** até ficar bom

**Resultado**: Frontend em 4 semanas (30-35h suas)

---

### Por que Frontend é o Gargalo?

- **Backend**: Lógica já existe, só reorganizar
- **Frontend**: Criar UI do zero
  - shadcn/ui components precisam configuração
  - Cada página tem particularidades
  - Charts precisam dados reais para testar
  - Responsividade (mobile + desktop)
  - Integração com API precisa validação

---

## 📅 Fases Simplificadas (Foco em Execução)

### **Fase 1: Domain Layer (2-3 dias = 4-6h)**

**Eu gero** (90% automatizado):
- 7 entidades (User, ScanHistory, Finding, Technology, IASummary, AuditLog, UserUsage)
- 3 value objects pragmáticos (ScanTarget, EmailAddress, ScanDuration)
- 7 interfaces de repositório
- 3 exceções de domínio
- Enums (copiar dos existentes)

**Você valida** (4-6h):
- Revisar entidades geradas (1-2h)
- Validar lógica de negócio movida para domain (1-2h)
- Compilar e garantir zero dependências externas (1h)
- Aprovar estrutura (1h)

**Arquivos críticos de referência:**
- `HeimdallWebOld/Models/HistoryModel.cs`
- `HeimdallWebOld/Models/UserModel.cs`
- `HeimdallWebOld/Services/ScanService.cs` (lógica para extrair)

---

### **Fase 2: Infrastructure Layer (1 semana = 10h)**

**Eu faço** (80% automatizado):
- Copiar AppDbContext adaptado para PostgreSQL (UseMySql → UseNpgsql)
- Gerar 7 Fluent API Configurations
- Copiar 9 repositories (adaptar para interfaces do Domain)
- Copiar 7 scanners (zero mudanças)
- Copiar GeminiService (zero mudanças)
- Criar UnitOfWork para transações
- Adaptar 14 SQL VIEWs para sintaxe PostgreSQL

**Você faz** (⚠️ **CRÍTICO** - 10h):
1. **Setup PostgreSQL** (30min):
   - Instalar PostgreSQL local
   - Criar database

2. **Executar migrations** (1h):
   - Rodar `dotnet ef database update`
   - Validar schema criado

3. **⚠️ TESTAR 14 SQL VIEWs** (4h):
   - Executar cada view manualmente
   - Comparar resultados com MySQL
   - Ajustar sintaxe se necessário
   - **ESTE É O MAIOR RISCO**

4. **Testar repositories** (2h):
   - CRUD básico de cada repositório
   - Validar queries funcionando

5. **Testar integrações** (1.5h):
   - Gemini API (fazer 1 scan real)
   - Scanners (executar todos)

6. **Validar performance** (1h):
   - Queries de dashboard
   - JSONB queries

**Arquivos críticos:**
- `HeimdallWebOld/Data/AppDbContext.cs`
- `HeimdallWebOld/SQL/*.sql` (14 views)
- `HeimdallWebOld/Repository/*.cs`
- `HeimdallWebOld/Scanners/*.cs`

---

### **Fase 3: Application Layer (3-4 dias = 6-8h)**

**Eu gero** (90% automatizado):
- **15+ handlers** baseados em `ScanService` + `Controllers`:
  - ExecuteScanCommandHandler (lógica do ScanService)
  - LoginCommandHandler, RegisterUserCommandHandler
  - GetHistoryQuery, GetHistoryByIdQuery
  - GetAdminDashboardQuery, GetUserStatisticsQuery
  - UpdateUserCommand, DeleteUserCommand, ToggleUserStatusCommand
- **Validators** (FluentValidation) para todos requests
- **DTOs** Request/Response (adaptar dos DTOs existentes)
- **AutoMapper** profiles
- UnitOfWork implementation

**Você valida** (6-8h):
- Revisar ExecuteScanCommandHandler (lógica crítica) (2h)
- Validar handlers de autenticação (1h)
- Testar validators (reject inputs inválidos) (1h)
- Validar mapeamentos DTO ↔ Entity (1h)
- Compilar e garantir que tudo funciona (1-2h)

**Arquivos críticos:**
- `HeimdallWebOld/Services/ScanService.cs` (lógica para handlers)
- `HeimdallWebOld/Controllers/*.cs` (mapear para handlers)
- `HeimdallWebOld/DTO/*.cs` (adaptar para Request/Response)

---

### **Fase 4: WebApi - Minimal APIs (2-3 dias = 4-6h)**

**Eu gero** (85% automatizado):
- **5 grupos de endpoints** mapeando controllers MVC:
  - AuthenticationEndpoints (login, register)
  - ScanEndpoints (POST scan, GET scans)
  - HistoryEndpoints (GET list, GET by id, export PDF)
  - UserEndpoints (CRUD usuários)
  - DashboardEndpoints (admin + user stats)
- **Program.cs completo**:
  - JWT authentication (copiar de HostingExtensions)
  - Rate limiting (85 global + 4 scan policy)
  - CORS para Next.js (localhost:3000)
  - Swagger/OpenAPI
- **Middlewares** (exception handling, logging)
- **appsettings.json** (connection string PostgreSQL, JWT config)

**Você testa** (4-6h):
- Testar todos endpoints no Postman/Swagger (2h)
- Validar autenticação (login + JWT) (1h)
- Validar rate limiting (fazer requests em massa) (30min)
- Testar CORS (fazer request do navegador) (30min)
- Validar erros retornam RFC 7807 format (1h)

**Mapeamento MVC → API:**
- `/Home/Scan` → `POST /api/v1/scans`
- `/History/Index` → `GET /api/v1/history`
- `/Login/Index` → `POST /api/v1/auth/login`
- `/Admin/Dashboard` → `GET /api/v1/dashboard/admin`
- (+ 7 outros endpoints)

**Arquivos críticos:**
- `HeimdallWebOld/Extensions/HostingExtensions.cs`
- `HeimdallWebOld/Controllers/*.cs`

---

### **Fase 5: Frontend - Next.js (⚠️ GARGALO - 3.5-4 semanas = 35-40h)**

**Stack**: Next.js 15 + React 19 + TailwindCSS + shadcn/ui + React Query + Zod

**11 Páginas para criar:**
1. Login (2-3h)
2. Register (2-3h)
3. Home + ScanForm (3-4h)
4. History list + paginação (5-6h)
5. History details + JSON viewer (4-5h)
6. Admin dashboard + charts (6-8h)
7. User dashboard + stats (5-6h)
8. Profile + upload imagem (3-4h)
9. Admin user management (4-5h)
10. Layout (Header, Sidebar, Footer) (3-4h)

**Eu gero** (70% das páginas):
- Setup completo (Next.js + shadcn/ui + Tailwind)
- API client (Axios + interceptors)
- Todas 11 páginas (80% funcionais)
- Todos componentes shadcn/ui configurados
- React Query setup
- Formulários com validação Zod

**Você trabalha** (35-40h):
- **Semanas 3-4** (20h):
  - Testar auth flow (login, register, logout)
  - Testar scan (executar + ver loading + resultado)
  - Ajustar UI/UX (cores, espaçamentos, layout)
  - Validar responsividade (mobile/desktop)
- **Semanas 5-6** (20h):
  - Testar history (lista, paginação, detalhes)
  - Testar dashboards com dados reais
  - Ajustar gráficos (Recharts)
  - Validar filtros e buscas
  - Testar profile (edição, upload imagem)
  - Testar admin user management
  - Debugging de bugs visuais

**Arquivos de referência:**
- `HeimdallWebOld/Views/**/*.cshtml` (estrutura das páginas)
- `HeimdallWebOld/wwwroot/ts/**/*.ts` (lógica TypeScript existente)

---

### **Fase 6: Testing & Validation (1 semana = 10h)**

**Eu gero** (50% automatizado):
- Unit tests básicos (Domain entities, validators)
- Integration tests (repositories, migrations)

**Você faz** (10h - **TESTING MANUAL E2E**):

**Checklist Completa**

**Fase 1: Domain**
- [ ] Todas entidades têm lógica de negócio
- [ ] Value objects validam invariantes
- [ ] Exceções de domínio bem definidas
- [ ] Sem dependências de infraestrutura

**Fase 2: Infrastructure**
- [ ] Migrations PostgreSQL executam
- [ ] 14 SQL VIEWs criadas e validadas
- [ ] Repositories retornam dados corretos
- [ ] UnitOfWork commit/rollback funciona
- [ ] Gemini API integrada
- [ ] 7 scanners executam corretamente

**Fase 3: Application**
- [ ] Todos use cases têm handlers
- [ ] Validators rejeitam input inválido
- [ ] DTOs mapeiam corretamente
- [ ] Exception handling consistente
- [ ] Logging estruturado (Serilog)

**Fase 4: WebApi**
- [ ] Endpoints retornam status codes corretos
- [ ] JWT authentication funciona
- [ ] Rate limiting funciona
- [ ] CORS permite Next.js
- [ ] Swagger documentado
- [ ] Erros seguem RFC 7807

**Fase 5: Frontend**
- [ ] Todas páginas renderizam
- [ ] Formulários validam
- [ ] API calls usam React Query
- [ ] Auth state persiste
- [ ] Charts exibem dados
- [ ] Responsive (mobile/desktop)
- [ ] Acessibilidade (WCAG 2.1 AA)

**Fase 6: End-to-End**
- [ ] Usuário registra e faz login
- [ ] Usuário executa scan e vê resultados
- [ ] Admin vê dashboard
- [ ] Export PDF funciona
- [ ] Rate limiting previne abuso
- [ ] Quota de usuário enforçada (5 scans/dia)

---

## ⚠️ Riscos Técnicos

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Perda de dados MySQL → PostgreSQL | Médio | Alto | Fase dual database, checksums, rollback plan |
| Mudanças na API Gemini | Baixo | Alto | Interface IGeminiService, mock em testes, versionamento |
| Timeouts de scanner | Médio | Médio | Manter 75s timeout, timeouts por scanner, degradação graciosa |
| Rate limiting agressivo | Médio | Baixo | Monitorar métricas, config ajustável, bypass admin |
| CORS com Next.js | Baixo | Médio | Testar cedo, documentar origins, withCredentials |
| Segurança JWT cookie | Baixo | Alto | HttpOnly + Secure + SameSite=Strict |
| 14 SQL VIEWs incompatíveis | Médio | Médio | Migração manual, teste de cada view, índices |
| Upload de imagem quebrado | Baixo | Baixo | Cloud storage (S3/Azure), fallback local |
| Degradação de performance | Médio | Médio | Caching (Redis), índices DB, connection pooling |
| Deadlocks transacionais | Baixo | Médio | Otimizar UnitOfWork, retry policy, monitorar queries lentas |

---

## ❌ Anti-Patterns (O que NÃO fazer)

### Arquitetura
1. ❌ Não criar microserviços (20K LOC = monolito é suficiente)
2. ❌ Não usar event sourcing (CQRS Light basta)
3. ❌ Não implementar DDD completo (evitar VOs para primitivos)
4. ❌ Não criar generic repositories (usar interfaces específicas)
5. ❌ Não usar EF Core para dashboards (manter SQL VIEWs)

### Database
6. ❌ Não usar NoSQL (dados relacionais precisam de RDBMS)
7. ❌ Não dropar MySQL imediatamente (fase dual é crítica)
8. ❌ Não skipar índices (raw_json_result JSONB precisa de GIN index)
9. ❌ Não auto-gerar migrations (revisar cada migration)
10. ❌ Não migrar views com EF (criar manualmente em SQL)

### API
11. ❌ Não usar controllers (Minimal APIs é o padrão)
12. ❌ Não retornar entities (sempre DTOs)
13. ❌ Não ignorar validação (usar FluentValidation)
14. ❌ Não skipar versionamento (usar `/api/v1/`)
15. ❌ Não expor erros internos (RFC 7807 Problem Details)

### Frontend
16. ❌ Não usar Pages Router (App Router é o futuro)
17. ❌ Não misturar SSR/CSR aleatoriamente (saber quando usar Server Components)
18. ❌ Não fazer fetch em Client Components (Server Components ou React Query)
19. ❌ Não armazenar JWT em localStorage (HttpOnly cookies apenas)
20. ❌ Não usar CSS-in-JS (TailwindCSS é mais rápido)

### Testing
21. ❌ Não skipar integration tests (unit tests não bastam)
22. ❌ Não testar detalhes de implementação (testar comportamento)
23. ❌ Não mockar tudo (usar DB real em integration tests - Testcontainers)
24. ❌ Não ignorar E2E tests (critical paths precisam de E2E)
25. ❌ Não testar lógica de UI isolada (usar React Testing Library)

---

## 📅 Timeline REALISTA (2h/dia)

### **Semana 1: Backend Completo - Domain + Infrastructure** (10h)
**Eu faço** (80% automatizado):
- Gero Domain: entidades, VOs, enums, interfaces (2h)
- Gero Infrastructure: AppDbContext, Fluent API configs (2h)
- Copio 9 repositories adaptados para Domain interfaces (1h)
- Copio 7 scanners + GeminiService (1h)

**Você faz**:
- Setup PostgreSQL local (30min)
- Revisar entidades geradas (1h)
- Executar migrations PostgreSQL (1h)
- **⚠️ CRÍTICO**: Testar as 14 SQL VIEWs uma por uma (1.5h)

**Deliverable**: Backend Domain + Infrastructure funcionando

---

### **Semana 2: Backend - Application + WebApi** (10h)
**Eu faço** (85% automatizado):
- Gero todos handlers (ExecuteScan, Login, etc) (2h)
- Gero validators (FluentValidation) (1h)
- Gero DTOs Request/Response (1h)
- Gero todos endpoints Minimal APIs (2h)
- Gero Program.cs completo (JWT, rate limiting, CORS) (1h)

**Você faz**:
- Revisar lógica dos handlers críticos (1h)
- Testar todos endpoints no Postman/Swagger (1.5h)
- Validar autenticação + rate limiting funcionando (30min)

**Deliverable**: API REST completa e funcional

---

### **Semanas 3-4: Frontend - Setup + Auth + Scan** (20h)
**Eu faço** (70% automatizado):
- Setup Next.js + shadcn/ui + TailwindCSS (2h)
- API client (Axios + interceptors) (1h)
- Layout base (Header, Sidebar, Footer) (2h)
- Login page + LoginForm (2h)
- Register page + RegisterForm (2h)
- Home page + ScanForm (2h)

**Você faz**:
- Testar fluxo de login/registro end-to-end (2h)
- Validar scan funcionando (executar + ver resultado) (2h)
- Ajustes de UI/UX conforme preferência (3h)
- Validar responsividade (mobile/desktop) (2h)

**Deliverable**: Frontend básico funcional (auth + scan)

---

### **Semanas 5-6: Frontend - History + Dashboards** (20h)
**Eu faço**:
- History list page + tabela paginada (3h)
- History details page + JSON viewer (3h)
- Admin dashboard + charts (4h)
- User dashboard + stats (3h)

**Você faz**:
- Testar paginação de histórico (1h)
- Validar visualização de JSON detalhado (1h)
- Testar dashboards com dados reais (2h)
- Ajustes visuais dos gráficos (2h)
- Validar filtros e buscas (1h)

**Deliverable**: Todas páginas principais funcionando

---

### **Semana 7: Frontend Final + Profile + Testing** (10h)
**Eu faço**:
- Profile page + edição de usuário (2h)
- Admin user management page (2h)
- Testes unitários básicos (gerados) (1h)

**Você faz**:
- Testar edição de perfil + upload de imagem (1h)
- Testar gerenciamento de usuários (admin) (1h)
- Executar testes E2E manual de todos fluxos (2h)
- Corrigir bugs encontrados (1h)

**Deliverable**: Sistema completo e validado

---

## 📊 Resumo de Tempo

| Fase | Duração | Horas Totais | % Seu Trabalho |
|------|---------|--------------|----------------|
| **Backend (Domain + Infrastructure + Application + WebApi)** | 2 semanas | 20h | 40% (8h você / 12h eu) |
| **Frontend (Setup + Auth + Scan)** | 2 semanas | 20h | 55% (11h você / 9h eu) |
| **Frontend (History + Dashboards)** | 2 semanas | 20h | 50% (10h você / 10h eu) |
| **Frontend Final + Testing** | 1 semana | 10h | 50% (5h você / 5h eu) |

**Total**: 6-7 semanas (~70h totais)
- **Você**: ~34h (49%)
- **Eu**: ~36h (51%)

**Gargalos Reais**:
1. ⚠️ **Semana 1**: Testar 14 SQL VIEWs no PostgreSQL (crítico)
2. ⚠️ **Semanas 3-6**: Frontend (aqui é onde você vai sentir o peso)
3. ⚠️ **Semana 7**: Testing E2E + correção de bugs

---

## 🚀 Estratégia de Deploy

### Blue-Green Deployment

**Blue (Antigo)**: MVC em MySQL
**Green (Novo)**: Minimal APIs + Next.js em PostgreSQL

**Fases**:
1. **Semana 12**: Deploy API + Next.js em staging
2. **Semana 13**: Rodar paralelo (Blue + Green) em produção
   - Usuários antigos: MVC
   - Usuários novos: Next.js
   - Ambos escrevem no mesmo PostgreSQL
3. **Semana 14**: Shift gradual de tráfego (10% → 50% → 100%)
4. **Semana 15**: Descomissionar MVC

### Monitoring

**Métricas**:
- Response time API (P50, P95, P99)
- Query performance PostgreSQL
- Rate limiting rejections
- Auth failures
- Scan success/failure rate
- Gemini API errors

**Triggers de Rollback**:
- Response time > 2s por 5 minutos
- Error rate > 5% por 10 minutos
- Database deadlocks > 10/minuto
- Gemini API errors > 20% dos requests

---

## 📁 Arquivos Críticos para Implementação

1. **`HeimdallWebOld/Services/ScanService.cs`** (266 linhas)
   - Lógica core de orquestração de scan
   - Decompor em: ExecuteScanCommandHandler, ScannerService, ScanSession aggregate

2. **`HeimdallWebOld/Data/AppDbContext.cs`**
   - Entity configurations + 14 SQL VIEWs
   - Migrar para PostgreSQL
   - Criar Fluent API configs em Infrastructure

3. **`HeimdallWebOld/Models/HistoryModel.cs`**
   - Aggregate root para scans
   - Transformar em ScanHistory entity + ScanSession aggregate

4. **`HeimdallWebOld/Extensions/HostingExtensions.cs`**
   - JWT auth, rate limiting, middleware pipeline
   - Padrão para Minimal APIs Program.cs

5. **`HeimdallWebOld/Repository/UserRepository.cs`**
   - Exemplo de repository pattern
   - Template para Infrastructure layer repos

---

## ✅ Critérios de Sucesso

1. **Funcional**: Todas funcionalidades existentes preservadas
2. **Performance**: Response time ≤ 500ms (P95)
3. **Segurança**: JWT HttpOnly, rate limiting, CORS configurado
4. **Escalabilidade**: PostgreSQL com connection pooling, caching
5. **Manutenibilidade**: DDD Light, código testável, SOLID
6. **UX**: Next.js responsivo, acessível (WCAG 2.1 AA)
7. **Deploy**: Blue-green com rollback < 5 minutos
8. **Monitoring**: Métricas, logs estruturados, alertas

---

## 📝 Próximos Passos

1. **Revisar este plano** com stakeholders
2. **Criar repositórios Git** (backend monorepo + frontend separado)
3. **Setup ambiente de desenvolvimento** (PostgreSQL, Node.js, .NET 9)
4. **Iniciar Fase 1**: Criar projeto Domain
5. **Sprints semanais**: Review + retrospectiva

---

**Este é um plano de migração, não uma implementação automática. Cada fase deve ser executada cuidadosamente com validação contínua.**
