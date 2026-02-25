# Sprint 4 — Monitoring & Cache API Testing Guide

**Data:** 2026-02-19
**Sprint:** Implementation Sprint 4 — Snapshot, Monitoramento & Cache
**Build:** 0 erros, 0 warnings

---

## Endpoints Implementados

| Method | URL | Auth | Descrição |
|--------|-----|------|-----------|
| `GET`  | `/api/v1/monitor` | JWT | Listar alvos monitorados do usuário |
| `POST` | `/api/v1/monitor` | JWT | Criar alvo monitorado |
| `DELETE` | `/api/v1/monitor/{id}` | JWT | Remover alvo monitorado |
| `GET`  | `/api/v1/monitor/{id}/history` | JWT | Histórico de snapshots do alvo |

---

## Pré-requisitos

```bash
# 1. Backend rodando
cd /home/alex/Documents/WindowsBkp/Dotnet/HeimdallWeb
dotnet run --project src/HeimdallWeb.WebApi

# 2. Obter JWT token (login)
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{"emailOrLogin":"alexandrescarano@gmail.com","password":"Admin@123"}'

# Salvar cookie JWT para requests subsequentes
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrLogin":"alexandrescarano@gmail.com","password":"Admin@123"}' | jq -r '.token')
```

---

## Test Cases

### 1. POST /api/v1/monitor — Criar alvo monitorado

**Caso 1.1 — Sucesso: Daily**
```bash
curl -X POST http://localhost:5000/api/v1/monitor \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"url":"https://example.com","frequency":"Daily"}'
```
**Resposta esperada:** `201 Created`
```json
{
  "id": 1,
  "url": "https://example.com",
  "frequency": "Daily",
  "lastCheck": null,
  "nextCheck": "2026-02-19T...",
  "isActive": true,
  "createdAt": "2026-02-19T..."
}
```

**Caso 1.2 — Sucesso: Weekly**
```bash
curl -X POST http://localhost:5000/api/v1/monitor \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"url":"https://google.com","frequency":"Weekly"}'
```
**Resposta esperada:** `201 Created`

**Caso 1.3 — Erro: URL duplicada (mesmo usuário)**
```bash
curl -X POST http://localhost:5000/api/v1/monitor \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"url":"https://example.com","frequency":"Daily"}'
```
**Resposta esperada:** `409 Conflict`
```json
{
  "type": "...",
  "title": "Conflict",
  "status": 409,
  "detail": "URL already being monitored"
}
```

**Caso 1.4 — Erro: Frequência inválida**
```bash
curl -X POST http://localhost:5000/api/v1/monitor \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"url":"https://example.com","frequency":"Monthly"}'
```
**Resposta esperada:** `400 Bad Request`

**Caso 1.5 — Erro: URL vazia**
```bash
curl -X POST http://localhost:5000/api/v1/monitor \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"url":"","frequency":"Daily"}'
```
**Resposta esperada:** `400 Bad Request` (validação FluentValidation)

**Caso 1.6 — Erro: Sem autenticação**
```bash
curl -X POST http://localhost:5000/api/v1/monitor \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com","frequency":"Daily"}'
```
**Resposta esperada:** `401 Unauthorized`

---

### 2. GET /api/v1/monitor — Listar alvos monitorados

**Caso 2.1 — Sucesso: Com alvos cadastrados**
```bash
curl http://localhost:5000/api/v1/monitor \
  -H "Authorization: Bearer $TOKEN"
```
**Resposta esperada:** `200 OK`
```json
[
  {
    "id": 1,
    "url": "https://example.com",
    "frequency": "Daily",
    "lastCheck": null,
    "nextCheck": "2026-02-19T...",
    "isActive": true,
    "createdAt": "2026-02-19T..."
  }
]
```

**Caso 2.2 — Sucesso: Lista vazia (usuário sem alvos)**
```bash
# Login com outro usuário sem alvos
curl http://localhost:5000/api/v1/monitor \
  -H "Authorization: Bearer $OUTRO_TOKEN"
```
**Resposta esperada:** `200 OK` com `[]`

**Caso 2.3 — Erro: Sem autenticação**
```bash
curl http://localhost:5000/api/v1/monitor
```
**Resposta esperada:** `401 Unauthorized`

---

### 3. DELETE /api/v1/monitor/{id} — Remover alvo

**Caso 3.1 — Sucesso**
```bash
curl -X DELETE http://localhost:5000/api/v1/monitor/1 \
  -H "Authorization: Bearer $TOKEN"
```
**Resposta esperada:** `204 No Content`

**Caso 3.2 — Erro: ID não existe**
```bash
curl -X DELETE http://localhost:5000/api/v1/monitor/9999 \
  -H "Authorization: Bearer $TOKEN"
```
**Resposta esperada:** `404 Not Found`

