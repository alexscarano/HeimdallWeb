# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## ⚠️ CRITICAL RULES - READ FIRST

**These rules are MANDATORY and SEVERE. NO exceptions:**

1. **✅ Mark completed tasks** in `plano_migracao.md` after EVERY task
2. **🌐 Use browser automation** (MCP Chrome/Puppeteer) after ANY frontend change — inclusive após o agente `nexus-next-js` completar qualquer alteração
3. **🧪 Test ALL endpoints** after backend sprints + create testing guide (`.MD`)
4. **🚫 NO Docker** for development - use `dotnet build/run` only
5. **🎨 Consult designer agent** for ALL UI/design decisions - NEVER improvise
6. **📋 Follow the plan** - `plano_migracao.md` is the source of truth

**If you violate these rules, STOP and ask for clarification.**

---

## 🤖 Specialized Agents - When to Use

**This project has specialized agents. Use them proactively:**

### designer Agent
**When:** Before implementing ANY frontend component or UI
**Use for:**
- Page layouts and structure
- Color schemes and typography
- Component styling
- Responsive design patterns
- User experience flows

**Example:**
```
Task(subagent_type="designer", prompt="Design the admin dashboard layout...")
```

### nexus-next-js Agent
**When:** Implementing ANY Next.js frontend feature or component
**Use for:**
- Next.js 15 + React 19 page and component implementation
- TailwindCSS + shadcn/ui integration
- App Router structure and routing
- Client/Server component decisions
- API integration with the backend (fetch, React Query, etc.)
- State management in the frontend
- Performance optimization (SSR, SSG, ISR)

**🎨 frontend-design Skill:** The `nexus-next-js` agent MAY load the `frontend-design` skill (via `Skill("frontend-design")`) when greater visual creativity and design quality is needed — e.g., for complex layouts, landing pages, or components that go beyond standard patterns. Use this skill proactively when the task demands high design quality.

**Example:**
```
Task(subagent_type="nexus-next-js", prompt="Implement the login page for HeimdallWeb using the design provided...")
```

**🚨 MANDATORY POST-IMPLEMENTATION:** After the nexus-next-js agent completes ANY frontend change, you MUST use MCP Claude-in-Chrome to:
1. Navigate to the changed page in the browser
2. Take screenshots (desktop + mobile viewport)
3. Test all interactions (clicks, form submissions, navigation)
4. Check browser console for errors
5. Verify responsive behavior
6. Document any visual issues found

### dotnet-backend-expert Agent
**When:** Working on .NET backend code
**Use for:**
- Backend architecture decisions
- EF Core optimization
- Minimal API implementation
- Repository pattern review
- Performance optimization

### sql-data-modeling-expert Agent
**When:** Working with database schema or queries
**Use for:**
- Database migrations
- SQL VIEW optimization
- PostgreSQL conversion
- Query performance tuning
- Index strategy

### linux-devsecops-analyst Agent
**When:** Security, deployment, or infrastructure concerns
**Use for:**
- Security hardening
- CI/CD pipeline setup
- Deployment strategy
- Container security
- Infrastructure as code

### sentinel-cybersec-generalist Agent
**When:** After completing any major feature
**Use for:**
- Code review and feedback
- Security vulnerability analysis
- Best practices validation
- Final quality check

**🚨 IMPORTANT:** Use these agents liberally. They exist to improve quality and prevent mistakes.

---

## Project Overview

**HeimdallWeb** is a web security scanning application currently undergoing migration from ASP.NET Core MVC monolith to a modern DDD Light + Minimal APIs + Next.js architecture.

**Current State:**
- ASP.NET Core 8.0 MVC application with MySQL backend
- ~20,384 lines of C# code
- 7 core tables, 14 SQL VIEWs, 52 migrations
- Security scanning functionality with 7 specialized scanners
- Google Gemini AI integration for vulnerability analysis
- JWT authentication with rate limiting

**Target Architecture:** Modular backend (DDD Light + Minimal APIs) with PostgreSQL + Next.js 15 frontend

**⚠️ CRITICAL:** Always consult `plano_migracao.md` before making architectural decisions. The migration plan defines the exact strategy, anti-patterns to avoid, and phased approach.

---

## 🚨 MANDATORY WORKFLOW RULES - NO EXCEPTIONS

**These rules are SEVERE and MUST be followed without deviation:**

