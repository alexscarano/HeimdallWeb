# Sprint 5 — Google Auth, Email & User Management Testing Guide

**Data:** 2026-02-19
**Sprint:** Implementation Sprint 5 — Auth Google, Email & User Management
**Build:** 0 erros, 0 warnings

---

## Endpoints Implementados

| Method | URL | Auth | Descrição |
|--------|-----|------|-----------|
| `POST` | `/api/v1/auth/forgot-password` | Anônimo | Solicitar reset de senha |
| `POST` | `/api/v1/auth/reset-password` | Anônimo | Confirmar reset com token |
| `POST` | `/api/v1/auth/google` | Anônimo | Login/Register via Google OAuth |
| `POST` | `/api/v1/support/contact` | Anônimo | Formulário de contato |

---

## Pré-requisitos

```bash
# Backend rodando
dotnet run --project src/HeimdallWeb.WebApi

# Token JWT para testes
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrLogin":"alexandrescarano@gmail.com","password":"Admin@123"}' | jq -r '.token')
```

---

## Test Cases

### 1. POST /api/v1/auth/forgot-password — Solicitar Reset de Senha

**Caso 1.1 — Sucesso: email válido cadastrado**
```bash
curl -X POST http://localhost:5000/api/v1/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"alexandrescarano@gmail.com"}'
```
**Resposta esperada:** `200 OK`
```json
{
  "message": "If this email is registered, a reset link was sent."
}
```
**Verificar:** Email enviado (se SMTP configurado) ou log "SMTP is not configured" (sem token no log).

**Caso 1.2 — Sucesso: email NÃO cadastrado (segurança — não revelar)**
```bash
curl -X POST http://localhost:5000/api/v1/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"naocadastrado@example.com"}'
```
**Resposta esperada:** `200 OK` — **mesma mensagem** do caso 1.1 (email enumeration prevention)

**Caso 1.3 — Erro: email inválido**
```bash
curl -X POST http://localhost:5000/api/v1/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"nao-e-email"}'
```
**Resposta esperada:** `400 Bad Request`

**Caso 1.4 — Rate limit**
```bash
# Após 10 requests em 1 minuto
```
**Resposta esperada:** `429 Too Many Requests`

---

### 2. POST /api/v1/auth/reset-password — Confirmar Reset

> **Fluxo completo:**
> 1. Chamar forgot-password → sistema gera token
> 2. Token chega por email (ou ler do banco via SQL abaixo)
> 3. Chamar reset-password com o token raw

**Obter token do banco (para testes sem SMTP):**
```sql
-- Atenção: password_reset_token armazena o HASH SHA-256, não o token raw
-- Em dev, quando SMTP não configurado, o link é logado no console (sem token após fix de segurança)
-- Para testar: configure SMTP temporariamente ou intercepte em ForgotPasswordCommandHandler
SELECT username, password_reset_token, password_reset_expires FROM tb_user WHERE username = 'alexandrescarano';
```

**Caso 2.1 — Sucesso**
```bash
curl -X POST http://localhost:5000/api/v1/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"token":"<token-raw-do-email>","newPassword":"NovaSenh@123"}'
```
**Resposta esperada:** `200 OK`
```json
{
  "message": "Password updated successfully."
}
```

**Caso 2.2 — Erro: token inválido**
```bash
curl -X POST http://localhost:5000/api/v1/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"token":"token-invalido-qualquer","newPassword":"NovaSenh@123"}'
```
**Resposta esperada:** `400 Bad Request`
```json
{
  "detail": "Invalid or expired token"
}
```

**Caso 2.3 — Erro: token expirado (após 1 hora)**
- **Resposta esperada:** `400 Bad Request` com `"Token has expired"`

**Caso 2.4 — Erro: senha fraca**
```bash
curl -X POST http://localhost:5000/api/v1/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"token":"<token>","newPassword":"senha123"}'
```
**Resposta esperada:** `400 Bad Request` (sem maiúscula)

---

### 3. POST /api/v1/auth/google — Google OAuth

> **Nota:** Requer `Google:ClientId` configurado em `appsettings.json`.
> Se não configurado, todos os requests retornam `401` (por segurança).

**Pré-requisito:** Configurar Google OAuth Client ID no projeto.

**Caso 3.1 — Erro: Google:ClientId não configurado (ambiente dev padrão)**
```bash
curl -X POST http://localhost:5000/api/v1/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken":"qualquer-token"}'
```
**Resposta esperada:** `401 Unauthorized`
```json
{
  "detail": "Google authentication is not available at this time."
}
```

**Caso 3.2 — Erro: id_token inválido (com ClientId configurado)**
```bash
curl -X POST http://localhost:5000/api/v1/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken":"token-invalido"}'
```
**Resposta esperada:** `401 Unauthorized`
```json
{
  "detail": "Invalid Google token"
}
```

**Caso 3.3 — Sucesso: novo usuário via Google (com ClientId configurado e token válido)**
- **Resposta esperada:** `200 OK` com `LoginResponse` (mesma estrutura do login normal)
- Cookie `authHeimdallCookie` setado

**Caso 3.4 — Erro: email já existe como conta local (Account Takeover Prevention)**
- Tentar autenticar Google com email que já existe localmente
- **Resposta esperada:** `401 Unauthorized`
```json
{
  "detail": "An account with this email already exists. Please log in with your password and link Google from your profile settings."
}
```

