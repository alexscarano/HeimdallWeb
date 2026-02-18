# Plano de Evolução Técnica - HeimdallWeb

Este documento define o roteiro técnico detalhado para a evolução do HeimdallWeb, focado em tornar a plataforma robusta, escalável e amigável.

## 0. Considerações Prévias e Regras

-   **Arquitetura:** DDD Light + Minimal APIs (Vertical Slice onde possível).
-   **Banco de Dados:** PostgreSQL (Migração do MySQL mandatória conforme `plano_migracao.md`).
-   **Frontend:** Next.js 15 (App Router) + Tailwind + shadcn/ui.
-   **Verificação de Conflitos:** As novas tabelas propostas (`tb_risk_weights`, `tb_scan_profile`, `tb_monitored_target`, etc.) **NÃO** conflitam com as Views existentes em `src/HeimdallWeb.Infrastructure/Data/Views`, pois estas últimas são apenas para agregação de leitura (Dashboard Stats), enquanto as novas tabelas persistem estado e configuração.

---

## FASE 1: Core Engine Refactoring & Scoring (Sprint 1)

**Objetivo:** Modernizar o motor de scan para paralelismo real e implementar sistema de pontuação calibrável.

### 1.1. Domain Layer (Modelagem)

Criar entidade para pesos de risco dinâmicos.

-   **Entidade:** `RiskWeight` (`tb_risk_weights`)
    -   `Id` (int, PK)
    -   `Category` (string) - ex: "SSL", "Headers", "Port"
    -   `Weight` (decimal) - ex: 1.5, 0.8
    -   `IsActive` (bool)
    -   *Seed inicial no `DatabaseSeeder`.*

### 1.2. Application Layer (Engine)

Refatorar `ScanService` para usar paralelismo robusto.

-   **Interface:** Atualizar `IScanner` para suportar `CancellationToken`.
    ```csharp
    public interface IScanner {
        Task<ScanResult> ExecuteAsync(string target, ScanProfile profile, CancellationToken ct);
        ScannerMetadata Metadata { get; } // Capabilities, Timeout default
    }
    ```
-   **Pipeline Paralelo:**
    -   Substituir loops sequenciais por `Task.WhenAll`.
    -   Implementar `GenericRunner` que aceita lista de `IScanner`.
    -   **Timeout Global:** `CancellationTokenSource.CreateLinkedTokenSource` com timeout configurável (ex: 120s).
    -   **Timeout Individual:** Wrapper em cada execução de scanner para não derrubar o pipeline inteiro se um falhar/travar.

### 1.3. Infrastructure Layer (Scoring)

-   Implementar `ScoreCalculatorService`:
    -   Ler `tb_risk_weights` (com cache em memória de 10min).
    -   Calcular Score Base (0-100) baseado em vulnerabilidades encontradas vs pesos.
    -   Categorizar: A (90+), B (80-89), C (70-79), D (60-69), F (<60).

---

## FASE 2: Advanced Scan Mode & Profiles (Sprint 2)

**Objetivo:** Permitir scans personalizados e diferentes níveis de profundidade.

### 2.1. Domain Layer

-   **Entidade:** `ScanProfile` (`tb_scan_profile`)
    -   `Id` (int, PK)
    -   `Name` (string) - "Quick", "Standard", "Deep", "Custom"
    -   `Description` (string)
    -   `ConfigJson` (jsonb) - Configurações específicas (quais scanners ativos, timeouts, profundidade).
    -   `IsSystem` (bool) - Perfis padrão não podem ser deletados.

### 2.2. Application Layer

-   **DTOs:** `ScanRequestDto` agora aceita `ProfileId` ou lista manual de scanners.
-   **Validation:** Garantir que scanners solicitados existem.

### 2.3. WebApi

-   `GET /api/v1/profiles` - Listar perfis.
-   `GET /api/v1/profiles` - Listar perfis.
-   `POST /api/v1/scan` - Atualizado para receber configurações.

### 2.5. Custom Scan Profiles (Feature Adicional)

**Objetivo:** Permitir seleção granular de scanners via interface visual.

-   **Backend:**
    -   Atualizar `ExecuteScanRequest` e `ExecuteScanCommand` para aceitar `List<string> EnabledScanners`.
    -   Atualizar `ScannerManager` para filtrar execução baseado na lista, se fornecida.
