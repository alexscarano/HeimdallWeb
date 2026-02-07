# Program.cs Architecture - Reference Guide

**Created**: 2025-01-XX  
**Status**: ✅ Active  
**Version**: 1.0

---

## 📊 Quick Reference

```
STARTUP FLOW:
1. var builder = WebApplication.CreateBuilder(args);
2. builder.Services.Add*Configuration()           ← Extension methods register DI
3. var app = builder.Build();
4. app.Use*()                                      ← Extension methods configure middleware
5. app.Map*Endpoints()                            ← Extension methods map routes
6. app.Run();
```

---

## 🏗️ Directory Structure & Responsibilities

```
src/HeimdallWeb.WebApi/
│
├── Program.cs (60 lines)
│   └── Contains ONLY:
│       • Using statements for extension methods
│       • builder.Services.Add*Configuration() calls
│       • app.Use*() calls
│       • app.Map*Endpoints() calls
│
├── Configuration/
│   └── EndpointConfiguration.cs
│       └── MapAllEndpoints()
│           → Maps all 5 endpoint groups
│           → Single source of truth for routes
│
├── Middleware/
│   ├── DevelopmentMiddleware.cs
│   │   └── UseSwaggerDevelopment()
│   │       → Swagger UI (development only)
│   │       → Environment guard
│   │
│   └── SecurityMiddleware.cs
│       └── UseSecurityMiddlewarePipeline()
│           → UseHttpsRedirection()
│           → UseCors()
│           → UseAuthentication()
│           → UseAuthorization()
│           → UseRateLimiter()
│           ⚠️ ORDER IS CRITICAL - DO NOT CHANGE
│
├── ServiceRegistration/
│   ├── AuthenticationConfiguration.cs
│   │   ├── AddJwtAuthenticationConfiguration()
│   │   │   → JWT validation parameters
│   │   │   → Cookie extraction logic
│   │   │   → Reads from appsettings.json
│   │   │
│   │   └── AddAuthorizationConfiguration()
│   │       → Authorization service registration
│   │
│   ├── CorsConfiguration.cs
│   │   └── AddCorsConfiguration()
│   │       → CORS policy for Next.js (localhost:3000)
│   │       → AllowCredentials for JWT cookies
│   │
│   ├── LayerRegistration.cs
│   │   ├── AddApplicationLayer()
│   │   │   → Handlers, Validators, Services
│   │   │
│   │   └── AddInfrastructureLayer()
│   │       → DbContext, Repositories, External APIs
│   │
│   ├── RateLimitingConfiguration.cs
│   │   └── AddRateLimitingConfiguration()
│   │       → Global: 85 req/min per IP
│   │       → ScanPolicy: 4 req/min per IP
│   │
│   └── SwaggerConfiguration.cs
│       └── AddSwaggerConfiguration()
│           → API documentation
│           → Security definitions
│           → Cookie-based JWT scheme
│
└── Endpoints/
    ├── AuthenticationEndpoints.cs
    │   └── MapAuthenticationEndpoints()
    │       → POST /api/v1/auth/login
    │       → POST /api/v1/auth/register
    │       → POST /api/v1/auth/logout
    │
    ├── ScanEndpoints.cs
    │   └── MapScanEndpoints()
    │       → POST /api/v1/scan/execute
    │       → GET /api/v1/scan/list
    │       → GET /api/v1/scan/{id}
    │
    ├── HistoryEndpoints.cs
    │   └── MapHistoryEndpoints()
    │       → GET /api/v1/history
    │       → GET /api/v1/history/{id}
    │
    ├── UserEndpoints.cs
    │   └── MapUserEndpoints()
    │       → GET /api/v1/user/profile
    │       → PUT /api/v1/user/profile
    │
    └── DashboardEndpoints.cs
        └── MapDashboardEndpoints()
            → GET /api/v1/dashboard/admin
            → GET /api/v1/dashboard/user/{id}
```

---

## 🔄 Call Chain

### Service Registration (Order doesn't matter for DI)

```
Program.cs
├─ AddSwaggerConfiguration()
│  └─ builder.Services.AddEndpointsApiExplorer()
│  └─ builder.Services.AddSwaggerGen(options => ...)
│
├─ AddCorsConfiguration()
│  └─ builder.Services.AddCors(options => ...)
│
├─ AddJwtAuthenticationConfiguration()
│  └─ builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
│  └─ builder.Services.AddJwtBearer(options => ...)
│
├─ AddAuthorizationConfiguration()
│  └─ builder.Services.AddAuthorization()
│
├─ AddRateLimitingConfiguration()
│  └─ builder.Services.AddRateLimiter(options => ...)
│
├─ AddApplicationLayer()
│  └─ builder.Services.AddApplication()
│     └─ ICommandHandler<T, R> registered
│     └─ IQueryHandler<T, R> registered
│     └─ Validators registered
│     └─ Services registered
│
└─ AddInfrastructureLayer()
   └─ builder.Services.AddInfrastructure(configuration)
      └─ DbContext configured
      └─ IRepository<T> registered
      └─ IUnitOfWork registered
      └─ External services registered
```

