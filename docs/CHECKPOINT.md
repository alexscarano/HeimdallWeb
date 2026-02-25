# 📍 Checkpoint — HeimdallWeb Evolution Plan

**Data:** 2026-02-18
**Branch:** `migracao`
**Working dir:** `/home/alex/Documents/WindowsBkp/Dotnet/HeimdallWeb`
**Fonte do plano:** `implementation_plan.md` (raiz) + `docs/plano_migracao.md`

---

## ✅ O que está completo

### Migração original (Fases 1–6.1 do `plano_migracao.md`)
- Backend completo: Domain, Application, Infrastructure, WebApi (Minimal APIs)
- Frontend Next.js completo (Sprints 5.1–5.8)
- Segurança hardening (Sprint 6.1)
- **Sprint 6.2 (E2E manual):** ainda pendente

### Evolution Plan — `implementation_plan.md`

#### Sprint 1 — Core Engine & Scoring ✅
- `RiskWeight` entity + repo + EF config + migration `Sprint1_RiskWeights_ScanScore` ✅ aplicada no DB
- `ScanHistory.Score` (int?) + `ScanHistory.Grade` (string?) + método `SetScore()` ✅
- `ScannerMetadata` record (Key, DisplayName, Category, DefaultTimeout) ✅
- `IScanner.Metadata` property adicionada à interface ✅
- `ScannerManager` refatorado: `Task.WhenAll` + timeout individual por scanner ✅
- `ScoreCalculatorService` (score 0–100, grade A–F, cache MemoryCache 10min) ✅
- `ExecuteScanResponse` com `int? Score` e `string? Grade` ✅
- Seed: 7 risk weights padrão (SSL=1.5, Headers=1.2, Port=1.3, Sensitive=1.4, Redirect=0.9, Robots=0.8, General=1.0) ✅

#### Sprint 2 — Advanced Scan Mode & Profiles ✅
- `ScanProfile` entity + repo + EF config + migration `Sprint2_ScanProfiles` ✅ aplicada no DB
- `GET /api/v1/profiles` endpoint (AllowAnonymous) em `ProfileEndpoints.cs` ✅
- `ExecuteScanCommand/Request/Response` com `int? ProfileId` opcional ✅
- `ScanEndpoints.cs` passa `ProfileId` ao command ✅
- `Program.cs` registra `app.MapProfileEndpoints()` ✅
- Seed: Quick, Standard, Deep (IsSystem=true) ✅

#### Sprint 3 — New Scanners ✅ COMPLETO (2026-02-18)
- 7 novos scanners **implementados e compilando** (0 erros):

| # | Classe | Key | Category | Timeout | Status |
|---|--------|-----|----------|---------|--------|
| 1 | `TlsCapabilityScanner` | TLS | SSL | 10s | ✅ |
| 2 | `CspAnalyzerScanner` | CSP | Headers | 8s | ✅ |
| 3 | `DomainAgeScanner` | DomainAge | General | 10s | ✅ |
| 4 | `IpChangeScanner` | IpChange | General | 8s | ✅ |
| 5 | `ResponseBehaviorScanner` | ResponseBehavior | General | 15s | ✅ |
| 6 | `SubdomainDiscoveryScanner` | Subdomain | General | 15s | ✅ |
| 7 | `SecurityTxtScanner` | SecurityTxt | General | 8s | ✅ |

- `ScannerManager` atualizado: **13 scanners totais** (6 originais + 7 novos)
- Build: 0 erros em Domain, Application, Infrastructure, WebApi

---

## 📋 Próximo sprint a executar

> **Ordem recomendada:** Sprint 1 ✅ → Sprint 5 (Auth) → Sprint 2 ✅ → Sprint 3 ✅ → **Sprint 6 (UX)** → Sprint 4 (Monitoramento)

### 🎯 Sprint 6 — UX & Frontend (Next.js) ← PRÓXIMO

| Tarefa | Descrição |
|--------|-----------|
| Landing page | `/app/(public)/page.tsx` — Hero, Features, Preview |
| `ScoreGauge` | Círculo animado SVG/Canvas com score 0–100 e grade A–F |
| `RiskCard` | Cards coloridos por severidade |
| `Timeline` | Histórico visual de scans |
| Página Monitor | `/app/(app)/monitor/page.tsx` (depende Sprint 4) |
| Forgot Password page | `/app/auth/forgot-password/page.tsx` (depende Sprint 5) |
| Support page | `/app/support/page.tsx` |
| Progress real | SSE ou polling de status durante scan |
| Toggle Simples/Avançado | Esconde JSON bruto, mostra só cards + recomendações |
| Score/Grade no histórico | Exibir `Score` e `Grade` na tabela e detalhes |
| Browser tests | MCP Chrome para todas as novas páginas |
| Build frontend | 0 erros |

### Sprints futuros (não iniciados)

| Sprint | Descrição |
|--------|-----------|
| Sprint 4 | Snapshot, Monitoramento & Cache (`MonitoredTarget`, `RiskSnapshot`, `ScanCache`, Worker) |
| Sprint 5 | Google Auth + Email (MailKit, forgot-password, reset-password) |

---

## 🗄️ Banco de dados local

- **Connection string (Development):** `Host=localhost;Database=db_heimdall;Username=heimdall;Password=heimdall;Port=5432`
- **Arquivo:** `src/HeimdallWeb.WebApi/appsettings.Development.json`
- **Migrations aplicadas:**
  - `20260208174144_InitialWithPublicIds`
  - `20260218135651_Sprint1_RiskWeights_ScanScore`
  - `20260218153933_Sprint2_ScanProfiles`

**Aplicar novas migrations:**
```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/HeimdallWeb.Infrastructure \
  --startup-project src/HeimdallWeb.WebApi
```

---

## 🏗️ Build de verificação

```bash
# Verificar cada camada individualmente (ignorar MSB3021 — permissão Docker no bin/)
dotnet build src/HeimdallWeb.Domain/HeimdallWeb.Domain.csproj 2>&1 | grep "Build "
dotnet build src/HeimdallWeb.Application/HeimdallWeb.Application.csproj 2>&1 | grep "Build "
dotnet build src/HeimdallWeb.Infrastructure/HeimdallWeb.Infrastructure.csproj 2>&1 | grep "Build "

# Verificar erros C# no WebApi
dotnet build src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj 2>&1 | grep "error CS"
```
