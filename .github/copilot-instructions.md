Repository: HeimdallWeb — Copilot instructions

Purpose
- Short, repository-specific guidance for Copilot sessions: build/test/run commands, high-level architecture, and important conventions.

Quick commands
- Restore dependencies: dotnet restore
- Build full solution: dotnet build HeimdallWeb.sln
- Build a single project: dotnet build src/HeimdallWeb.Application/HeimdallWeb.Application.csproj
- Run legacy MVC app (development): dotnet run --project HeimdallWebOld/HeimdallWebOld.csproj
- Run WebAPI (development): dotnet run --project src/HeimdallWeb.WebApi/
- Run all tests: dotnet test
- Run a single test (xUnit): dotnet test tests/HeimdallWeb.Application.Tests/HeimdallWeb.Application.Tests.csproj --filter "FullyQualifiedName~Namespace.ClassName.TestMethod"
  - The --filter uses partial matches; replace with the exact FullyQualifiedName when possible.
- EF Core migrations / DB update (examples used in docs):
  - dotnet ef database update --project HeimdallWebOld
  - dotnet ef database update --project src/HeimdallWeb.Infrastructure --startup-project src/HeimdallWeb.WebApi
- TypeScript (legacy frontend under HeimdallWebOld): cd HeimdallWebOld && npm install && npx tsc

Notes on linting
- No repository-wide dotnet-format or dedicated linter tasks are configured; use dotnet format or add CI steps if desired.

High-level architecture (big picture)
- Multi-project .NET solution (HeimdallWeb.sln): common layout under src/ with projects:
  - Contracts: API/DTO contracts
  - Domain: core domain entities/value objects
  - Application: command/query handlers, orchestration (migration target)
  - Infrastructure: EF Core DbContext, repositories, data access, migrations
  - WebApi: Minimal API entry points (newer target)
- Legacy monolith: HeimdallWebOld/ is the existing ASP.NET Core MVC application that contains controllers, views, and a set of "Scanners" (security scanning logic).
- Scanners and orchestration: ScanService coordinates multiple IScanner implementations (HeaderScanner, SslScanner, PortScanner, etc.). Repositories implement data access and map to EF entities.
- Persistence: EF Core is used; the project contains 14 SQL VIEWs under SQL/ (MySQL syntax) and a DesignTimeDbContextFactory for EF CLI operations.
- Auth & infra: JWT auth issued as an HttpOnly cookie (authHeimdallCookie). Rate limiting policies are configured in hosting extensions.
- Migration goal: repository is mid-migration from the legacy MVC monolith to a DDD-light modular backend + Minimal APIs and a Next.js frontend — see plano_migracao.md and docs/ for phase details.

Key conventions (repository-specific)
- Use dotnet CLI (dotnet restore/build/run/test/ef) for development; Docker is reserved for production in this project (see CLAUDE.md/.GEMINI.md).
- Every completed task must be marked in plano_migracao.md (follow project-specific workflow).
- After implementing backend endpoints, create an endpoint testing guide in docs/testing/ and manually run tests (curl/Postman) as documented.
- Any frontend change must be validated with browser automation (MCP/Puppeteer) and documented (see CLAUDE.md).
- Preserve and migrate SQL VIEWs manually when moving to PostgreSQL — do not auto-convert without review.
- Tests use xUnit (see tests/*.csproj); test projects target net10.0.
- DesignTimeDbContextFactory exists and is required for dotnet ef commands; prefer explicit --project/--startup-project in EF commands as shown in docs.
- Custom native/third-party DLLs live in dlls/ (e.g., ASHelpers.dll) — be cautious with licensing and platform compatibility.

Important files to read before changing behavior
- README.md — project overview and common commands
- plano_migracao.md — migration plan and required steps (MANDATORY workflow rules)
- CLAUDE.md and .GEMINI.md — repository-specific assistant rules and severe constraints (must be read by assistants before making changes)
- docs/testing/ — testing guides for each migration phase
- SQL/ — POSTGRESQL VIEW definitions (critical for dashboards)

AI assistant integration notes
- There is a detailed CLAUDE.md that defines strict rules for automated assistants (task tracking, no Docker for dev, mandatory testing, design approvals). Read it and follow its rules.
- Also consult .GEMINI.md for notes relevant to automated code execution and tests.

Where to run commands
- From repository root for solution-wide commands (dotnet build/test) or from HeimdallWebOld/ for legacy frontend (npm tasks).

When in doubt
- Follow plano_migracao.md and CLAUDE.md. If a change touches UI, consult the designer agent or the docs indicated in CLAUDE.md before implementing.

References
- README.md
- plano_migracao.md
- CLAUDE.md
- docs/testing/

