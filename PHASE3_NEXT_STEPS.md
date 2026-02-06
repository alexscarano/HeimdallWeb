# Phase 3 - Next Steps & Implementation Guide

## Current Status

✅ **ExecuteScanCommand COMPLETE** - The most complex handler is done and serves as the template for all others.

🔨 **Remaining:** 17 simpler handlers to implement following the established pattern.

---

## Immediate Action Items (Blocking)

### 1. Fix Scanner Build Errors (1-2 hours)

**Problem:** 5 compilation errors - Scanners reference `HeimdallWeb.Helpers.NetworkUtils`

**Solution:**
```bash
# Copy NetworkUtils.cs to Application layer
cp HeimdallWebOld/Helpers/NetworkUtils.cs src/HeimdallWeb.Application/Helpers/

# Update namespace in NetworkUtils.cs
# FROM: namespace HeimdallWeb.Helpers
# TO: namespace HeimdallWeb.Application.Helpers

# Update scanner using statements
# FROM: using HeimdallWeb.Helpers;
# TO: using HeimdallWeb.Application.Helpers;
```

**Files to update:**
- `Services/Scanners/HeaderScanner.cs`
- `Services/Scanners/HttpRedirectScanner.cs`
- `Services/Scanners/PortScanner.cs`
- `Services/Scanners/SslScanner.cs`
- `Services/Scanners/SensitivePathsScanner.cs` (also needs ASHelpers)

**Verification:**
```bash
dotnet build src/HeimdallWeb.Application/HeimdallWeb.Application.csproj
# Expected: 0 errors
```

### 2. Remove AutoMapper Packages (5 minutes)

**Reason:** Using explicit extension methods instead of AutoMapper for better clarity and testability

**Edit:** `src/HeimdallWeb.Application/HeimdallWeb.Application.csproj`

**Remove these packages:**
```xml
<!-- DELETE these lines completely: -->
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
```

**Keep only:**
```xml
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
```

---

## Phase 3 Completion Roadmap (18-26 hours)

### Priority 1: Auth & User Management (8-10 hours)

#### A. Login & Registration (2-3 hours)

**1. LoginCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/Auth/Login/LoginCommand.cs
src/HeimdallWeb.Application/Commands/Auth/Login/LoginCommandHandler.cs
src/HeimdallWeb.Application/Commands/Auth/Login/LoginCommandValidator.cs
```

**Source:** `LoginController.Enter` (lines 30-82)

**Key Features:**
- User lookup by email or username
- Password verification (use `PasswordUtils.VerifyPassword`)
- JWT token generation (use `TokenService.generateToken`)
- Audit logging (`USER_LOGIN`, `USER_LOGIN_FAILED`)

**Command:**
```csharp
public record LoginCommand(
    string EmailOrUsername,
    string Password,
    string RemoteIp
);
```

**Response:**
```csharp
// Use existing LoginResponse DTO
public record LoginResponse(
    string Token,
    int UserId,
    string Username,
    bool IsAdmin
);
```

---

**2. RegisterUserCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/User/RegisterUser/RegisterUserCommand.cs
src/HeimdallWeb.Application/Commands/User/RegisterUser/RegisterUserCommandHandler.cs
src/HeimdallWeb.Application/Commands/User/RegisterUser/RegisterUserCommandValidator.cs
```

**Source:** `UserController.Register` (lines 73-96)

**Key Features:**
- Duplicate user check
- Password hashing (use `PasswordUtils.HashPassword`)
- User entity creation
- Auto-login (generate JWT)

**Command:**
```csharp
public record RegisterUserCommand(
    string Username,
    string Email,
    string Password
);
```

---

#### B. User Profile Management (3-4 hours)

