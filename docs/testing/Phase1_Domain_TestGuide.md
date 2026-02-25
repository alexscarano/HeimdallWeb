# Fase 1: Domain Layer - Guia de Testes Manuais

**Data de criação:** 2026-02-04
**Fase:** 1 - Domain Layer
**Projeto:** `src/HeimdallWeb.Domain/`

---

## 📋 Visão Geral

Este guia explica como testar manualmente todos os componentes da Domain Layer implementados na Fase 1. Como a Domain Layer não possui dependências externas (apenas .NET 10 BCL), todos os testes podem ser executados criando um projeto de console ou usando o projeto de testes xUnit.

---

## ✅ Pré-requisitos

```bash
# Verificar que o projeto compila
dotnet build src/HeimdallWeb.Domain/

# Criar projeto de console para testes manuais (opcional)
dotnet new console -n HeimdallWeb.Domain.ManualTests -f net10.0
cd HeimdallWeb.Domain.ManualTests
dotnet add reference ../src/HeimdallWeb.Domain/HeimdallWeb.Domain.csproj
```

---

## 1️⃣ Testando Value Objects

### 1.1 EmailAddress

**Cenários a testar:**

#### ✅ Teste 1: Email válido
```csharp
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Domain.Exceptions;

// TESTE: Email válido deve ser aceito
var email = EmailAddress.Create("user@example.com");
Console.WriteLine($"✅ Email criado: {email.Value}");
Console.WriteLine($"✅ Email normalizado: {email.Value == "user@example.com"}");

// TESTE: Email é normalizado para lowercase
var emailUpper = EmailAddress.Create("USER@EXAMPLE.COM");
Console.WriteLine($"✅ Uppercase normalizado: {emailUpper.Value == "user@example.com"}");
```

**Resultado esperado:**
```
✅ Email criado: user@example.com
✅ Email normalizado: True
✅ Uppercase normalizado: True
```

#### ❌ Teste 2: Email inválido deve lançar exceção
```csharp
// TESTE: Email sem @ deve falhar
try
{
    var invalidEmail = EmailAddress.Create("invalid-email");
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}

// TESTE: Email vazio deve falhar
try
{
    var emptyEmail = EmailAddress.Create("");
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}

// TESTE: Email null deve falhar
try
{
    var nullEmail = EmailAddress.Create(null);
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}
```

**Resultado esperado:**
```
✅ ValidationException corretamente lançada: Email address cannot be empty.
✅ ValidationException corretamente lançada: Email address cannot be empty.
✅ ValidationException corretamente lançada: Email address cannot be empty.
```

#### ✅ Teste 3: Conversão implícita
```csharp
// TESTE: Conversão implícita para string
EmailAddress email = EmailAddress.Create("test@example.com");
string emailString = email; // Conversão implícita
Console.WriteLine($"✅ Conversão para string: {emailString == "test@example.com"}");

// TESTE: Igualdade de value objects
var email1 = EmailAddress.Create("user@example.com");
var email2 = EmailAddress.Create("user@example.com");
var email3 = EmailAddress.Create("other@example.com");

Console.WriteLine($"✅ Igualdade (mesmo email): {email1.Equals(email2)}");
Console.WriteLine($"✅ Desigualdade (email diferente): {!email1.Equals(email3)}");
```

**Resultado esperado:**
```
✅ Conversão para string: True
✅ Igualdade (mesmo email): True
✅ Desigualdade (email diferente): True
```

---

### 1.2 ScanTarget

**Cenários a testar:**