-   **Frontend:**
    -   Criar `CustomScanModal` com checkboxes agrupados por categoria (Segurança, Infra, Performance, etc.).
    -   Atualizar `ScanForm` para abrir modal ao selecionar perfil "Custom".
    -   Passar lista de scanners selecionados no payload da API.

---

## FASE 3: Novos Scanners (Sprint 3)

**Objetivo:** Expandir a capacidade de detecção. Implementar cada um como uma implementação de `IScanner`.

1.  **TLS Capability Scanner 2.0:**
    -   Uso de `SslStream` com verificação de versões de protocolo (TLS 1.2, 1.3) e Cipher Suites fracos.
2.  **CSP Semantic Analyzer:**
    -   Parser de header `Content-Security-Policy`.
    -   Detectar `unsafe-inline`, `unsafe-eval`, wildcards `*`.
3.  **Domain Age Scanner:**
    -   WHOIS lookup (via biblioteca ou API externa se disponível) para determinar idade do domínio.
4.  **IP Change Detection:**
    -   Resolve DNS e compara com histórico (se houver).
5.  **Response Behavior:**
    -   Medir Time To First Byte (TTFB).
    -   Verificar consistência de respostas 404/500.
6.  **Subdomain Quick Discovery:**
    -   Verificar subdomínios comuns (`www`, `api`, `dev`, `staging`) via DNS resolution paralela.
7.  **Security.txt Scanner:**
    -   Verificar presença e validade de `/.well-known/security.txt` (RFC 9116).

### 3.1. Atualização do Prompt Gemini
-   **Local:** `HeimdallWeb.Infrastructure/External/GeminiService.cs` (Novo local após refatoração).
-   **Ação:** Atualizar o prompt hardcoded para:
    -   Reconhecer novas categorias: `CSP Analysis`, `Domain Reputation`, `Infra Change`, `Compliance (security.txt)`.
    -   Interpretar JSONs dos novos scanners (ex: output do `SslStream`, análise de CSP).
    -   Considerar o contexto de "Perfil de Scan" na análise (ex: scan rápido vs profundo).
    -   Manter formato de resposta JSON estrito para facilitar parsing.
    -   Incluir instruções sobre como tratar `security.txt` e mudanças de IP.

---

## FASE 4: Snapshot, Monitoramento & Cache (Sprint 4)

**Objetivo:** Persistência histórica e monitoramento contínuo.

### 4.1. Domain Layer & Database

-   **Entidade:** `MonitoredTarget` (`tb_monitored_target`)
    -   `Id`, `UserId`, `Url`, `Frequency` (Daily, Weekly), `LastCheck`, `NextCheck`, `IsActive`.
-   **Entidade:** `RiskSnapshot` (`tb_risk_snapshot`)
    -   Armazena o estado "resumido" de um alvo em um ponto do tempo para comparação rápida.
-   **Entidade:** `ScanCache` (`tb_scan_cache`)
    -   `Id`, `CacheKey` (Hash do Target + Profile), `ResultJson` (jsonb), `ExpiresAt`.
    -   **Estratégia:** Antes de iniciar scan, checar cache válido. Se hit, retornar imediatamente com flag `is_cached: true`.

### 4.2. Application Layer (Background Jobs)

-   **Worker Service:** `HeimdallWeb.Worker` (Novo projeto ou `IHostedService` no WebApi).
-   **Scheduler:** Usar `Quartz.NET` ou simples `PeriodicTimer` para verificar `tb_monitored_target`.
-   **Risk Delta:** Ao finalizar scan monitorado, comparar com `Snapshot` anterior. Se houver mudança crítica (ex: Score caiu 10 pontos), gerar `RiskEvent`.

### 4.3. Infra

-   Configurar **PostgreSQL JSONB** para `ScanCache` e `ResultJson` (Performance crítica). Criar índices GIN.

---

## FASE 5: Autenticação Google, Email & User Management (Sprint 5)

**Objetivo:** Login social, recuperação de conta e gestão de usuários.

### 5.1. Email Service
-   **Interface:** `IEmailService` (Domain).
-   **Implementação:** SMTP via `MailKit` (Recomendado).
    -   *Motivo:* Independência de fornecedor. Permite usar qualquer serviço transacional moderno (Resend, SendGrid, Amazon SES) através de seus endpoints SMTP padrão, ou servidores corporativos.