**3. UpdateUserCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/User/UpdateUser/UpdateUserCommand.cs
src/HeimdallWeb.Application/Commands/User/UpdateUser/UpdateUserCommandHandler.cs
src/HeimdallWeb.Application/Commands/User/UpdateUser/UpdateUserCommandValidator.cs
```

**Source:** `UserController.Profile` (update action, lines 129-173)

**Key Features:**
- Duplicate email/username check
- Profile image upload support
- User entity update
- Transaction management

**Command:**
```csharp
public record UpdateUserCommand(
    int UserId,
    string Username,
    string Email,
    string? ProfileImagePath
);
```

---

**4. DeleteUserCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/User/DeleteUser/DeleteUserCommand.cs
src/HeimdallWeb.Application/Commands/User/DeleteUser/DeleteUserCommandHandler.cs
src/HeimdallWeb.Application/Commands/User/DeleteUser/DeleteUserCommandValidator.cs
```

**Source:** `UserController.Profile` (delete action, lines 174-199)

**Key Features:**
- Password verification
- Confirmation check
- User deletion
- Cascade handling (via EF Core)

**Command:**
```csharp
public record DeleteUserCommand(
    int UserId,
    string Password,
    bool ConfirmDelete
);
```

---

#### C. Admin User Management (2-3 hours)

**5. ToggleUserStatusCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/User/ToggleUserStatus/ToggleUserStatusCommand.cs
src/HeimdallWeb.Application/Commands/User/ToggleUserStatus/ToggleUserStatusCommandHandler.cs
src/HeimdallWeb.Application/Commands/User/ToggleUserStatus/ToggleUserStatusCommandValidator.cs
```

**Source:** `AdminController.ToggleUserStatus` (lines 94-118)

**Key Features:**
- Admin role check
- Admin protection (can't block other admins)
- User activate/deactivate

**Command:**
```csharp
public record ToggleUserStatusCommand(
    int UserId,
    bool IsActive
);
```

---

**6. DeleteUserByAdminCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/User/DeleteUserByAdmin/DeleteUserByAdminCommand.cs
src/HeimdallWeb.Application/Commands/User/DeleteUserByAdmin/DeleteUserByAdminCommandHandler.cs
src/HeimdallWeb.Application/Commands/User/DeleteUserByAdmin/DeleteUserByAdminCommandValidator.cs
```

**Source:** `AdminController.DeleteUser` (lines 122-141)

**Command:**
```csharp
public record DeleteUserByAdminCommand(
    int UserId
);
```

---

### Priority 2: Queries (8-12 hours)

#### A. User Queries (1-2 hours)

**7. GetUserProfileQuery**
```bash
# Files to create:
src/HeimdallWeb.Application/Queries/User/GetUserProfile/GetUserProfileQuery.cs
src/HeimdallWeb.Application/Queries/User/GetUserProfile/GetUserProfileQueryHandler.cs
src/HeimdallWeb.Application/Queries/User/GetUserProfile/GetUserProfileQueryValidator.cs
```

**Source:** `UserController.Profile` (GET, lines 34-56)

**Query:**
```csharp
public record GetUserProfileQuery(int UserId);
```

---

**8. GetUserStatisticsQuery**
```bash
# Files to create:
src/HeimdallWeb.Application/Queries/User/GetUserStatistics/GetUserStatisticsQuery.cs
src/HeimdallWeb.Application/Queries/User/GetUserStatistics/GetUserStatisticsQueryHandler.cs
src/HeimdallWeb.Application/Queries/User/GetUserStatistics/GetUserStatisticsQueryValidator.cs
```

**Source:** `UserController.Statistics` (lines 59-70)

---

#### B. Scan Queries (4-5 hours)

**9. GetFindingsByHistoryIdQuery**  
**10. GetTechnologiesByHistoryIdQuery**  
**11. GetScanHistoryByIdQuery**  
**12. GetUserScanHistoriesQuery**  
**13. ExportHistoryPdfQuery**  
**14. ExportSingleHistoryPdfQuery**

*Follow same pattern as ExecuteScanCommand*

#### C. Admin Queries (2-3 hours)

**15. GetAdminDashboardQuery**  
**16. GetUsersQuery**

---

### Priority 3: Configuration & Mapping (3-4 hours)

#### A. Extension Methods for Mapping (2-3 hours)