#### ✅ Teste 1: Normalização de domínio/URL
```csharp
using HeimdallWeb.Domain.ValueObjects;

// TESTE: Remover protocolo
var target1 = ScanTarget.Create("https://example.com");
Console.WriteLine($"✅ Protocolo removido: {target1.Value == "example.com"}");

// TESTE: Remover www
var target2 = ScanTarget.Create("www.example.com");
Console.WriteLine($"✅ WWW removido: {target2.Value == "example.com"}");

// TESTE: Remover trailing slash
var target3 = ScanTarget.Create("example.com/");
Console.WriteLine($"✅ Trailing slash removido: {target3.Value == "example.com"}");

// TESTE: Normalização completa
var target4 = ScanTarget.Create("https://www.example.com:443/");
Console.WriteLine($"✅ Normalização completa: {target4.Value == "example.com"}");

// TESTE: Preservar subdomínio
var target5 = ScanTarget.Create("sub.example.com");
Console.WriteLine($"✅ Subdomínio preservado: {target5.Value == "sub.example.com"}");

// TESTE: Lowercase
var target6 = ScanTarget.Create("EXAMPLE.COM");
Console.WriteLine($"✅ Lowercase aplicado: {target6.Value == "example.com"}");
```

**Resultado esperado:**
```
✅ Protocolo removido: True
✅ WWW removido: True
✅ Trailing slash removido: True
✅ Normalização completa: True
✅ Subdomínio preservado: True
✅ Lowercase aplicado: True
```

#### ❌ Teste 2: Domínio inválido deve falhar
```csharp
// TESTE: Domínio vazio
try
{
    var empty = ScanTarget.Create("");
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}

// TESTE: Domínio com espaços
try
{
    var spaces = ScanTarget.Create("example .com");
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}

// TESTE: Domínio muito curto
try
{
    var short = ScanTarget.Create("a");
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}
```

**Resultado esperado:**
```
✅ ValidationException corretamente lançada: Scan target cannot be empty.
✅ ValidationException corretamente lançada: Scan target 'example .com' is not a valid domain or URL.
✅ ValidationException corretamente lançada: Scan target 'a' is not a valid domain or URL.
```

---

### 1.3 ScanDuration

**Cenários a testar:**

#### ✅ Teste 1: Duração válida
```csharp
using HeimdallWeb.Domain.ValueObjects;

// TESTE: Criar duração válida
var duration1 = ScanDuration.Create(TimeSpan.FromSeconds(30));
Console.WriteLine($"✅ Duração criada: {duration1.Value.TotalSeconds} segundos");

// TESTE: Conversão implícita para TimeSpan
TimeSpan ts = duration1;
Console.WriteLine($"✅ Conversão para TimeSpan: {ts.TotalSeconds == 30}");

// TESTE: Conversão implícita de TimeSpan
ScanDuration duration2 = TimeSpan.FromMinutes(2);
Console.WriteLine($"✅ Conversão de TimeSpan: {duration2.Value.TotalMinutes == 2}");
```

**Resultado esperado:**
```
✅ Duração criada: 30 segundos
✅ Conversão para TimeSpan: True
✅ Conversão de TimeSpan: True
```

#### ❌ Teste 2: Duração negativa ou zero deve falhar
```csharp
// TESTE: Duração negativa
try
{
    var negative = ScanDuration.Create(TimeSpan.FromSeconds(-10));
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}

// TESTE: Duração zero
try
{
    var zero = ScanDuration.Create(TimeSpan.Zero);
    Console.WriteLine("❌ FALHOU: Deveria ter lançado ValidationException");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException corretamente lançada: {ex.Message}");
}
```

**Resultado esperado:**
```
✅ ValidationException corretamente lançada: Scan duration must be positive.
✅ ValidationException corretamente lançada: Scan duration must be positive.
```

---

## 2️⃣ Testando Entidades

### 2.1 User

**Cenários a testar:**

#### ✅ Teste 1: Criar usuário válido
```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Domain.Enums;

// TESTE: Criar usuário completo
var email = EmailAddress.Create("john.doe@example.com");
var user = new User
{
    UserId = 1,
    Username = "johndoe",
    Email = email,
    PasswordHash = "hashed_password_here",
    UserType = UserType.Default,
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    ProfileImage = null
};

Console.WriteLine($"✅ Usuário criado: {user.Username}");
Console.WriteLine($"✅ Email: {user.Email.Value}");
Console.WriteLine($"✅ Tipo: {user.UserType}");
Console.WriteLine($"✅ Ativo: {user.IsActive}");
```

