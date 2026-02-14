# Ambiente de Desenvolvimento Docker — HeimdallWeb

Guia completo para iniciar, operar e resolver problemas do ambiente de desenvolvimento
multi-container do HeimdallWeb com Docker Compose.

---

## Pre-requisitos

- Docker Engine >= 24.0 ou Docker Desktop >= 4.25
- Docker Compose v2 (incluso no Docker Engine >= 23.0)
- Git (para clonar o repositório)

Verificar versões instaladas:

```bash
docker --version
docker compose version
```

---

## Estrutura dos containers

| Servico       | Imagem Base                          | Porta Host | Porta Container | Funcao                         |
|---------------|--------------------------------------|------------|-----------------|--------------------------------|
| `postgres`    | `postgres:16-alpine`                 | 5432       | 5432            | Banco de dados PostgreSQL      |
| `api`         | `mcr.microsoft.com/dotnet/sdk:10.0`  | 5110       | 5110            | Backend .NET 10 Minimal APIs   |
| `frontend`    | `node:20-alpine`                     | 3000       | 3000            | Frontend Next.js 16            |

---

## Primeiro uso

### 1. Configurar variaveis de ambiente

```bash
# Na raiz do repositório
cp .env.example .env
```

Edite `.env` e preencha os valores obrigatórios:

- `POSTGRES_PASSWORD` — senha do PostgreSQL (não use "postgres" em produção)
- `JWT_KEY` — chave secreta JWT com mínimo de 32 caracteres
- `GEMINI_API_KEY` — chave da API Google Gemini

O arquivo `.env` de desenvolvimento já contém valores pré-preenchidos com as credenciais
do `appsettings.json` existente. Para desenvolvimento local, ele pode ser usado diretamente.

### 2. Subir o ambiente

```bash
# Na raiz do repositório
docker compose up -d
```

O Docker Compose irá:
1. Construir as imagens (primeira execução demora ~3-5 minutos)
2. Subir o PostgreSQL e aguardar o healthcheck passar
3. Subir a API (aplica migrations automaticamente e inicia dotnet watch)
4. Subir o frontend (inicia next dev com Fast Refresh)

### 3. Verificar se tudo está rodando

```bash
docker compose ps
```

Todos os serviços devem estar com status `running` (não `exited`).

---

## URLs de acesso

| Recurso             | URL                              |
|---------------------|----------------------------------|
| Frontend            | http://localhost:3000            |
| Backend API         | http://localhost:5110            |
| Swagger UI          | http://localhost:5110/swagger    |
| PostgreSQL (psql)   | localhost:5432 / db_heimdall     |

---

## Comandos do dia-a-dia

### Ver logs em tempo real

```bash
# Todos os serviços
docker compose logs -f

# Apenas a API
docker compose logs -f api

# Apenas o frontend
docker compose logs -f frontend

# Apenas o banco
docker compose logs -f postgres
```

### Reiniciar um serviço especifico

```bash
# Útil quando dotnet watch não detecta uma mudança
docker compose restart api

# Reiniciar frontend (raramente necessário — Fast Refresh é automático)
docker compose restart frontend
```

### Abrir shell em um container

```bash
# API (para rodar comandos dotnet ef, etc.)
docker compose exec api bash

# Frontend (para rodar npm, etc.)
docker compose exec frontend sh

# Banco de dados (psql interativo)
docker compose exec postgres psql -U postgres -d db_heimdall
```

### Rodar migrations manualmente (dentro do container api)

```bash
docker compose exec api bash -c "dotnet ef database update \
  --project /workspace/src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj"
```

### Criar nova migration (dentro do container api)

```bash
docker compose exec api bash -c "dotnet ef migrations add NomeDaMigration \
  --project /workspace/src/HeimdallWeb.Infrastructure/HeimdallWeb.Infrastructure.csproj \
  --startup-project /workspace/src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj"
```

---

## Gerenciar volumes (dados)

### Parar o ambiente (preserva dados)

```bash
docker compose down
```

### Parar e APAGAR todos os dados (volumes)

```bash
# ATENCAO: Isto apaga o banco de dados permanentemente
docker compose down -v
```

### Listar volumes existentes

```bash
docker volume ls | grep heimdall
```

### Fazer backup do banco

```bash
docker compose exec postgres pg_dump -U postgres db_heimdall > backup_$(date +%Y%m%d).sql
```

### Restaurar backup

```bash
docker compose exec -T postgres psql -U postgres db_heimdall < backup_20240101.sql
```

---

## Rebuild das imagens

Necessário quando as dependências mudam (NuGet packages, npm packages) ou quando
os Dockerfiles são alterados.