**Create:**
```bash
src/HeimdallWeb.Application/Extensions/ScanHistoryExtensions.cs
src/HeimdallWeb.Application/Extensions/UserExtensions.cs
src/HeimdallWeb.Application/Extensions/FindingExtensions.cs
src/HeimdallWeb.Application/Extensions/TechnologyExtensions.cs
```

**Example - ScanHistoryExtensions.cs:**
```csharp
namespace HeimdallWeb.Application.Extensions;

public static class ScanHistoryExtensions
{
    public static ExecuteScanResponse ToDto(this ScanHistory scanHistory)
    {
        return new ExecuteScanResponse(
            HistoryId: scanHistory.HistoryId,
            Target: scanHistory.Target.Value,
            Duration: scanHistory.Duration.Value,
            HasCompleted: scanHistory.HasCompleted,
            Summary: scanHistory.Summary
        );
    }

    public static ScanHistoryDto ToScanHistoryDto(this ScanHistory scanHistory)
    {
        return new ScanHistoryDto
        {
            HistoryId = scanHistory.HistoryId,
            Target = scanHistory.Target.Value,
            Duration = scanHistory.Duration.Value.ToString(@"hh\:mm\:ss"),
            CreatedDate = scanHistory.CreatedDate,
            HasCompleted = scanHistory.HasCompleted,
            Summary = scanHistory.Summary
        };
    }
}

public static class FindingExtensions
{
    public static FindingResponse ToDto(this Finding finding)
    {
        return new FindingResponse
        {
            FindingId = finding.FindingId,
            Type = finding.Type,
            Description = finding.Description,
            Severity = finding.Severity.ToString(),
            Evidence = finding.Evidence,
            Recommendation = finding.Recommendation
        };
    }
}
```

**Benefits of Extension Methods:**
- ✅ Explicit and testable
- ✅ No magic/reflection overhead
- ✅ IDE autocomplete support
- ✅ Easy to debug
- ✅ No external dependencies

---

#### B. Dependency Injection (1 hour)

**Create:** `src/HeimdallWeb.Application/DependencyInjection.cs`

```csharp
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HeimdallWeb.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Extension methods are static - no registration needed

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Application Services
        services.AddScoped<IScannerService, ScannerService>();
        services.AddScoped<IGeminiService, GeminiService>();
        // Add TokenService when copied

        // Command Handlers (register as you implement them)
        // services.AddScoped<ICommandHandler<ExecuteScanCommand, ExecuteScanResponse>, ExecuteScanCommandHandler>();
        // services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginCommandHandler>();
        // ... etc

        return services;
    }
}
```

**Usage in WebAPI Program.cs:**
```csharp
builder.Services.AddApplication();
```

---

### Priority 4: Remaining Scan Commands (0.5-1 hour)

**17. DeleteScanHistoryCommand**
```bash
# Files to create:
src/HeimdallWeb.Application/Commands/Scan/DeleteScanHistory/DeleteScanHistoryCommand.cs
src/HeimdallWeb.Application/Commands/Scan/DeleteScanHistory/DeleteScanHistoryCommandHandler.cs
src/HeimdallWeb.Application/Commands/Scan/DeleteScanHistory/DeleteScanHistoryCommandValidator.cs
```

**Source:** `HistoryController.DeleteHistory` (lines 44-63)

**Command:**
```csharp
public record DeleteScanHistoryCommand(
    int HistoryId,
    int UserId  // For authorization check
);
```

---

## Implementation Pattern (Follow for All Handlers)

### 1. Command/Query (Simple Record)
```csharp
namespace HeimdallWeb.Application.Commands.[Feature].[Action];

public record [Action]Command(
    // Parameters from controller/service
    string Parameter1,
    int Parameter2
);
```

### 2. Validator (FluentValidation)
```csharp
namespace HeimdallWeb.Application.Commands.[Feature].[Action];

public class [Action]CommandValidator : AbstractValidator<[Action]Command>
{
    public [Action]CommandValidator()
    {
        RuleFor(x => x.Parameter1)
            .NotEmpty().WithMessage("Parameter1 is required")
            .MaximumLength(100).WithMessage("Parameter1 too long");
        
        RuleFor(x => x.Parameter2)
            .GreaterThan(0).WithMessage("Parameter2 must be positive");
    }
}
```