**Caso 3.3 — Erro: IDOR (outro usuário tentar deletar)**
```bash
curl -X DELETE http://localhost:5000/api/v1/monitor/1 \
  -H "Authorization: Bearer $OUTRO_TOKEN"
```
**Resposta esperada:** `404 Not Found` (não revela existência, segurança IDOR)

**Caso 3.4 — Erro: Sem autenticação**
```bash
curl -X DELETE http://localhost:5000/api/v1/monitor/1
```
**Resposta esperada:** `401 Unauthorized`

---

### 4. GET /api/v1/monitor/{id}/history — Histórico de snapshots

**Caso 4.1 — Sucesso: Com histórico (após worker rodar)**
```bash
curl http://localhost:5000/api/v1/monitor/1/history \
  -H "Authorization: Bearer $TOKEN"
```
**Resposta esperada:** `200 OK`
```json
[
  {
    "id": 1,
    "score": 85,
    "grade": "B",
    "findingsCount": 3,
    "criticalCount": 0,
    "highCount": 1,
    "createdAt": "2026-02-19T..."
  }
]
```

**Caso 4.2 — Sucesso: Sem histórico ainda (alvo recém criado)**
```bash
curl http://localhost:5000/api/v1/monitor/1/history \
  -H "Authorization: Bearer $TOKEN"
```
**Resposta esperada:** `200 OK` com `[]`

**Caso 4.3 — Erro: IDOR**
```bash
curl http://localhost:5000/api/v1/monitor/1/history \
  -H "Authorization: Bearer $OUTRO_TOKEN"
```
**Resposta esperada:** `404 Not Found`

---

## Teste de Cache (ScanCacheService)

O cache é aplicado automaticamente no endpoint de scan. Para verificar:

**Passo 1 — Primeiro scan (cache miss)**
```bash
# Primeiro request — scan real, mais demorado (~30-90s)
time curl -X POST http://localhost:5000/api/v1/scans \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"target":"https://example.com"}'
```
**Verificar:** Resposta tem `"isCached": false`

**Passo 2 — Segundo scan no mesmo alvo (cache hit)**
```bash
# Segundo request com mesmo target — resposta imediata
time curl -X POST http://localhost:5000/api/v1/scans \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"target":"https://example.com"}'
```
**Verificar:**
- Resposta tem `"isCached": true`
- Tempo de resposta < 100ms (significativamente mais rápido)

---

## Verificação do MonitoringWorker

O worker roda a cada 30 minutos automaticamente. Para validar:

```bash
# 1. Verificar logs ao iniciar o servidor
dotnet run --project src/HeimdallWeb.WebApi 2>&1 | grep "MonitoringWorker"
# Deve aparecer: "MonitoringWorker started."

# 2. Verificar que o worker processa alvos com NextCheck <= NOW
# (criar alvo e verificar no banco se LastCheck foi atualizado após 30min)
```

**No banco PostgreSQL:**
```sql
-- Verificar alvos monitorados
SELECT id, url, frequency, last_check, next_check, is_active FROM tb_monitored_target;

-- Verificar snapshots gerados pelo worker
SELECT * FROM tb_risk_snapshot ORDER BY created_at DESC LIMIT 10;

-- Verificar cache de scans
SELECT id, cache_key, expires_at, is_expired FROM tb_scan_cache;
```

---

## Verificação de Segurança

### IDOR Prevention
- [x] GET /api/v1/monitor → retorna apenas alvos do usuário autenticado
- [x] DELETE /api/v1/monitor/{id} → valida ownership antes de deletar
- [x] GET /api/v1/monitor/{id}/history → valida ownership antes de retornar

### Autenticação
- [x] Todos os endpoints requerem JWT válido
- [x] Token expirado retorna `401 Unauthorized`

---

## Entidades no Banco (PostgreSQL)

```sql
-- Verificar tabelas criadas pela migration Sprint4
\dt tb_monitored_target
\dt tb_risk_snapshot
\dt tb_scan_cache

-- Verificar índices GIN na tb_scan_cache
\di ix_tb_scan_cache*
-- Esperado: índice GIN em result_json

-- Verificar índices composite em monitored_target
\di ux_tb_monitored_target*
-- Esperado: índice único em (user_id, url)
```

---

## Checklist de Validação

- [ ] `POST /api/v1/monitor` — 201 com body correto
- [ ] `POST /api/v1/monitor` — 409 para URL duplicada
- [ ] `POST /api/v1/monitor` — 400 para frequência inválida
- [ ] `GET /api/v1/monitor` — 200 com lista do usuário
- [ ] `DELETE /api/v1/monitor/{id}` — 204 sem body
- [ ] `DELETE /api/v1/monitor/{id}` — 404 para IDOR
- [ ] `GET /api/v1/monitor/{id}/history` — 200 com lista de snapshots
- [ ] Cache: `isCached: true` no segundo scan para mesmo alvo
- [ ] Worker: logs "MonitoringWorker started." no startup
- [ ] Build: `dotnet build` → 0 erros, 0 warnings