### 1. Task Tracking in plano_migracao.md
- **ALWAYS** mark tasks as completed in `plano_migracao.md` after finishing them
- Add `✅` or `[x]` to completed checklist items
- Update phase status as you progress
- Keep the migration plan synchronized with actual progress

### 2. Frontend Testing with Browser Automation
- **ALWAYS** use MCP Claude-in-Chrome or Puppeteer after ANY frontend change
- Test the actual UI changes in a real browser
- Capture screenshots of changes
- Verify responsive behavior (desktop + mobile)
- Document any visual issues found

### 3. Backend Sprint Testing Protocol
After completing ANY backend sprint/implementation:

**MANDATORY STEPS:**
1. **Test ALL endpoints** using Postman, curl, or MCP tools
2. **Verify status codes** (200, 201, 400, 401, 403, 404, 500, etc.)
3. **Validate response payloads** (structure, data types, required fields)
4. **Test error scenarios** (invalid input, missing auth, etc.)
5. **Create a testing guide** (`.MD` file) with:
   - Endpoint URL and method
   - Required headers/body
   - Expected status codes
   - Sample requests/responses
   - Common error scenarios

**Example:** After implementing Auth endpoints, create `docs/testing/AuthEndpoints_TestGuide.md`

### 4. Running .NET Projects
- **DO NOT** use Docker for development
- **ALWAYS** use standard .NET CLI commands:
  ```bash
  dotnet clean
  dotnet build
  dotnet run
  ```
- Docker is ONLY for production deployment

### 5. Design Decisions
- **NEVER** invent or assume design choices
- **ALWAYS** consult the `designer` agent for:
  - UI layout decisions
  - Color schemes
  - Component styling
  - User experience flows
  - Responsive design patterns
- Wait for design approval before implementing frontend features

### 6. Zero Tolerance for Errors
- All instructions in this file and `plano_migracao.md` are **SEVERE**
- **NO improvisation** - follow the documented approach
- **NO shortcuts** - complete all verification steps
- **NO assumptions** - ask questions if unclear
- Quality over speed - do it right the first time

---

## Development Commands

### Build & Run
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run in development mode
dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj

# Run in production mode
dotnet publish -c Release -o ./publish
cd publish && dotnet HeimdallWeb.dll
```

### Database Migrations
```bash
# Apply migrations to MySQL database
dotnet ef database update --project HeimdallWebOld

# Create new migration
dotnet ef migrations add MigrationName --project HeimdallWebOld

# View migration SQL
dotnet ef migrations script --project HeimdallWebOld

# Rollback to specific migration
dotnet ef database update MigrationName --project HeimdallWebOld
```

### Frontend Assets
```bash
# Install npm dependencies (TypeScript compilation)
cd HeimdallWebOld
npm install

# Compile TypeScript files
npx tsc