---

### 4. POST /api/v1/support/contact — Formulário de Contato

**Caso 4.1 — Sucesso**
```bash
curl -X POST http://localhost:5000/api/v1/support/contact \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@example.com",
    "subject": "Dúvida sobre o sistema",
    "message": "Gostaria de saber mais sobre os planos disponíveis na plataforma."
  }'
```
**Resposta esperada:** `200 OK`
```json
{
  "message": "Your message was sent successfully."
}
```

**Caso 4.2 — Erro: campos obrigatórios faltando**
```bash
curl -X POST http://localhost:5000/api/v1/support/contact \
  -H "Content-Type: application/json" \
  -d '{"name":"J","email":"invalido","subject":"ok","message":"ok"}'
```
**Resposta esperada:** `400 Bad Request` (Name mínimo 2 chars, email inválido)

**Caso 4.3 — Erro: mensagem muito curta**
```bash
curl -X POST http://localhost:5000/api/v1/support/contact \
  -H "Content-Type: application/json" \
  -d '{"name":"João","email":"joao@example.com","subject":"Assunto","message":"ok"}'
```
**Resposta esperada:** `400 Bad Request` (message mínimo 10 chars)

**Caso 4.4 — Sem autenticação (deve aceitar — AllowAnonymous)**
```bash
# Request sem header Authorization — deve funcionar
curl -X POST http://localhost:5000/api/v1/support/contact \
  -H "Content-Type: application/json" \
  -d '{"name":"João Silva","email":"joao@example.com","subject":"Dúvida sobre o sistema","message":"Mensagem de contato de teste aqui."}'
```
**Resposta esperada:** `200 OK`

---

## Verificação do Banco de Dados

```sql
-- Verificar novos campos em tb_user
\d tb_user
-- Esperado: auth_provider, external_id, password_reset_token, password_reset_expires

-- Verificar usuário após forgot-password
SELECT username, auth_provider, external_id,
       password_reset_token IS NOT NULL AS has_reset_token,
       password_reset_expires
FROM tb_user WHERE username = 'alexandrescarano';

-- Verificar que token foi limpo após reset-password (campo deve ser NULL)
SELECT password_reset_token, password_reset_expires FROM tb_user WHERE username = 'alexandrescarano';

-- Verificar usuário Google criado
SELECT username, auth_provider, external_id, password = '' AS no_password
FROM tb_user WHERE auth_provider = 'Google';
```

---

## Verificação de Segurança

### Email Enumeration Prevention
- [x] `POST /forgot-password` com email não cadastrado → `200 OK` (mesma resposta)
- [x] `POST /forgot-password` com email cadastrado → `200 OK` (mesma resposta)
- [x] Logs NÃO contêm o reset token raw

### Account Takeover Prevention
- [x] Google OAuth com email de conta local existente → `401 Unauthorized`
- [x] `email_verified` do Google deve ser `true`

### Token Security
- [x] Token armazenado como SHA-256 hash (não raw)
- [x] Token expira em 1 hora
- [x] Token invalidado após uso (`ClearPasswordResetToken()`)

### Transport Security
- [x] SMTP usa `StartTls` (não `StartTlsWhenAvailable`) — rejeita downgrade

### Input Sanitization
- [x] HTML encoding em templates de email (previne HTML injection)

---

## Configuração de SMTP (para testar com email real)

Adicionar em `appsettings.json` (não commitar credenciais reais):
```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "seu-email@gmail.com",
  "Password": "sua-app-password",
  "FromAddress": "noreply@heimdall.app",
  "FromName": "HeimdallWeb",
  "ContactEmail": "suporte@heimdall.app"
}
```

Para Gmail: use [App Password](https://support.google.com/accounts/answer/185833), não a senha da conta.

---

## Configuração do Google OAuth (para testar login social)

1. Criar projeto no [Google Cloud Console](https://console.cloud.google.com/)
2. Habilitar "Google Identity" / OAuth 2.0
3. Criar Client ID (Web application)
4. Adicionar em `appsettings.json`:
```json
"Google": {
  "ClientId": "seu-client-id.apps.googleusercontent.com"
}
```

---

## Checklist de Validação

- [ ] `POST /api/v1/auth/forgot-password` — 200 para email cadastrado
- [ ] `POST /api/v1/auth/forgot-password` — 200 para email NÃO cadastrado (enum prevention)
- [ ] `POST /api/v1/auth/forgot-password` — 400 para email inválido
- [ ] `POST /api/v1/auth/reset-password` — 200 com token válido
- [ ] `POST /api/v1/auth/reset-password` — 400 com token inválido
- [ ] `POST /api/v1/auth/reset-password` — 400 com senha fraca
- [ ] `POST /api/v1/auth/google` — 401 sem Google:ClientId configurado
- [ ] `POST /api/v1/auth/google` — 401 com token inválido
- [ ] `POST /api/v1/support/contact` — 200 com dados válidos
- [ ] `POST /api/v1/support/contact` — 400 com dados inválidos
- [ ] Banco: colunas `auth_provider`, `external_id`, `password_reset_token`, `password_reset_expires` em `tb_user`
- [ ] Build: `dotnet build` → 0 erros, 0 warnings