-   **Features:**
    -   **Recuperação de Senha:** Fluxo de "Esqueci minha senha" com token temporário.
    -   **Suporte:** Endpoint para envio de mensagens de contato (Fale Conosco).

### 5.2. Auth Strategy

-   **Package:** `Microsoft.AspNetCore.Authentication.Google`.
-   **Flow:**
    1.  Frontend chama `signIn('google')` (NextAuth.js ou similar).
    2.  Google retorna `id_token`.
    3.  Frontend envia `id_token` para Backend (`POST /api/v1/auth/google`).
    4.  Backend valida token via Google APIs.
    5.  Backend procura usuário em `tb_user` (email).
        -   Se não existe: Cria automaticamente.
        -   Se existe: Atualiza dados (imagem).
    6.  Backend emite JWT próprio do sistema (Session Token) em Cookie HttpOnly.

### 5.3. Database

-   Atualizar `tb_user`: Adicionar `AuthProvider` (Local/Google), `ExternalId`, `PasswordResetToken`, `PasswordResetExpires`.

---

## FASE 6: UX & Frontend (Sprint 6)

**Objetivo:** Interface moderna e responsiva integrada à API.

### 6.0. Agentes e Skills Mandatórios

-   **Agente:** `nexus-next-js` (Definido em `CLAUDE.md`).
    -   *Responsabilidade:* Implementar todas as páginas e componentes.
    -   *Instrução:* Usar `Task(subagent_type="nexus-next-js", prompt="Implementar página X...")`.
-   **Skill:** `frontend-design` (ou similar do `awesome-claude-skills`).
    -   *Responsabilidade:* Garantir qualidade visual e consistência com shadcn/ui.
-   **Skill:** `api-integration` (Geral).
    -   *Responsabilidade:* Criar camada de serviço no frontend (`services/api.ts`) para consumir o backend.
    -   *Padrão:* Usar `fetch` com tipagem forte (Zod + TypeScript) para validar respostas da API.

### 6.1. Next.js App Router Structure

-   `/app/(public)/page.tsx` - Landing Page (Hero, Features, Preview).
-   `/app/(app)/dashboard/page.tsx` - Visão geral.
-   `/app/(app)/scan/[id]/page.tsx` - Detalhes do Scan.
    -   **Components:**
        -   `ScoreGauge`: Círculo animado (SVG/Canvas).
        -   `RiskCard`: Cards coloridos por severidade.
        -   `Timeline`: Histórico visual de scans.
-   `/app/(app)/monitor/page.tsx` - Gestão de alvos monitorados.
-   `/app/auth/login/page.tsx` - Botão Google.
-   `/app/auth/forgot-password/page.tsx` - Recuperação de senha.
-   `/app/support/page.tsx` - Formulário de contato.

### 6.2. UX Improvements

-   **Feedback Visual:** Progress bar real durante o scan (via SSE ou Polling do status do job).
-   **Simples vs Avançado:** Toggle que esconde detalhes técnicos (Headers brutos, JSON) e mostra apenas cards e recomendações.

---

## 7. Riscos e Mitigação

1.  **Rate Limiting do Google Gemini:**
    -   *Mitigação:* Cache agressivo (`ScanCache`) e fila de processamento (Rate limit na saída para API).
2.  **Performance do Banco (JSONB):**
    -   *Mitigação:* Índices GIN em campos chave do JSONB. Arquivamento de scans muito antigos (Cold Storage) se tabela crescer demais.
3.  **Falsos Positivos:**
    -   *Mitigação:* Tabela `tb_risk_weights` permite zerar peso de scanners problemáticos sem redeploy.

## 8. Ordem de Execução Recomendada

1.  **Database Migration (PostgreSQL)** - Pré-requisito absoluto.
2.  **Fase 1 (Engine)** - Fundamento para todo o resto.
3.  **Fase 5 (Auth)** - Necessário para criar sistema de perfis e monitoramento por usuário.
4.  **Fase 2 & 3 (Scanners & Profiles)** - Core Value.
5.  **Fase 6 (Frontend)** - Interface para consumir o novo Core.
6.  **Fase 4 (Monitoramento)** - Feature "Stickiness" (retenção).

## Tecnologias e Dependências

-   **Backend:** .NET 8/9, EF Core 9, Npgsql.
-   **Frontend:** Next.js 15, React 19, Tailwind, shadcn/ui (Components, Charts & Icons).
-   **Infra:** PostgreSQL 16+, Docker (para db).