**Resultado esperado:**
```
✅ Usuário criado: johndoe
✅ Email: john.doe@example.com
✅ Tipo: Default
✅ Ativo: True
```

#### ✅ Teste 2: Métodos de domínio
```csharp
// TESTE: Desativar usuário
user.Deactivate();
Console.WriteLine($"✅ Usuário desativado: {!user.IsActive}");

// TESTE: Ativar usuário
user.Activate();
Console.WriteLine($"✅ Usuário ativado: {user.IsActive}");

// TESTE: Atualizar senha
var oldPassword = user.PasswordHash;
user.UpdatePassword("new_hashed_password");
Console.WriteLine($"✅ Senha atualizada: {user.PasswordHash != oldPassword}");
Console.WriteLine($"✅ UpdatedAt definido: {user.UpdatedAt.HasValue}");
```

**Resultado esperado:**
```
✅ Usuário desativado: True
✅ Usuário ativado: True
✅ Senha atualizada: True
✅ UpdatedAt definido: True
```

#### ✅ Teste 3: Coleções de navegação (read-only)
```csharp
// TESTE: Coleções inicializadas
Console.WriteLine($"✅ ScanHistories inicializada: {user.ScanHistories != null}");
Console.WriteLine($"✅ UserUsages inicializada: {user.UserUsages != null}");
Console.WriteLine($"✅ AuditLogs inicializada: {user.AuditLogs != null}");

// TESTE: Coleções são read-only (não pode reassign)
// user.ScanHistories = new List<ScanHistory>(); // ❌ Compilador não permite
Console.WriteLine($"✅ Coleções são read-only (verificar no código)");
```

**Resultado esperado:**
```
✅ ScanHistories inicializada: True
✅ UserUsages inicializada: True
✅ AuditLogs inicializada: True
✅ Coleções são read-only (verificar no código)
```

---

### 2.2 ScanHistory

**Cenários a testar:**

#### ✅ Teste 1: Criar histórico de scan
```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;

// TESTE: Criar scan history
var target = ScanTarget.Create("example.com");
var scanHistory = new ScanHistory
{
    HistoryId = 1,
    Target = target,
    RawJsonResult = "{}",
    Summary = "Scan in progress...",
    HasCompleted = false,
    Duration = null, // Ainda não completado
    CreatedDate = DateTime.UtcNow,
    UserId = 1
};

Console.WriteLine($"✅ Scan criado: {scanHistory.Target.Value}");
Console.WriteLine($"✅ Completado: {scanHistory.HasCompleted}");
Console.WriteLine($"✅ Duração: {(scanHistory.Duration == null ? "null" : scanHistory.Duration.Value.ToString())}");
```

**Resultado esperado:**
```
✅ Scan criado: example.com
✅ Completado: False
✅ Duração: null
```

#### ✅ Teste 2: Completar scan
```csharp
// TESTE: Completar scan com duração
var duration = TimeSpan.FromSeconds(45);
scanHistory.CompleteScan(duration);

Console.WriteLine($"✅ Scan completado: {scanHistory.HasCompleted}");
Console.WriteLine($"✅ Duração definida: {scanHistory.Duration != null}");
Console.WriteLine($"✅ Duração correta: {scanHistory.Duration?.Value.TotalSeconds == 45}");
```

**Resultado esperado:**
```
✅ Scan completado: True
✅ Duração definida: True
✅ Duração correta: True
```

#### ✅ Teste 3: Marcar como incompleto
```csharp
// TESTE: Marcar como incompleto (ex: timeout, erro)
scanHistory.MarkAsIncomplete();

Console.WriteLine($"✅ Scan marcado como incompleto: {!scanHistory.HasCompleted}");
Console.WriteLine($"✅ Duração mantida: {scanHistory.Duration != null}");
```

**Resultado esperado:**
```
✅ Scan marcado como incompleto: True
✅ Duração mantida: True
```

---

### 2.3 Finding

**Cenários a testar:**

