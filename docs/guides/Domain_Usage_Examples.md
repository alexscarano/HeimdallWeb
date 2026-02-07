# HeimdallWeb Domain Layer - Usage Examples

This document demonstrates how to use the domain entities, value objects, and exceptions correctly.

---

## Value Objects

### EmailAddress

```csharp
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Domain.Exceptions;

// Valid email
var email = EmailAddress.Create("user@example.com");
Console.WriteLine(email.Value); // "user@example.com"

// Normalization (lowercase)
var email2 = EmailAddress.Create("User@EXAMPLE.COM");
Console.WriteLine(email2.Value); // "user@example.com"

// Validation error
try
{
    var invalid = EmailAddress.Create("not-an-email");
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // "Email address 'not-an-email' is not valid."
}

// Implicit conversion to string
string emailString = email; // Implicit cast works
```

### ScanTarget

```csharp
using HeimdallWeb.Domain.ValueObjects;

// Normalization examples
var target1 = ScanTarget.Create("https://www.example.com/path");
Console.WriteLine(target1.Value); // "example.com" (normalized)

var target2 = ScanTarget.Create("example.com");
Console.WriteLine(target2.Value); // "example.com"

var target3 = ScanTarget.Create("www.example.com/");
Console.WriteLine(target3.Value); // "example.com"

// Validation error
try
{
    var invalid = ScanTarget.Create("not a domain");
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // "Scan target 'not a domain' is not a valid domain or URL."
}
```

### ScanDuration

```csharp
using HeimdallWeb.Domain.ValueObjects;

// Create from seconds
var duration = ScanDuration.FromSeconds(45.5);
Console.WriteLine(duration.Value); // TimeSpan: 00:00:45.5000000

// Create from TimeSpan
var duration2 = ScanDuration.Create(TimeSpan.FromMinutes(2));

// Implicit conversion to TimeSpan
TimeSpan ts = duration; // Works automatically

// Validation error
try
{
    var invalid = ScanDuration.FromSeconds(-5);
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // "Scan duration must be positive."
}
```

---

## Entities

### User

```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Domain.Enums;

// Create a new user
var email = EmailAddress.Create("john@example.com");
var user = new User(
    username: "johndoe",
    email: email,
    passwordHash: "hashed_password_here",
    userType: UserType.Default
);

// Domain methods (encapsulated business logic)
user.Activate();
user.UpdatePassword("new_hashed_password");
user.UpdateProfileImage("https://example.com/profile.jpg");

// Properties are read-only from outside
// user.IsActive = false; // Compilation error!
// Must use domain methods:
user.Deactivate();

Console.WriteLine(user.IsActive); // false
Console.WriteLine(user.UpdatedAt); // DateTime when deactivated
```

### ScanHistory

```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;

var target = ScanTarget.Create("example.com");
var scan = new ScanHistory(target, userId: 1);

Console.WriteLine(scan.HasCompleted); // false
Console.WriteLine(scan.CreatedDate); // DateTime.UtcNow

// Complete the scan
scan.CompleteScan(
    duration: TimeSpan.FromSeconds(45),
    rawJsonResult: "{\"findings\": [...]}",
    summary: "Scan completed successfully"
);

Console.WriteLine(scan.HasCompleted); // true
Console.WriteLine(scan.Duration?.Value); // TimeSpan: 00:00:45

// Cannot complete twice
try
{
    scan.CompleteScan(TimeSpan.FromSeconds(10), "{}", "test");
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // "Scan is already marked as completed."
}

// Mark as incomplete (e.g., timeout)
var scan2 = new ScanHistory(target, userId: 1);
scan2.MarkAsIncomplete("Scan timed out after 75 seconds");
Console.WriteLine(scan2.HasCompleted); // false
Console.WriteLine(scan2.Summary); // "Scan timed out after 75 seconds"
```

### Finding

```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;

var finding = new Finding(
    type: "SSL Certificate Expired",
    description: "The SSL certificate for this domain has expired",
    severity: SeverityLevel.Critical,
    evidence: "Certificate expiry: 2024-01-01",
    recommendation: "Renew the SSL certificate immediately",
    historyId: 1
);

// Update severity
finding.UpdateSeverity(SeverityLevel.High);
Console.WriteLine(finding.Severity); // High

// Update recommendation
finding.UpdateRecommendation("Renew SSL certificate and implement auto-renewal");
```