```bash
# Rebuildar tudo
docker compose build

# Rebuildar apenas a API
docker compose build api

# Rebuildar apenas o frontend
docker compose build frontend

# Rebuild forçando sem cache (limpa tudo)
docker compose build --no-cache api
```

---

## Hot Reload

### Backend (.NET)

O `dotnet watch` detecta mudanças em arquivos `.cs` e recompila automaticamente.
O polling de arquivos está habilitado (`DOTNET_USE_POLLING_FILE_WATCHER=1`) para
garantir compatibilidade com bind mounts no Linux.

Tempo médio de reload: 3-8 segundos.

Se o reload não ocorrer, reinicie o serviço:
```bash
docker compose restart api
```

### Frontend (Next.js)

O Next.js Fast Refresh detecta mudanças em `.tsx`, `.ts`, `.css` e recarrega o
browser automaticamente. O hot reload deve ser instantâneo.

---

## Resolucao de problemas

### Problema: Porta ja em uso

```
Error: port is already allocated
```

Solução: Identifique e pare o processo que está usando a porta:

```bash
# Verificar qual processo usa a porta 5110
ss -tlnp | grep 5110

# Ou parar o processo .NET local (se estiver rodando fora do Docker)
pkill -f "dotnet run"
pkill -f "dotnet watch"
```

### Problema: API nao conecta ao banco

Verifique se o container postgres está healthy:

```bash
docker compose ps postgres
# Status deve ser: running (healthy)
```

Se não estiver healthy, verifique os logs:

```bash
docker compose logs postgres
```

### Problema: Migrations falham na inicializacao

```bash
# Ver logs detalhados da API (inclui output do dotnet ef)
docker compose logs api

# Rodar migrations manualmente para ver o erro completo
docker compose exec api bash -c "dotnet ef database update \
  --project /workspace/src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj --verbose"
```

### Problema: node_modules corrompido no frontend

```bash
# Remove o volume do node_modules e reconstrói
docker compose down
docker volume rm heimdall_node_modules
docker compose up -d --build frontend
```

### Problema: Frontend nao encontra a API

O `next.config.ts` usa a variável `NEXT_BACKEND_URL` para redirecionar as chamadas
`/api/*` para o backend. Dentro do Docker, o valor é `http://api:5110`.

Verifique se a variável está configurada:

```bash
docker compose exec frontend env | grep NEXT_BACKEND_URL
# Deve retornar: NEXT_BACKEND_URL=http://api:5110
```

### Problema: Imagem nao encontrada para .NET 10

Se o Docker reportar que a imagem `mcr.microsoft.com/dotnet/sdk:10.0` não existe:

```bash
docker pull mcr.microsoft.com/dotnet/sdk:10.0
```

Se a imagem não existir ainda (SDK 10.0 pode estar em preview), ajuste o Dockerfile
para usar a versão de preview disponível:

```
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview
```

---

## Arquivos criados por esta configuracao

```
/
├── .env                                    # Variaveis de ambiente (nao comitar)
├── .env.example                            # Template publico de variaveis
├── .dockerignore                           # Exclui arquivos desnecessarios do build context
├── docker-compose.yml                      # Orquestracao principal
├── docker-compose.override.yml             # Overrides de dev (portas, volumes)
├── src/
│   ├── HeimdallWeb.WebApi/
│   │   ├── Dockerfile                      # Multi-stage: restore → dev → build → runtime
│   │   └── entrypoint.sh                   # Aguarda PG + migrations + dotnet watch
│   └── HeimdallWeb.Next/
│       ├── Dockerfile                      # Multi-stage: deps → dev → builder → runner
│       ├── .dockerignore                   # Exclui node_modules e .next do build context
│       └── next.config.ts                  # Atualizado para usar NEXT_BACKEND_URL
└── docs/guides/
    └── DOCKER_DEV.md                       # Este arquivo
```

---

## Diferenca entre development e producao

| Aspecto              | Development (docker-compose)         | Production (docker-compose --profile prod) |
|----------------------|--------------------------------------|--------------------------------------------|
| Imagem backend       | SDK completo (dotnet watch)          | ASP.NET runtime apenas                     |
| Imagem frontend      | Node.js (next dev)                   | Node.js (next start)                       |
| Hot reload           | Ativo (bind mounts)                  | Desativado                                 |
| Swagger UI           | Ativo                                | Desativado                                 |
| Logs                 | Verbosos                             | Apenas Warning+                            |
| HTTPS                | Nao (HTTP puro)                      | Sim (via proxy reverso)                    |
| Secrets              | .env file                            | Docker Secrets / Vault                     |

Para producao, use os stages `runtime` (backend) e `runner` (frontend) dos Dockerfiles.