#### ✅ Teste 1: Criar finding
```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;

// TESTE: Criar vulnerability finding
var finding = new Finding
{
    FindingId = 1,
    Type = "SSL Certificate Expired",
    Description = "The SSL certificate has expired on 2025-01-01",
    Severity = SeverityLevel.Critical,
    Evidence = "Certificate validity: Not after 2025-01-01T00:00:00Z",
    Recommendation = "Renew SSL certificate immediately",
    CreatedAt = DateTime.UtcNow,
    HistoryId = 1
};

Console.WriteLine($"✅ Finding criado: {finding.Type}");
Console.WriteLine($"✅ Severidade: {finding.Severity}");
Console.WriteLine($"✅ Descrição: {finding.Description.Length > 0}");
```

**Resultado esperado:**
```
✅ Finding criado: SSL Certificate Expired
✅ Severidade: Critical
✅ Descrição: True
```

#### ✅ Teste 2: Atualizar severidade
```csharp
// TESTE: Atualizar severidade (ex: após análise)
var oldSeverity = finding.Severity;
finding.UpdateSeverity(SeverityLevel.High);

Console.WriteLine($"✅ Severidade atualizada: {finding.Severity != oldSeverity}");
Console.WriteLine($"✅ Nova severidade: {finding.Severity == SeverityLevel.High}");
```

**Resultado esperado:**
```
✅ Severidade atualizada: True
✅ Nova severidade: True
```

---

### 2.4 Technology

**Cenários a testar:**

#### ✅ Teste 1: Criar technology
```csharp
using HeimdallWeb.Domain.Entities;

// TESTE: Criar technology detectada
var tech = new Technology
{
    TechnologyId = 1,
    Name = "Nginx",
    Version = "1.21.6",
    Category = "Web Server",
    Description = "High-performance web server",
    CreatedAt = DateTime.UtcNow,
    HistoryId = 1
};

Console.WriteLine($"✅ Technology criada: {tech.Name}");
Console.WriteLine($"✅ Versão: {tech.Version}");
Console.WriteLine($"✅ Categoria: {tech.Category}");
```

**Resultado esperado:**
```
✅ Technology criada: Nginx
✅ Versão: 1.21.6
✅ Categoria: Web Server
```

#### ✅ Teste 2: Technology sem versão
```csharp
// TESTE: Technology sem versão (comum para detecção incompleta)
var tech2 = new Technology
{
    TechnologyId = 2,
    Name = "PHP",
    Version = null, // Versão não detectada
    Category = "Backend",
    Description = "Server-side scripting language",
    CreatedAt = DateTime.UtcNow,
    HistoryId = 1
};

Console.WriteLine($"✅ Technology sem versão: {tech2.Version == null}");
Console.WriteLine($"✅ Nome: {tech2.Name}");
```

**Resultado esperado:**
```
✅ Technology sem versão: True
✅ Nome: PHP
```

---

### 2.5 IASummary

**Cenários a testar:**

#### ✅ Teste 1: Criar IA summary
```csharp
using HeimdallWeb.Domain.Entities;

// TESTE: Criar análise da IA
var iaSummary = new IASummary
{
    IASummaryId = 1,
    SummaryText = "Target has critical security issues requiring immediate attention",
    MainCategory = "SSL",
    OverallRisk = "Critical",
    TotalFindings = 15,
    FindingsCritical = 3,
    FindingsHigh = 5,
    FindingsMedium = 4,
    FindingsLow = 3,
    IANotes = "SSL certificate expired, weak ciphers detected",
    CreatedDate = DateTime.UtcNow,
    HistoryId = 1
};

Console.WriteLine($"✅ IA Summary criado");
Console.WriteLine($"✅ Risco geral: {iaSummary.OverallRisk}");
Console.WriteLine($"✅ Total findings: {iaSummary.TotalFindings}");
Console.WriteLine($"✅ Breakdown: C={iaSummary.FindingsCritical}, H={iaSummary.FindingsHigh}, M={iaSummary.FindingsMedium}, L={iaSummary.FindingsLow}");
```