### Technology

```csharp
using HeimdallWeb.Domain.Entities;

var tech = new Technology(
    name: "nginx",
    category: "Web Server",
    description: "High-performance HTTP server and reverse proxy",
    version: "1.24.0",
    historyId: 1
);

Console.WriteLine(tech.Name); // "nginx"
Console.WriteLine(tech.Version); // "1.24.0"
```

### IASummary

```csharp
using HeimdallWeb.Domain.Entities;

var summary = new IASummary(
    summaryText: "The target has several critical security vulnerabilities",
    mainCategory: "SSL",
    overallRisk: "High",
    totalFindings: 5,
    findingsCritical: 2,
    findingsHigh: 1,
    findingsMedium: 2,
    findingsLow: 0,
    iaNotes: "Immediate action required",
    historyId: 1
);

// Update the summary
summary.UpdateSummary(
    summaryText: "Updated analysis after remediation",
    iaNotes: "Some vulnerabilities addressed"
);
```

### AuditLog

```csharp
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;

var log = new AuditLog(
    code: LogEventCode.SCAN_COMPLETED,
    level: "Info",
    message: "Scan completed successfully",
    source: "ScanService",
    details: "Scan took 45 seconds, found 5 issues",
    userId: 1,
    historyId: 1,
    remoteIp: "192.168.1.100"
);

Console.WriteLine(log.Timestamp); // DateTime.UtcNow (when created)
Console.WriteLine(log.Code); // SCAN_COMPLETED
```

### UserUsage

```csharp
using HeimdallWeb.Domain.Entities;

var usage = new UserUsage(userId: 1, date: DateTime.Today);

Console.WriteLine(usage.RequestCounts); // 0

// Increment requests
usage.IncrementRequests();
usage.IncrementRequests();
usage.IncrementRequests(5);

Console.WriteLine(usage.RequestCounts); // 7

// Reset
usage.ResetRequests();
Console.WriteLine(usage.RequestCounts); // 0
```

---

## Exception Handling

### ValidationException

```csharp
using HeimdallWeb.Domain.Exceptions;
using HeimdallWeb.Domain.ValueObjects;

try
{
    var email = EmailAddress.Create(""); // Empty email
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // "Email address cannot be empty."
}

try
{
    var user = new User("abc", email, "hash"); // Username too short
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // "Username must have at least 6 characters."
}
```

### EntityNotFoundException

```csharp
using HeimdallWeb.Domain.Exceptions;

// Typically thrown by repositories when entity not found
throw new EntityNotFoundException("User", 123);
// Message: "User with key 123 was not found."
```

---

## Repository Usage (Interfaces)

**Note:** These are interfaces only. Implementations will be in the Infrastructure layer.

```csharp
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;

// Example: How repositories will be used (once implemented in Infrastructure)

public class SomeApplicationService
{
    private readonly IUserRepository _userRepository;
    private readonly IScanHistoryRepository _scanHistoryRepository;

    public SomeApplicationService(
        IUserRepository userRepository,
        IScanHistoryRepository scanHistoryRepository)
    {
        _userRepository = userRepository;
        _scanHistoryRepository = scanHistoryRepository;
    }

    public async Task<User?> GetUserByEmailAsync(string emailString)
    {
        var email = EmailAddress.Create(emailString);
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<bool> CheckUserExistsAsync(string emailString)
    {
        var email = EmailAddress.Create(emailString);
        return await _userRepository.ExistsByEmailAsync(email);
    }

    public async Task<IEnumerable<ScanHistory>> GetUserScansAsync(int userId)
    {
        return await _scanHistoryRepository.GetByUserIdAsync(userId);
    }
}
```

---

## Best Practices Summary

### 1. Always Create Value Objects Through Factory Methods
```csharp
// ✅ CORRECT
var email = EmailAddress.Create("user@example.com");

// ❌ WRONG - Cannot use 'new' (constructor is private)
// var email = new EmailAddress("user@example.com");
```

### 2. Use Domain Methods Instead of Property Setters
```csharp
var user = new User("johndoe", email, "hash");

// ✅ CORRECT - Use domain methods
user.Activate();
user.UpdatePassword("new_hash");

// ❌ WRONG - Properties have private setters
// user.IsActive = true; // Compilation error
// user.PasswordHash = "new_hash"; // Compilation error
```