### Middleware Pipeline (Order IS CRITICAL)

```
HTTP Request
    ↓
[HTTPS Redirection] ← Redirect HTTP to HTTPS
    ↓
[CORS] ← Validate origin early (before auth waste)
    ↓
[Authentication] ← Identify user (JWT from cookie)
    ↓
[Authorization] ← Check permissions (RequireAuthorization())
    ↓
[Rate Limiting] ← Count all requests (per IP)
    ↓
[Route Handler] ← Execute endpoint logic
    ↓
Response
```

### Endpoint Mapping (Registers route handlers)

```
Program.cs: app.MapAllEndpoints()
    ↓
Configuration/EndpointConfiguration.cs: MapAllEndpoints()
    ├─ app.MapAuthenticationEndpoints()
    │  └─ /api/v1/auth group
    │     ├─ POST /login (AllowAnonymous)
    │     ├─ POST /register (AllowAnonymous)
    │     └─ POST /logout (RequireAuthorization)
    │
    ├─ app.MapScanEndpoints()
    │  └─ /api/v1/scan group (RequireAuthorization)
    │
    ├─ app.MapHistoryEndpoints()
    │  └─ /api/v1/history group (RequireAuthorization)
    │
    ├─ app.MapUserEndpoints()
    │  └─ /api/v1/user group (RequireAuthorization)
    │
    └─ app.MapDashboardEndpoints()
       └─ /api/v1/dashboard group (RequireAuthorization)
```

---

## 🔐 Security Flow

### JWT Authentication Flow

```
1. Client sends: POST /api/v1/auth/login { username, password }
   ↓
2. Server validates credentials
   ↓
3. Server creates JWT token
   ↓
4. Server sets cookie: authHeimdallCookie = JWT (HttpOnly, Secure, SameSite=Strict)
   ↓
5. Client browser stores cookie (automatic)
   ↓
6. Subsequent requests: Browser auto-includes cookie
   ↓
7. Server extracts JWT from cookie (OnMessageReceived event)
   ↓
8. TokenValidationParameters validate:
   - Issuer (must match config)
   - Audience (must match config)
   - Lifetime (not expired)
   - Signature (valid key)
   ↓
9. ClaimsPrincipal created
   ↓
10. Authorization checks [RequireAuthorization]
    ↓
11. Rate limit checks (global + policy)
    ↓
12. Route handler executes
```

### CORS Flow

```
1. Client (localhost:3000) sends preflight: OPTIONS /api/v1/auth/login
   ↓
2. Server CORS middleware checks:
   - Origin: must be in whitelist (localhost:3000, https://localhost:3000)
   - Method: must be in allowed methods (GET, POST, PUT, DELETE, etc)
   - Headers: must be in allowed headers (Content-Type, etc)
   ↓
3. Server responds:
   - Access-Control-Allow-Origin: http://localhost:3000
   - Access-Control-Allow-Methods: GET, POST, PUT, DELETE
   - Access-Control-Allow-Headers: Content-Type, Authorization
   - Access-Control-Allow-Credentials: true  ← IMPORTANT for cookies
   ↓
4. Client browser checks response
   ↓
5. If OK, browser sends actual request (with credentials)
   ↓
6. Browser includes cookie automatically (credentials: include in fetch)
   ↓
7. Server processes normally
```

---

## 📋 Extension Method Naming Convention

### Service Registration Methods

**Pattern**: `Add*Configuration()`

```csharp
builder.Services.AddSwaggerConfiguration()      // API docs
builder.Services.AddCorsConfiguration()         // CORS policy
builder.Services.AddJwtAuthenticationConfiguration(config)  // JWT
builder.Services.AddAuthorizationConfiguration()           // AuthZ
builder.Services.AddRateLimitingConfiguration()           // Rate limit
builder.Services.AddApplicationLayer()         // Handlers, validators
builder.Services.AddInfrastructureLayer(config) // DbContext, repos
```

### Middleware Registration Methods

**Pattern**: `Use*()` or `Use*Middleware()`

```csharp
app.UseSwaggerDevelopment()            // Swagger UI (dev-only)
app.UseSecurityMiddlewarePipeline()    // CORS, Auth, AuthZ, RateLimit
```

### Route Mapping Methods

**Pattern**: `Map*Endpoints()`