**Resultado esperado:**
```
✅ IA Summary criado
✅ Risco geral: Critical
✅ Total findings: 15
✅ Breakdown: C=3, H=5, M=4, L=3
```

---

### 2.6 AuditLog

**Cenários a testar:**

#### ✅ Teste 1: Criar audit log
```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;

// TESTE: Criar log de evento
var log = new AuditLog
{
    LogId = 1,
    Timestamp = DateTime.UtcNow,
    Code = LogEventCode.INIT_SCAN,
    Level = "Info",
    Source = "ScanService",
    Message = "Scan process initiated",
    Details = "Target: example.com",
    UserId = 1,
    HistoryId = 1,
    RemoteIp = "192.168.1.100"
};

Console.WriteLine($"✅ Log criado: {log.Code}");
Console.WriteLine($"✅ Nível: {log.Level}");
Console.WriteLine($"✅ Mensagem: {log.Message}");
```

**Resultado esperado:**
```
✅ Log criado: INIT_SCAN
✅ Nível: Info
✅ Mensagem: Scan process initiated
```

#### ✅ Teste 2: Log de erro
```csharp
// TESTE: Log de erro com detalhes
var errorLog = new AuditLog
{
    LogId = 2,
    Timestamp = DateTime.UtcNow,
    Code = LogEventCode.SCAN_ERROR,
    Level = "Error",
    Source = "SslScanner",
    Message = "Failed to connect to target",
    Details = "Exception: Timeout after 8 seconds\nStackTrace: ...",
    UserId = 1,
    HistoryId = 1,
    RemoteIp = "192.168.1.100"
};

Console.WriteLine($"✅ Log de erro criado: {errorLog.Code}");
Console.WriteLine($"✅ Nível: {errorLog.Level}");
Console.WriteLine($"✅ Detalhes presente: {errorLog.Details != null}");
```

**Resultado esperado:**
```
✅ Log de erro criado: SCAN_ERROR
✅ Nível: Error
✅ Detalhes presente: True
```

---

### 2.7 UserUsage

**Cenários a testar:**

#### ✅ Teste 1: Criar user usage
```csharp
using HeimdallWeb.Domain.Entities;

// TESTE: Criar registro de uso
var usage = new UserUsage
{
    UserUsageId = 1,
    Date = DateTime.UtcNow.Date,
    RequestCounts = 0,
    UserId = 1
};

Console.WriteLine($"✅ Usage criado: Data={usage.Date:yyyy-MM-dd}");
Console.WriteLine($"✅ Requests iniciais: {usage.RequestCounts}");
```

**Resultado esperado:**
```
✅ Usage criado: Data=2026-02-04
✅ Requests iniciais: 0
```

#### ✅ Teste 2: Incrementar requests
```csharp
// TESTE: Incrementar contador (usado em rate limiting)
usage.IncrementRequests();
Console.WriteLine($"✅ Após 1º incremento: {usage.RequestCounts == 1}");

usage.IncrementRequests();
usage.IncrementRequests();
Console.WriteLine($"✅ Após 3 incrementos: {usage.RequestCounts == 3}");

// TESTE: Verificar limite
var maxRequests = 5;
var canMakeRequest = usage.RequestCounts < maxRequests;
Console.WriteLine($"✅ Pode fazer request (3 < 5): {canMakeRequest}");

// Simular atingir limite
usage.IncrementRequests();
usage.IncrementRequests();
var limitReached = usage.RequestCounts >= maxRequests;
Console.WriteLine($"✅ Limite atingido (5 >= 5): {limitReached}");
```

**Resultado esperado:**
```
✅ Após 1º incremento: True
✅ Após 3 incrementos: True
✅ Pode fazer request (3 < 5): True
✅ Limite atingido (5 >= 5): True
```

---

## 3️⃣ Testando Exceções de Domínio

### 3.1 ValidationException

**Cenários a testar:**