### 3. Handle Validation Exceptions
```csharp
// ✅ CORRECT - Catch ValidationException
try
{
    var email = EmailAddress.Create(userInput);
    var user = new User(username, email, passwordHash);
}
catch (ValidationException ex)
{
    // Log and return error to user
    return BadRequest(ex.Message);
}
```

### 4. Use Repository Interfaces, Not Concrete Implementations
```csharp
// ✅ CORRECT - Depend on abstraction
public class MyService
{
    private readonly IUserRepository _userRepository;

    public MyService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}

// ❌ WRONG - Don't depend on concrete repository class
// public class MyService
// {
//     private readonly UserRepository _userRepository; // ❌ Tight coupling
// }
```

### 5. Never Modify Collections Directly
```csharp
var user = new User("johndoe", email, "hash");

// ✅ CORRECT - Read from collections
var scanCount = user.ScanHistories.Count;

// ❌ WRONG - Cannot modify (IReadOnlyCollection)
// user.ScanHistories.Add(new ScanHistory(...)); // Compilation error
```

### 6. Use UTC for All DateTime Values
```csharp
// ✅ Domain entities use DateTime.UtcNow internally
var user = new User("johndoe", email, "hash");
Console.WriteLine(user.CreatedAt.Kind); // DateTimeKind.Utc

// Convert to local timezone in presentation layer only
var localTime = user.CreatedAt.ToLocalTime();
```

---

## Common Patterns

### Creating a Complete Scan Workflow

```csharp
// 1. Create user
var email = EmailAddress.Create("analyst@security.com");
var user = new User("security_analyst", email, "hashed_password");

// 2. Start scan
var target = ScanTarget.Create("https://target-website.com");
var scan = new ScanHistory(target, user.UserId);

// 3. Add findings
var finding1 = new Finding(
    type: "Missing Security Header",
    description: "X-Frame-Options header is missing",
    severity: SeverityLevel.Medium,
    evidence: "Header not found in response",
    recommendation: "Add X-Frame-Options: DENY header",
    historyId: scan.HistoryId
);

var finding2 = new Finding(
    type: "SSL Certificate Issue",
    description: "Certificate uses weak cipher",
    severity: SeverityLevel.High,
    evidence: "TLS_RSA_WITH_RC4_128_SHA detected",
    recommendation: "Disable weak ciphers and use modern TLS 1.3",
    historyId: scan.HistoryId
);

// 4. Complete scan
scan.CompleteScan(
    duration: TimeSpan.FromSeconds(32),
    rawJsonResult: "{\"findings\": [...]}", // JSON from scanners
    summary: "Scan found 2 security issues"
);

// 5. Add AI summary
var aiSummary = new IASummary(
    summaryText: "Target has medium-risk security issues",
    mainCategory: "Headers",
    overallRisk: "Medium",
    totalFindings: 2,
    findingsCritical: 0,
    findingsHigh: 1,
    findingsMedium: 1,
    findingsLow: 0,
    historyId: scan.HistoryId
);

// 6. Log the event
var log = new AuditLog(
    code: LogEventCode.SCAN_COMPLETED,
    level: "Info",
    message: "Security scan completed successfully",
    source: "ScanService",
    userId: user.UserId,
    historyId: scan.HistoryId
);

// 7. Track usage
var usage = new UserUsage(user.UserId, DateTime.Today);
usage.IncrementRequests();
```

---

## Testing Domain Entities (Unit Tests)

```csharp
using Xunit;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Domain.Exceptions;

public class UserTests
{
    [Fact]
    public void User_Create_ValidUser_Success()
    {
        // Arrange
        var email = EmailAddress.Create("test@example.com");

        // Act
        var user = new User("testuser", email, "hashed_password");

        // Assert
        Assert.Equal("testuser", user.Username);
        Assert.Equal(email, user.Email);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_Create_ShortUsername_ThrowsValidationException()
    {
        // Arrange
        var email = EmailAddress.Create("test@example.com");

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() =>
            new User("abc", email, "hashed_password")
        );

        Assert.Contains("6 characters", exception.Message);
    }

    [Fact]
    public void User_Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var email = EmailAddress.Create("test@example.com");
        var user = new User("testuser", email, "hashed_password");

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }
}
```

---

This document provides practical examples of how to use the HeimdallWeb Domain Layer correctly. Follow these patterns to ensure proper domain-driven design and maintainable code.