# Watch mode for TypeScript
npx tsc --watch
```

### Docker
**⚠️ DO NOT USE DOCKER FOR DEVELOPMENT**

Docker is configured for production deployment only. During development and migration phases, always use:
```bash
dotnet clean
dotnet build
dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj
```

Docker usage is reserved for final production deployment after migration is complete.

---

## Architecture

### Current MVC Architecture (HeimdallWebOld)

The existing codebase follows a layered architecture with clear separation of concerns:

**Controllers** → Handle HTTP requests, route to services
- `AdminController` - Admin dashboard and user management
- `HomeController` - Main scan interface
- `HistoryController` - Scan history and results
- `LoginController` - Authentication
- `UserController` - User profile and statistics

**Services** → Business logic and orchestration
- `ScanService` - Coordinates all scanners, primary orchestration logic (~266 LOC)
- `GeminiService` - AI analysis integration with Google Gemini API
- `PdfService` - PDF report generation
- `ImageService` - User profile image handling

**Repositories** → Data access layer (Repository Pattern)
- All implement interface contracts from `Interfaces/`
- Use EF Core for database operations
- `DashboardRepository` uses `MemoryCache` for performance optimization

**Scanners** → Specialized security scanners (implement `IScanner`)
- `HeaderScanner` - HTTP security headers
- `SslScanner` - SSL/TLS certificate validation
- `PortScanner` - Port scanning and service detection
- `HttpRedirectScanner` - HTTP to HTTPS redirect validation
- `RobotsScanner` - robots.txt analysis
- `SensitivePathsScanner` - Sensitive file/directory detection
- `ScannerManager` - Scanner coordination

**Models** → Domain entities (EF Core entities)
- Map directly to database tables with EF Core configurations
- Located in `Models/` with relationship mappings in `Models/Map/`

**Critical Database Views:**
- 14 SQL VIEWs for optimized dashboard queries
- Located in `SQL/` directory
- Mapped in EF Core via `ToView()` calls in `AppDbContext`
- **IMPORTANT:** These views are MySQL-specific and require manual conversion to PostgreSQL syntax during migration

### Key Architectural Patterns

**Dependency Injection:**
- Configured in `Extensions/ServiceCollectionExtensions.cs`
- All services, repositories registered as `Scoped` lifetime
- DbContext uses connection pooling (`AddDbContextPool`)

**Extension Methods:**
- `HostingExtensions.AddHeimdallServices()` - Service registration
- `HostingExtensions.UseHeimdallPipeline()` - Middleware pipeline
- Centralizes configuration, follows ASP.NET Core conventions

**Rate Limiting:**
- Global: 85 requests/minute per IP
- Scan-specific: 4 requests/minute per IP (named policy "ScanPolicy")
- Configured in `HostingExtensions.cs`

**JWT Authentication:**
- Token stored in HttpOnly cookie (`authHeimdallCookie`)
- Custom event handlers for redirection on auth failure
- Configuration in `Options/JwtOptions.cs`

**Caching Strategy:**
- Dashboard queries use `MemoryCache` with 5-minute expiration
- Implemented in `DashboardRepository`

---

## Migration Strategy (From plano_migracao.md)

### Phase-Based Approach

**Phase 1: Domain Layer (4-6h)**
- Extract domain entities from current Models
- Create value objects for ScanTarget, EmailAddress, ScanDuration
- Define repository interfaces
- Zero infrastructure dependencies

**Phase 2: Infrastructure Layer (10h)**
- Migrate MySQL → PostgreSQL
- Convert 14 SQL VIEWs to PostgreSQL syntax (CRITICAL STEP)
- Copy and adapt existing repositories
- Maintain scanners unchanged
- UnitOfWork for transaction management

**Phase 3: Application Layer (6-8h)**
- Create command/query handlers from ScanService logic
- FluentValidation for all requests
- AutoMapper for DTO mappings

**Phase 4: WebAPI - Minimal APIs (4-6h)**
- Replace MVC controllers with Minimal API endpoints
- Maintain JWT auth, rate limiting, CORS
- OpenAPI/Swagger documentation

**Phase 5: Frontend - Next.js (35-40h - THE BOTTLENECK)**
- Next.js 15 + React 19 + TailwindCSS + shadcn/ui
- 11 pages to implement from scratch
- This phase requires the most user validation time

**Phase 6: Testing & Validation (10h)**
- Unit tests, integration tests, E2E testing
- Manual validation of all critical flows

### Anti-Patterns to Avoid (from plano_migracao.md)

❌ **DO NOT:**
1. Create microservices (monolith is sufficient for 20K LOC)
2. Use event sourcing (CQRS Light is enough)
3. Implement full DDD (avoid VOs for primitives)
4. Create generic repositories (use specific interfaces)
5. Use EF Core for dashboard queries (keep SQL VIEWs)
6. Drop MySQL immediately (dual database phase is critical)
7. Skip database indexes (JSONB needs GIN index)
8. Auto-generate migrations (review each one)
9. Migrate views with EF (create manually in SQL)
10. Use Pages Router in Next.js (App Router is the future)
11. Store JWT in localStorage (HttpOnly cookies only)
12. Skip integration tests (unit tests aren't enough)

---

## Critical Files & Their Roles

### Core Business Logic
- **`Services/ScanService.cs`** (266 lines)
  - Primary orchestration logic for security scans
  - Coordinates all scanners, manages scan sessions
  - **MUST DECOMPOSE INTO:** `ExecuteScanCommandHandler`, `ScannerService`, `ScanSession` aggregate in Application layer

### Database Schema
- **`Data/AppDbContext.cs`**
  - EF Core DbContext with entity configurations
  - Defines all 14 SQL VIEWs mappings
  - MySQL-specific (requires PostgreSQL conversion)

### Security Scanners
- All in `Scanners/` directory
- **COPY UNCHANGED** to Infrastructure layer during migration
- Already implement `IScanner` interface
- Timeout handling built-in (3-8s per scanner)

### Configuration
- **`Extensions/HostingExtensions.cs`**
  - Template for Minimal APIs `Program.cs`
  - JWT auth, rate limiting, middleware pipeline
  - Reference this when building Phase 4 WebAPI

### SQL Views
- **`SQL/*.sql`** (14 files)
  - Dashboard statistics queries
  - **HIGH RISK AREA:** MySQL syntax incompatible with PostgreSQL
  - Must be manually converted and tested in Phase 2

### Authentication & Authorization
- **`Helpers/TokenService.cs`** - JWT token generation
- **`Helpers/CookiesHelper.cs`** - Secure cookie management
- **`Options/JwtOptions.cs`** - JWT configuration binding

---

## Database Schema

### Core Tables (7)

1. **`tb_user`** - User accounts
   - `user_id`, `username`, `email`, `password` (hashed), `user_type`, `is_active`, `profile_image`
   - User type: 1=Regular, 2=Admin

2. **`tb_history`** - Scan records
   - `history_id`, `target`, `raw_json_result`, `created_date`, `user_id`, `duration`, `has_completed`, `summary`
   - FK to `tb_user`

3. **`tb_finding`** - Vulnerabilities found
   - `finding_id`, `type`, `description`, `severity`, `evidence`, `recommendation`, `history_id`
   - FK to `tb_history`

4. **`tb_technology`** - Technologies detected
   - `technology_id`, `technology_name`, `version`, `category`, `description`, `history_id`
   - FK to `tb_history`

5. **`tb_log`** - Structured logging
   - `log_id`, `timestamp`, `level`, `source`, `message`, `details`, `user_id`, `history_id`, `remote_ip`, `code`
   - Enum-based event codes (`LogEventCode`)

6. **`tb_ia_summary`** - AI analysis results
   - `ia_summary_id`, `main_category`, `overall_risk`, `summary_text`, `findings_*`, `history_id`
   - Generated by Google Gemini API

7. **`tb_user_usage`** - Rate limiting/quota tracking
   - `user_usage_id`, `date`, `request_counts`, `user_id`

### SQL Views (14)
All views are for optimized dashboard queries. Located in `SQL/` directory:
- User statistics, scan statistics, logs overview
- Recent activity, scan trends, registration trends
- Risk distribution, category breakdowns
- **CRITICAL:** All use MySQL-specific syntax and must be manually converted to PostgreSQL

---

## Testing Strategy

### 🚨 MANDATORY: Backend Endpoint Testing After Each Sprint

After implementing ANY backend endpoints, you MUST:

1. **Test every endpoint** individually
2. **Document results** in a testing guide (`.MD` file in `docs/testing/`)
3. **Verify all scenarios** (success, validation errors, auth errors, not found, etc.)

**Testing Guide Template:**
```markdown
# [Feature] API Testing Guide

## Endpoints Implemented
- POST /api/v1/endpoint-name
- GET /api/v1/endpoint-name/{id}

## Test Cases

### 1. POST /api/v1/endpoint-name
**Request:**
- Method: POST
- URL: http://localhost:5000/api/v1/endpoint-name
- Headers: Content-Type: application/json
- Body:
  ```json
  {
    "field": "value"
  }
  ```

**Expected Response:**
- Status: 201 Created
- Body:
  ```json
  {
    "id": 1,
    "field": "value",
    "createdAt": "2024-01-01T00:00:00Z"
  }
  ```

**Error Scenarios:**
- Invalid input: 400 Bad Request
- Unauthorized: 401 Unauthorized
- Duplicate: 409 Conflict

### 2. GET /api/v1/endpoint-name/{id}
[... repeat for each endpoint]
```

### Frontend Testing with Browser Automation

**MANDATORY after ANY frontend change:**

1. **Use MCP Claude-in-Chrome:**
   ```javascript
   // Navigate to the changed page
   // Take screenshots before/after
   // Test interactions (clicks, forms, navigation)
   // Verify responsive behavior
   ```

2. **Or use Puppeteer:**
   ```bash
   # Run automated browser tests
   # Capture screenshots
   # Test user flows
   ```

   **⚙️ Puppeteer configuration (verified working):**
   ```javascript
   // Puppeteer module path (global install via @modelcontextprotocol/server-puppeteer)
   const puppeteer = require('/home/alex/.npm-global/lib/node_modules/@modelcontextprotocol/server-puppeteer/node_modules/puppeteer');

   // Chrome binary (latest verified version)
   const CHROME_PATH = `${process.env.HOME}/.cache/puppeteer/chrome/linux-145.0.7632.46/chrome-linux64/chrome`;

   // Required launch args for headless Linux
   const browser = await puppeteer.launch({
     executablePath: CHROME_PATH,
     args: ['--no-sandbox', '--disable-setuid-sandbox'],
     headless: true
   });
   ```

   **⚠️ Login form fields:**
   - Email input: `input[name="emailOrLogin"]`  ← NOT `input[type="email"]`
   - Password input: `input[name="password"]`
   - Test credentials (admin): `alexandrescarano@gmail.com` / `Admin@123`

   **⚠️ Dark mode toggle:**
   ```javascript
   // Enable dark mode (next-themes reads from localStorage)
   await page.evaluate(() => localStorage.setItem('theme', 'dark'));
   await page.reload({ waitUntil: 'networkidle2' });

   // Disable dark mode
   await page.evaluate(() => localStorage.removeItem('theme'));
   await page.reload({ waitUntil: 'networkidle2' });
   ```

   **⚠️ Wait for hydration:** Always add `await new Promise(r => setTimeout(r, 2000-3000))` after navigation — Next.js needs time to hydrate before inputs are interactive.

3. **Document findings** in the testing guide

### Manual Testing Workflow (MVC - Legacy)
1. **Scan Flow:**
   - Navigate to Home page
   - Enter target URL (e.g., `https://example.com`)
   - Submit scan form
   - Verify loading state, timeout handling (75s max)
   - Check scan results display correctly

2. **Authentication:**
   - Register new user
   - Login with credentials
   - Verify JWT cookie is set (`authHeimdallCookie`)
   - Test logout clears cookie
   - Test protected routes redirect to login

3. **Dashboard:**
   - Login as admin (user_type = 2)
   - Verify all dashboard metrics load
   - Check SQL VIEWs return correct data
   - Validate charts render

4. **Rate Limiting:**
   - Make 85+ requests in 1 minute (should throttle)
   - Make 4+ scan requests in 1 minute (should redirect with `?rateLimited=1`)

### Migration Testing
**Phase 2 Critical:** After PostgreSQL migration, manually test all 14 SQL VIEWs:
```sql
-- Test each view individually
SELECT * FROM vw_dashboard_user_stats;
SELECT * FROM vw_dashboard_scan_stats;
-- ... repeat for all 14 views
```

Compare results with MySQL version to ensure data integrity.

---

## Common Pitfalls

### During Migration

1. **SQL VIEW Conversion:**
   - MySQL's `DATE_SUB()` → PostgreSQL's `INTERVAL`
   - MySQL's `NOW()` → PostgreSQL's `NOW()` or `CURRENT_TIMESTAMP`
   - MySQL's `IF()` → PostgreSQL's `CASE WHEN`
   - Test EVERY view individually

2. **JSONB vs JSON:**
   - PostgreSQL requires explicit JSONB casting
   - Add GIN index on `raw_json_result` column
   - Update queries to use JSONB operators (`->`, `->>`)

3. **Connection String:**
   - MySQL: `Server=localhost;Database=heimdallweb;User=user;Password=pass;`
   - PostgreSQL: `Host=localhost;Database=heimdallweb;Username=user;Password=pass;`

4. **EF Core Provider:**
   - MySQL: `Pomelo.EntityFrameworkCore.MySql`
   - PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL`

### Existing Code Issues

- **Hard-coded timeouts:** Scanners use various timeout values (3s-8s), consider making configurable
- **No retry logic:** External API calls (Gemini) lack retry policies
- **Limited error handling:** Some scanners don't catch all exceptions
- **No circuit breaker:** Gemini API integration could benefit from Polly

---

## External Dependencies

### APIs
- **Google Gemini AI API:**
  - Requires `GEMINI_API_KEY` in appsettings
  - Used for vulnerability analysis and risk classification
  - Rate limits apply (monitor usage)

### NuGet Packages
- `Microsoft.EntityFrameworkCore` (9.0.10)
- `Pomelo.EntityFrameworkCore.MySql` (9.0.0) - Will change to Npgsql
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0)
- `QuestPDF` (2025.7.4) - PDF generation
- `Newtonsoft.Json` (13.0.4)
- Custom DLL: `ASHelpers.dll` (located in `dlls/`)

### Frontend Libraries
- jQuery (legacy, to be removed in Next.js migration)
- Bootstrap 5 (to be replaced with TailwindCSS)
- Prism.js (JSON syntax highlighting)
- Chart.js (to be replaced with Recharts in Next.js)

---

## Configuration

### Required appsettings.json
```json
{
  "ConnectionStrings": {
    "AppDbConnectionString": "Server=localhost;Database=heimdallweb;User=user;Password=pass;"
  },
  "Jwt": {
    "Key": "minimum_32_character_secret_key",
    "Issuer": "HeimdallWeb",
    "Audience": "HeimdallWebUsers",
    "RequireHttpsMetadata": false
  },
  "GEMINI_API_KEY": "your_gemini_api_key_here"
}
```

### Environment Variables (Docker)
- `ConnectionStrings__AppDbConnectionString`
- `Jwt__Key`
- `GEMINI_API_KEY`
- `ASPNETCORE_ENVIRONMENT`

---

## Working with This Codebase

### 🚨 MANDATORY Pre-Work Checklist

**BEFORE starting ANY task:**

1. **Open `plano_migracao.md`**
   - Read the relevant phase section
   - Understand anti-patterns to avoid
   - Check dependencies between phases

2. **Identify the task** in the migration plan
   - Find the specific checklist item
   - Understand acceptance criteria
   - Note any critical files mentioned

3. **Check for reusable code**
   - Existing repositories can be copied/adapted
   - Scanners should be copied unchanged
   - Services may need decomposition

4. **Plan your approach**
   - Don't over-engineer (DDD Light, not full DDD)
   - Follow existing patterns
   - Ask questions if unclear

### 🚨 MANDATORY Post-Work Checklist

**AFTER completing ANY task:**

1. **Mark task as completed** in `plano_migracao.md`
   ```markdown
   - [x] Task description
   or
   - ✅ Task description
   ```

2. **Test your changes** (based on task type):
   - **Backend:** Test all endpoints, create testing guide
   - **Frontend:** Use browser automation, capture screenshots
   - **Database:** Run migrations, test queries

3. **Verify compilation:**
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

4. **Document any issues** or deviations from the plan

5. **Commit with descriptive message:**
   ```bash
   git add .
   git commit -m "Phase X: Completed [Task Name] - [Brief description]"
   ```

### When Adding Features
- Maintain consistency with existing patterns (Repository + Service layers)
- Use dependency injection for all services
- Follow JWT auth pattern for protected endpoints
- Apply rate limiting where appropriate

### Design Decisions - CRITICAL PROCESS

**🚨 ABSOLUTE RULE: NEVER improvise or invent design without consulting the designer agent.**

**When to consult the designer agent:**
- Before implementing ANY frontend component
- Before choosing colors, fonts, spacing
- Before deciding layout structure
- Before implementing responsive behavior
- When unsure about UI/UX patterns

**How to consult the designer agent:**
```bash
# Use the Task tool with subagent_type="designer"
Task(
  subagent_type="designer",
  description="Design login page layout",
  prompt="I need to design the login page for HeimdallWeb.
         Requirements: Email/password fields, remember me checkbox,
         forgot password link, register link.
         Should follow modern security application aesthetics.
         Please provide layout structure, color scheme, and component recommendations."
)
```

**After receiving design:**
1. Review the designer's recommendations
2. Confirm with user if needed
3. Implement EXACTLY as designed
4. Document design decisions in code comments
5. Test with browser automation

**What the designer agent provides:**
- Component layout and structure
- Color schemes and typography
- Spacing and sizing guidelines
- Responsive behavior patterns
- Accessibility considerations

**Example comment in code:**
```typescript
// Design Decision (Designer Agent - 2024-01-15):
// Login form uses centered card layout with max-width 400px
// Primary color: #1a73e8 (trust blue)
// Error states: #d32f2f (material red)
// Responsive: full-width on mobile (<768px)
```

### Code Style
- Use `async/await` for all I/O operations
- Implement proper error handling with try-catch
- Log important events using structured logging
- Use DTOs for data transfer between layers
- Keep controllers thin, move logic to services

---

## 📋 Complete Workflow Example

**Scenario:** Implementing Phase 4 - Authentication Endpoints

### Step 1: Pre-Work (MANDATORY)
```bash
# 1. Read the migration plan
cat plano_migracao.md | grep -A 50 "Fase 4"

# 2. Identify the task
# Task: Create POST /api/v1/auth/login and POST /api/v1/auth/register

# 3. Check for reusable code
# - TokenService already exists in Helpers/
# - LoginDTO exists in DTO/
# - UserRepository has GetByEmail method
```

### Step 2: Implementation
```bash
# Build and verify starting state
dotnet clean
dotnet build
dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj

# Implement the endpoints...
# (code implementation happens here)
```

### Step 3: Backend Testing (MANDATORY)
```bash
# Test POST /api/v1/auth/login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!@#"}'

# Expected: 200 OK with JWT token

# Test error scenarios
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"invalid","password":"wrong"}'

# Expected: 401 Unauthorized

# Create testing guide
echo "# Auth Endpoints Testing Guide" > docs/testing/AuthEndpoints_TestGuide.md
# (document all test cases)
```

### Step 4: Verification (MANDATORY)
```bash
# Compile check
dotnet clean
dotnet build

# Run and verify
dotnet run

# Check logs for errors
tail -f logs/app.log
```

### Step 5: Mark Task Complete (MANDATORY)
```markdown
# Edit plano_migracao.md

**Fase 4: WebApi - Minimal APIs (2-3 dias = 4-6h)**

Eu gero (85% automatizado):
- [x] AuthenticationEndpoints (login, register) ✅ Completed 2024-01-15
- [ ] ScanEndpoints (POST scan, GET scans)
- [ ] HistoryEndpoints (GET list, GET by id, export PDF)
```

### Step 6: Commit (MANDATORY)
```bash
git add .
git commit -m "Phase 4: Implemented Authentication endpoints (login/register)

- Created POST /api/v1/auth/login endpoint
- Created POST /api/v1/auth/register endpoint
- Added JWT token generation
- Tested all success/error scenarios
- Created testing guide at docs/testing/AuthEndpoints_TestGuide.md
- Status codes verified: 200, 400, 401, 409

Refs: plano_migracao.md Phase 4"
```

### For Frontend Tasks - Additional Steps

**Step 3b: Design Consultation (MANDATORY)**
```typescript
// Before implementing login page

// 1. Consult designer agent
Task(
  subagent_type="designer",
  description="Design login page",
  prompt="Design the login page for HeimdallWeb security scanner..."
)

// 2. Wait for design recommendations
// 3. Implement EXACTLY as designed
```

**Step 3c: Implementation with nexus-next-js Agent (MANDATORY for Next.js frontend)**
```typescript
// After receiving the design from the designer agent

// 1. Delegate implementation to nexus-next-js agent
Task(
  subagent_type="nexus-next-js",
  description="Implement login page",
  prompt="Implement the login page for HeimdallWeb based on the design: [design details here]..."
)

// 2. Agent implements the component/page
// 3. Proceed to browser verification (Step 3d)
```

**Step 3d: Browser Testing with MCP (MANDATORY)**
```javascript
// Use MCP Claude-in-Chrome IMMEDIATELY after any frontend change
// This step is REQUIRED even if the code "looks correct"

// 1. Get tab context
mcp__claude-in-chrome__tabs_context_mcp()

// 2. Navigate to the changed page
mcp__claude-in-chrome__navigate({ url: "http://localhost:3000/login" })

// 3. Take desktop screenshot
mcp__claude-in-chrome__computer({ action: "screenshot" })

// 4. Test interactions (fill form, click buttons, etc.)
mcp__claude-in-chrome__find({ query: "login form fields" })

// 5. Check responsive (mobile viewport)
mcp__claude-in-chrome__resize_window({ width: 375, height: 812 })
mcp__claude-in-chrome__computer({ action: "screenshot" })

// 6. Read console for errors
mcp__claude-in-chrome__read_console_messages({ pattern: "error|warn" })

// 7. Document any visual issues found
```

---

## 🚨 MANDATORY Verification Checklist

After making ANY changes, you MUST verify ALL applicable items:

### Code Quality
✅ **Code compiles without warnings:** `dotnet clean && dotnet build`
✅ **No compiler errors or warnings**
✅ **All using statements resolved**
✅ **No unused variables or methods**

### Database
✅ **Migrations apply successfully:** `dotnet ef database update`
✅ **Database schema is correct:** Verify tables/columns
✅ **SQL VIEWs work (if applicable):** Test each view individually
✅ **No SQL errors in logs**

### Application Runtime
✅ **Application starts:** `dotnet run`
✅ **No runtime exceptions during startup**
✅ **All dependencies injected correctly**
✅ **Configuration loaded properly**

### Backend Endpoints (if implemented/modified)
✅ **All endpoints tested with correct status codes**
✅ **Response payloads validated**
✅ **Error scenarios tested (400, 401, 404, 500)**
✅ **Testing guide created in `docs/testing/`**

### Frontend (if implemented/modified)
✅ **Browser automation tests run:** MCP Chrome or Puppeteer
✅ **Screenshots captured of changes**
✅ **Responsive behavior verified (mobile + desktop)**
✅ **No console errors in browser**

### Authentication & Security
✅ **JWT authentication works:** Login/logout flow
✅ **Protected routes require auth**
✅ **Rate limiting active:** Test throttling behavior
✅ **Cookies set correctly (HttpOnly, Secure, SameSite)**

### Migration Plan
✅ **Task marked as completed** in `plano_migracao.md`
✅ **Phase checklist updated**
✅ **Any deviations documented**

### Commit & Documentation
✅ **Changes committed with descriptive message**
✅ **Testing guide created (if backend sprint)**
✅ **Design decisions documented (if frontend)**
✅ **README or docs updated (if needed)**

---

**🚨 CRITICAL REMINDER:**
- **ALL rules in this file are MANDATORY**
- **NO exceptions or shortcuts**
- **Follow `plano_migracao.md` for migration-specific tasks**
- **Ask questions if anything is unclear**
- **Quality and correctness over speed**

---

## 🚨 Rule Violation Handling

**If you find yourself about to violate any of these rules, STOP immediately:**

### Violation: Not marking tasks in plano_migracao.md
**❌ WRONG:** Complete task and move to next one
**✅ CORRECT:** Edit `plano_migracao.md`, mark task with `[x]` or `✅`, then proceed

### Violation: Skipping endpoint testing
**❌ WRONG:** "Endpoints look good, moving on"
**✅ CORRECT:** Test EVERY endpoint, document results, create testing guide

### Violation: Not using browser automation
**❌ WRONG:** "I can see the code is correct"
**✅ CORRECT:** Use MCP Chrome/Puppeteer, take screenshots, verify visually

### Violation: Using Docker for development
**❌ WRONG:** `docker-compose up`
**✅ CORRECT:** `dotnet clean && dotnet build && dotnet run`

### Violation: Inventing design
**❌ WRONG:** "I'll make the button blue and centered"
**✅ CORRECT:** Consult designer agent, get approval, implement as designed

### Violation: Skipping verification checklist
**❌ WRONG:** "Build succeeded, task is done"
**✅ CORRECT:** Complete ENTIRE verification checklist before marking complete

---

## 📞 When In Doubt

**If you are uncertain about ANY of these rules:**

1. **STOP** - Do not proceed with assumptions
2. **Ask the user** - Use clear, specific questions
3. **Reference this file** - Point to the specific rule
4. **Wait for clarification** - Do not improvise

**Example questions:**
- "I need to implement the login page. Should I consult the designer agent first for the UI design, as per CLAUDE.md Section 'Design Decisions'?"
- "I completed the authentication endpoints. Before moving on, I need to create a testing guide. Should I create it at `docs/testing/AuthEndpoints_TestGuide.md`?"
- "I'm unsure if this task counts as 'frontend change'. Should I run browser automation tests?"

---

## 🎯 Success Criteria

**You are following these rules correctly when:**

✅ `plano_migracao.md` is always up-to-date with completed tasks
✅ `docs/testing/` directory contains testing guides for all backend sprints
✅ Frontend changes are always verified with browser screenshots
✅ Designer agent is consulted before ANY UI implementation
✅ Docker is NEVER used during development
✅ Every commit message references the phase and task from `plano_migracao.md`
✅ Verification checklist is completed before marking any task done

---

**Remember: These rules exist to ensure quality, prevent rework, and maintain consistency throughout the migration. They are SEVERE and NON-NEGOTIABLE. Following them strictly will result in a successful migration with minimal issues.**