```csharp
using HeimdallWeb.Domain.Exceptions;

// TESTE: Lançar e capturar ValidationException
try
{
    throw new ValidationException("Invalid email format");
}
catch (ValidationException ex)
{
    Console.WriteLine($"✅ ValidationException capturada: {ex.Message}");
    Console.WriteLine($"✅ É DomainException: {ex is DomainException}");
}
```

**Resultado esperado:**
```
✅ ValidationException capturada: Invalid email format
✅ É DomainException: True
```

---

### 3.2 EntityNotFoundException

**Cenários a testar:**

```csharp
using HeimdallWeb.Domain.Exceptions;

// TESTE: Lançar EntityNotFoundException
try
{
    throw new EntityNotFoundException("User", 999);
}
catch (EntityNotFoundException ex)
{
    Console.WriteLine($"✅ EntityNotFoundException capturada: {ex.Message}");
    Console.WriteLine($"✅ Mensagem formatada corretamente: {ex.Message.Contains("User with key 999 was not found")}");
    Console.WriteLine($"✅ É DomainException: {ex is DomainException}");
}
```

**Resultado esperado:**
```
✅ EntityNotFoundException capturada: User with key 999 was not found.
✅ Mensagem formatada corretamente: True
✅ É DomainException: True
```

---

## 4️⃣ Testando Enums

### 4.1 UserType

```csharp
using HeimdallWeb.Domain.Enums;

// TESTE: Valores corretos
Console.WriteLine($"✅ Default = 1: {(int)UserType.Default == 1}");
Console.WriteLine($"✅ Admin = 2: {(int)UserType.Admin == 2}");

// TESTE: Parsing
var userTypeFromDb = 2;
var userType = (UserType)userTypeFromDb;
Console.WriteLine($"✅ Parse de int: {userType == UserType.Admin}");
```

**Resultado esperado:**
```
✅ Default = 1: True
✅ Admin = 2: True
✅ Parse de int: True
```

---

### 4.2 SeverityLevel

```csharp
using HeimdallWeb.Domain.Enums;

// TESTE: Valores ordenados por gravidade
Console.WriteLine($"✅ Informational = 0: {(int)SeverityLevel.Informational == 0}");
Console.WriteLine($"✅ Low = 1: {(int)SeverityLevel.Low == 1}");
Console.WriteLine($"✅ Medium = 2: {(int)SeverityLevel.Medium == 2}");
Console.WriteLine($"✅ High = 3: {(int)SeverityLevel.High == 3}");
Console.WriteLine($"✅ Critical = 4: {(int)SeverityLevel.Critical == 4}");

// TESTE: Comparação
var severity1 = SeverityLevel.Critical;
var severity2 = SeverityLevel.Medium;
Console.WriteLine($"✅ Critical > Medium: {severity1 > severity2}");
```

**Resultado esperado:**
```
✅ Informational = 0: True
✅ Low = 1: True
✅ Medium = 2: True
✅ High = 3: True
✅ Critical = 4: True
✅ Critical > Medium: True
```

---

### 4.3 LogEventCode

```csharp
using HeimdallWeb.Domain.Enums;

// TESTE: Valores principais
var codes = new[]
{
    LogEventCode.INIT_SCAN,
    LogEventCode.SCAN_COMPLETED,
    LogEventCode.SCAN_ERROR,
    LogEventCode.AI_REQUEST,
    LogEventCode.USER_LOGIN
};

Console.WriteLine($"✅ Códigos definidos: {codes.Length} testados");

// TESTE: Conversão para string
var codeString = LogEventCode.INIT_SCAN.ToString();
Console.WriteLine($"✅ ToString(): {codeString == "INIT_SCAN"}");

// TESTE: Parse de string
var parsedCode = Enum.Parse<LogEventCode>("SCAN_COMPLETED");
Console.WriteLine($"✅ Parse: {parsedCode == LogEventCode.SCAN_COMPLETED}");
```

**Resultado esperado:**
```
✅ Códigos definidos: 5 testados
✅ ToString(): True
✅ Parse: True
```

---

## 5️⃣ Verificações de Build e Dependências

### 5.1 Compilação