```csharp
app.MapAuthenticationEndpoints()       // /api/v1/auth
app.MapScanEndpoints()                 // /api/v1/scan
app.MapHistoryEndpoints()              // /api/v1/history
app.MapUserEndpoints()                 // /api/v1/user
app.MapDashboardEndpoints()            // /api/v1/dashboard
app.MapAllEndpoints()                  // All of above
```

---

## 🎯 When to Add New Configuration

### Scenario 1: Add Logging

**Current**:
```csharp
// None - logging not configured
```

**Add this**:

Create `ServiceRegistration/LoggingConfiguration.cs`:
```csharp
public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services)
{
    services.AddLogging(config =>
    {
        config.AddConsole();
        config.AddDebug();
        config.SetMinimumLevel(LogLevel.Information);
    });
    return services;
}
```

Update `Program.cs`:
```csharp
builder.Services.AddLoggingConfiguration();
```

### Scenario 2: Add Health Checks

Create `ServiceRegistration/HealthCheckConfiguration.cs`:
```csharp
public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
{
    services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>()
        .AddCheck("GeminiAPI", ...)
        .AddCheck("ScannerService", ...);
    return services;
}

public static WebApplication UseHealthCheckMiddleware(this WebApplication app)
{
    app.UseHealthChecks("/health");
    return app;
}
```

Update `Program.cs`:
```csharp
builder.Services.AddHealthCheckConfiguration();
// ... later ...
app.UseHealthCheckMiddleware();
```

### Scenario 3: Add Caching

Create `ServiceRegistration/CachingConfiguration.cs`:
```csharp
public static IServiceCollection AddCachingConfiguration(this IServiceCollection services)
{
    services.AddStackExchangeRedisCache(options => {...});
    return services;
}
```

Update `Program.cs`:
```csharp
builder.Services.AddCachingConfiguration();
```

### Scenario 4: Add Structured Logging

Create `Middleware/StructuredLoggingMiddleware.cs`:
```csharp
public static WebApplication UseStructuredLogging(this WebApplication app)
{
    app.UseMiddleware<StructuredLoggingMiddleware>();
    return app;
}
```

Update `Program.cs`:
```csharp
app.UseSwaggerDevelopment();
app.UseStructuredLogging();        // Add before security
app.UseSecurityMiddlewarePipeline();
```

---

## ⚠️ CRITICAL - Do NOT Change Middleware Order

```csharp
// ❌ WRONG - CORS after Auth
app.UseAuthentication();
app.UseCors();              // TOO LATE - auth already ran
app.UseAuthorization();
app.UseRateLimiter();

// ✅ CORRECT
app.UseCors();              // First - validate origins
app.UseAuthentication();    // Then - identify user
app.UseAuthorization();     // Then - check permissions
app.UseRateLimiter();       // Last - count all requests
```

**Why Order Matters**:

1. **HTTPS** → Upgrade insecure connections first
2. **CORS** → Reject invalid origins before authentication waste
3. **Authentication** → Identify user via JWT
4. **Authorization** → Check if user can access resource
5. **Rate Limit** → Count request against quota

---

## 🧪 Testing Extension Methods

### Example Unit Test

```csharp
[Fact]
public void AddJwtAuthenticationConfiguration_WithValidConfig_Should_RegisterAuthentication()
{
    // Arrange
    var services = new ServiceCollection();
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Jwt:Key"] = "my-super-secret-key-at-least-32-chars-long!",
            ["Jwt:Issuer"] = "HeimdallWeb",
            ["Jwt:Audience"] = "HeimdallWebUsers"
        })
        .Build();

    // Act
    var result = services.AddJwtAuthenticationConfiguration(config);

    // Assert
    Assert.NotNull(result);
    var provider = services.BuildServiceProvider();
    var authService = provider.GetService<IAuthenticationService>();
    Assert.NotNull(authService);
}
```

---

## 📚 References

### Files
- `Program.cs` - Entry point
- `ServiceRegistration/*.cs` - DI configuration
- `Middleware/*.cs` - Middleware pipeline
- `Configuration/*.cs` - Route mapping
- `Endpoints/*.cs` - Route handlers

### External Documentation
- [ASP.NET Core Middleware Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware)
- [JWT Bearer Options](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions)
- [CORS Policy Builder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.cors.infrastructure.corspolicybuilder)
- [Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)

---

## 💾 Maintenance Tips

1. **Always add new configuration via extension methods**
2. **Keep Program.cs ≤ 100 lines**
3. **Document critical middleware order**
4. **Use consistent naming convention**
5. **Add XML documentation to every public method**
6. **Test extension methods in isolation**
7. **Update this guide when adding new configurations**

---

**Last Updated**: 2025-01-XX  
**Status**: Active (Using in production)  
**Maintainer**: Clean Architecture Pattern
