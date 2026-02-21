# CLAUDE.md

Guia operacional do HeimdallWeb — leia antes de qualquer tarefa.

---

## Projeto

**HeimdallWeb** é uma plataforma de segurança web com scanning automatizado, scoring de risco e análise por IA.

**Stack:**
- Backend: .NET 10 · DDD Light · Minimal APIs · PostgreSQL · EF Core
- Frontend: Next.js 15 · React 19 · TailwindCSS · shadcn/ui
- AI: Google Gemini API
- Auth: JWT em HttpOnly cookies

**Estado atual:** Consulte `implementation_plan.md` para o sprint ativo e `docs/plano_migracao.md` para histórico de migração.

---

## Arquitetura

```
src/
├── HeimdallWeb.Domain/          Entities, Value Objects, interfaces de repositório
├── HeimdallWeb.Application/     CQRS handlers (commands/queries), services, validators
├── HeimdallWeb.Infrastructure/  EF Core, repositories, scanners, email, external APIs
├── HeimdallWeb.WebApi/          Minimal APIs endpoints, middleware, Program.cs
└── HeimdallWeb.Next/            Next.js 15 App Router (/(app), /(auth), /(public))
```

**Padrões:** CQRS Light · Repository + UnitOfWork · FluentValidation · JWT HttpOnly

---

## Docker (Método padrão de desenvolvimento)

Três containers principais:

| Container | Serviço | Porta | Descrição |
|-----------|---------|-------|-----------|
| `heimdall_postgres` | `postgres` | 5432 | PostgreSQL 16 |
| `heimdall_api` | `api` | 5110 | .NET 10 API (hot reload) |
| `heimdall_frontend` | `frontend` | 3000 | Next.js 15 (fast refresh) |

```bash
# Subir tudo
docker compose up -d

# Subir serviço específico
docker compose up -d api
docker compose up -d frontend

# Logs em tempo real
docker compose logs -f api
docker compose logs -f frontend

# Reiniciar serviço (após mudança de config)
docker compose restart api

# Shell no container
docker compose exec api bash
docker compose exec frontend sh

# Derrubar tudo (mantém volume do banco)
docker compose down

# Reset total (apaga banco)
docker compose down -v
```

> **Nota:** O container `api` aplica migrations automaticamente via `entrypoint.sh` ao subir.

---

## Migrations (.NET EF Core)

Sempre referenciar Infrastructure como projeto de migrations e WebApi como startup:

```bash
# Criar nova migration (rodar na raiz do projeto, fora do Docker)
dotnet ef migrations add NomeDaMigration \
  --project src/HeimdallWeb.Infrastructure \
  --startup-project src/HeimdallWeb.WebApi

# Aplicar migrations manualmente (requer postgres rodando)
dotnet ef database update \
  --project src/HeimdallWeb.Infrastructure \
  --startup-project src/HeimdallWeb.WebApi \
  --connection "Host=localhost;Port=5432;Database=db_heimdall;Username=postgres;Password=postgres"

# Ver SQL gerado
dotnet ef migrations script \
  --project src/HeimdallWeb.Infrastructure \
  --startup-project src/HeimdallWeb.WebApi

# Rollback para migration específica
dotnet ef database update NomeDaMigrationAnterior \
  --project src/HeimdallWeb.Infrastructure \
  --startup-project src/HeimdallWeb.WebApi
```

---

## Agentes Especializados

Use estes agentes proativamente — eles existem para evitar erros:

| Quando | Agente | Para quê |
|--------|--------|----------|
| Qualquer decisão de UI/UX | `designer` | Layout, cores, componentes, responsividade |
| Implementar frontend Next.js | `nexus-next-js` | Páginas, componentes, hooks, API integration |
| Código C# / .NET backend | `dotnet-backend-expert` | Handlers, repos, EF Core, Minimal APIs |
| Schema, queries, migrations | `sql-data-modeling-expert` | PostgreSQL, índices, views, performance |
| Docker containers, compose, networking | `docker-captain` | Setup, debugging de containers, volumes, redes |
| Scripts shell, automações, CLI | `bash-engineer` | Entrypoint scripts, automações, shell scripts |
| Após feature major | `sentinel-cybersec-generalist` | Revisão de segurança, OWASP, boas práticas |
| Infra, CI/CD, segurança de sistema | `linux-devsecops-analyst` | Hardening, pipelines, segurança de infra |

---

## Planejamento de Features

Antes de iniciar qualquer implementação nova:

1. **Usar `superpowers:brainstorming`** para explorar abordagens e alinhar com o usuário
2. **Usar `AskUserQuestion`** para coletar preferências antes de assumir qualquer decisão de design ou arquitetura
3. Só após aprovação do design → usar `superpowers:writing-plans` para criar o plano de implementação

---

## Debugging

**SEMPRE** usar a skill `superpowers:systematic-debugging` antes de propor qualquer correção:

```
Skill("superpowers:systematic-debugging")
```

Não tente corrigir erros no escuro. A skill guia o processo de investigação antes de qualquer mudança.

---

## Regras Críticas

### Frontend
1. **Consulte `designer`** antes de qualquer componente novo ou mudança visual
2. **Use `nexus-next-js`** para implementar
3. **Teste com Playwright MCP** após qualquer mudança frontend:
   - Screenshot desktop + mobile (375px)
   - Verificar console de erros
   - Testar interações (forms, navegação)

### Backend
4. **Teste todos os endpoints** após implementar (curl/Swagger)
5. **Crie testing guide** em `docs/testing/NomeFeature_TestGuide.md`

### Geral
6. **Após feature de segurança:** use `sentinel-cybersec-generalist`
7. **Migrations:** sempre com `--project` e `--startup-project` corretos
8. **JWT:** sempre em HttpOnly cookie, nunca localStorage

---

## Arquivos Chave

| Arquivo | Papel |
|---------|-------|
| `implementation_plan.md` | Sprint atual, features planejadas e status |
| `docs/plano_migracao.md` | Histórico de migração (referência, não checklist) |
| `src/HeimdallWeb.WebApi/Program.cs` | Entry point, DI, middleware pipeline |
| `src/HeimdallWeb.Application/DependencyInjection.cs` | Registro de handlers e services |
| `src/HeimdallWeb.Infrastructure/DependencyInjection.cs` | Registro de repos |
| `src/HeimdallWeb.Infrastructure/Data/AppDbContext.cs` | DbContext, entities, views |
| `src/HeimdallWeb.WebApi/entrypoint.sh` | Startup Docker: migrations + views + watch |
| `docker-compose.yml` | Orquestração dos 3 containers |

---

## Endpoints Ativos

`/api/v1/auth` · `/api/v1/scans` · `/api/v1/scan-histories` · `/api/v1/users`
`/api/v1/dashboard` · `/api/v1/profiles` · `/api/v1/monitor`
`/api/v1/notifications` · `/api/v1/support`

Swagger disponível em `http://localhost:5110/swagger` (apenas em Development).