```bash
# TESTE: Projeto compila sem erros/warnings
dotnet build src/HeimdallWeb.Domain/ --configuration Debug

# Resultado esperado:
# Build succeeded.
#     0 Warning(s)
#     0 Error(s)
```

### 5.2 Dependências

```bash
# TESTE: Zero dependências externas (apenas .NET 10 BCL)
dotnet list src/HeimdallWeb.Domain/ package

# Resultado esperado:
# Project 'HeimdallWeb.Domain' has no NuGet package references.
```

### 5.3 Nullable Reference Types

```bash
# TESTE: Verificar que nullable está habilitado
grep -r "nullable" src/HeimdallWeb.Domain/HeimdallWeb.Domain.csproj

# Resultado esperado:
# <Nullable>enable</Nullable>
```

---

## 6️⃣ Checklist Final de Validação

Execute todos os testes acima e marque:

### Value Objects
- [ ] EmailAddress: Validação de formato funciona
- [ ] EmailAddress: Normalização para lowercase funciona
- [ ] EmailAddress: Conversão implícita funciona
- [ ] ScanTarget: Normalização de URL funciona
- [ ] ScanTarget: Validação de domínio funciona
- [ ] ScanTarget: Conversão implícita funciona
- [ ] ScanDuration: Validação de duração positiva funciona
- [ ] ScanDuration: Conversão implícita funciona

### Entidades
- [ ] User: Criação funciona
- [ ] User: Activate/Deactivate funcionam
- [ ] User: UpdatePassword funciona e atualiza UpdatedAt
- [ ] ScanHistory: CompleteScan define duração e status
- [ ] ScanHistory: MarkAsIncomplete mantém duração
- [ ] Finding: UpdateSeverity funciona
- [ ] Technology: Criação com/sem versão funciona
- [ ] IASummary: Contadores de findings funcionam
- [ ] AuditLog: Criação de logs funciona
- [ ] UserUsage: IncrementRequests funciona

### Exceções
- [ ] ValidationException é lançada corretamente
- [ ] EntityNotFoundException formata mensagem corretamente
- [ ] Ambas herdam de DomainException

### Enums
- [ ] UserType: Valores corretos (1 e 2)
- [ ] SeverityLevel: Valores ordenados (0-4)
- [ ] LogEventCode: Todos os códigos definidos

### Build
- [ ] Compilação sem warnings/errors
- [ ] Zero dependências NuGet
- [ ] Nullable reference types habilitado

---

## 7️⃣ Problemas Comuns e Soluções

### ❌ Problema: ValidationException não está sendo lançada

**Solução:**
- Verifique que está passando valor inválido
- Verifique que está importando `HeimdallWeb.Domain.Exceptions`

### ❌ Problema: Value Object não aceita conversão implícita

**Solução:**
- Verifique que o operador `implicit` está definido no VO
- Use conversão explícita: `(string)emailAddress`

### ❌ Problema: Entidade não compila

**Solução:**
- Verifique que todas as propriedades required estão inicializadas
- Use `null!` para propriedades que serão setadas pelo EF Core depois

---

## 📊 Relatório de Testes

Após executar todos os testes, preencha:

**Data dos testes:** ___________
**Executado por:** ___________

**Resultado:**
- Total de testes executados: ___________
- Testes passaram: ___________
- Testes falharam: ___________

**Observações:**
```
[Escreva aqui qualquer observação sobre os testes]
```

---

## 🚀 Próximos Passos

Após validar a Domain Layer:

1. ✅ Marcar Fase 1 como concluída em `plano_migracao.md`
2. ➡️ Iniciar **Fase 2: Infrastructure Layer**
   - EF Core entity configurations
   - Migração PostgreSQL
   - Implementação de repositories
   - Testes de integração com banco

---

**Referências:**
- `docs/Phase1_Domain_Implementation_Summary.md` - Documentação completa
- `docs/Domain_Usage_Examples.md` - Exemplos de uso
- `src/HeimdallWeb.Domain/` - Código fonte