### 3. Handler (Business Logic)
```csharp
namespace HeimdallWeb.Application.Commands.[Feature].[Action];

public class [Action]CommandHandler : ICommandHandler<[Action]Command, [Action]Response>
{
    private readonly IUnitOfWork _unitOfWork;
    // ... other dependencies

    public [Action]CommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<[Action]Response> Handle([Action]Command command, CancellationToken ct = default)
    {
        // 1. Validate (if needed beyond FluentValidation)
        // 2. Execute business logic
        // 3. Save to database
        // 4. Log if needed
        // 5. Return response
    }
}
```

**Use ExecuteScanCommandHandler as your reference implementation!**

---

## Verification Checklist

After implementing each handler:

- [ ] Command/Query created (record)
- [ ] Validator created (FluentValidation rules)
- [ ] Handler created (implements ICommandHandler/IQueryHandler)
- [ ] Handler uses IUnitOfWork
- [ ] Handler logs to AuditLog if critical operation
- [ ] Exceptions use Application layer exceptions (not domain exceptions)
- [ ] DTO created if new response type needed
- [ ] No references to HttpContext, HttpContextAccessor, or ASP.NET Core types
- [ ] Handler compiles without errors
- [ ] Validator has at least basic tests (NotEmpty, ranges, formats)

---

## Build Verification Commands

```bash
# After each handler implementation
dotnet clean
dotnet build src/HeimdallWeb.Application/HeimdallWeb.Application.csproj

# After all handlers
dotnet build  # Full solution build

# Expected result:
# 0 Error(s)
# 0 Warning(s)
```

---

## Final Deliverables

When Phase 3 is complete, you should have:

1. **18 Command/Query handlers** (8 commands + 10 queries)
2. **18 Validators** (one per handler)
3. **20+ DTOs** (requests and responses)
4. **Extension methods** ToDto()/ToDomain() for all entities
5. **1 DependencyInjection.cs** (registers all services)
6. **0 build errors**
7. **0 build warnings**
8. **Zero references to HeimdallWebOld namespace**

---

## Estimated Timeline

| Task | Hours | When |
|------|-------|------|
| Fix scanner dependencies | 1-2h | NOW |
| Auth commands (2) | 2-3h | Day 1 |
| User commands (4) | 3-4h | Day 1-2 |
| User queries (2) | 1-2h | Day 2 |
| Scan queries (6) | 4-5h | Day 2-3 |
| Admin queries (2) | 2-3h | Day 3 |
| Extension methods ToDto()/ToDomain() | 2-3h | Day 3 |
| DependencyInjection.cs | 1h | Day 3 |
| Final verification | 2h | Day 3 |
| **TOTAL** | **18-26h** | **3-4 days** |

---

## Tips for Success

1. **Use ExecuteScanCommandHandler as template** - It has all the patterns you need
2. **Start simple** - LoginCommand is easier than ExecuteScanCommand
3. **Test as you go** - Build after each handler
4. **Copy-paste smartly** - Reuse validation patterns
5. **Follow naming conventions** - Consistency is key
6. **Don't skip validators** - They prevent bad data
7. **Log critical operations** - Use AuditLog for important actions
8. **Keep handlers thin** - Business logic in domain entities when possible

---

**Ready to start? Begin with fixing scanner dependencies!**

```bash
# Step 1: Fix scanners
mkdir -p src/HeimdallWeb.Application/Helpers
cp HeimdallWebOld/Helpers/NetworkUtils.cs src/HeimdallWeb.Application/Helpers/

# Step 2: Update namespaces in NetworkUtils.cs and scanners
# (use your IDE's find-replace: HeimdallWeb.Helpers → HeimdallWeb.Application.Helpers)

# Step 3: Verify
dotnet build src/HeimdallWeb.Application/HeimdallWeb.Application.csproj
```

---

**Last Updated:** 2026-02-05 23:55 BRT  
**Next Action:** Fix scanner dependencies → Implement LoginCommand
