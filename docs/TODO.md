# HeimdallWeb - TODO List & Roadmap

**Date Created**: 2026-02-07  
**Current Phase**: Phase 4 - WebAPI (Minimal APIs) ✅ **95% Complete**  
**Next Phase**: Phase 5 - Frontend (Next.js 15)

---

## 🎯 IMMEDIATE PRIORITIES (This Week)

### 1. ✅ **Phase 4 Completion - API Endpoints** [MOSTLY DONE]

#### 1.1 Testing & Validation
- [x] Test authentication endpoints (register, login, logout)
- [x] Test user endpoints (profile, statistics, update)
- [x] Test scan endpoints (execute, list)
- [x] Fix critical bugs (read-only collections, exception handling, logout status)
- [ ] **Test admin endpoints with admin user** 🔥 HIGH PRIORITY
  - [ ] Create admin user in database
  - [ ] Test `GET /api/v1/dashboard/admin`
  - [ ] Test `GET /api/v1/dashboard/users`
  - [ ] Test `PATCH /api/v1/admin/users/{id}/status`
  - [ ] Test `DELETE /api/v1/admin/users/{id}`
  - [ ] Document admin testing results
  
- [ ] **Test file upload endpoint** 🔥 HIGH PRIORITY
  - [ ] Test `POST /api/v1/users/{id}/profile-image`
  - [ ] Validate base64 image parsing
  - [ ] Test image size limits
  - [ ] Test invalid image formats
  - [ ] Document image upload guide

- [ ] **Test PDF export endpoints** 🔥 HIGH PRIORITY
  - [ ] Test `GET /api/v1/scan-histories/{id}/export` (single scan PDF)
  - [ ] Test `GET /api/v1/scan-histories/export` (all scans PDF)
  - [ ] Validate PDF structure
  - [ ] Test QuestPDF rendering
  - [ ] Document PDF export process

- [ ] **Test DELETE endpoints** 🔥 MEDIUM PRIORITY
  - [ ] Test `DELETE /api/v1/users/{id}` (self-delete with password)
  - [ ] Test `DELETE /api/v1/scan-histories/{id}`
  - [ ] Test authorization (users can only delete their own data)
  - [ ] Document deletion workflows

#### 1.2 API Documentation Enhancement
- [ ] **Improve Swagger documentation** 📝 MEDIUM PRIORITY
  - [ ] Add XML comments to all endpoints
  - [ ] Add request/response examples
  - [ ] Document all error scenarios
  - [ ] Add authentication requirements
  - [ ] Add rate limiting information
  - [ ] Configure Swagger groups (Auth, Users, Scans, Admin)

- [ ] **Create Postman collection** 📝 MEDIUM PRIORITY
  - [ ] Export all endpoints to Postman
  - [ ] Add environment variables (base URL, tokens)
  - [ ] Create test scenarios
  - [ ] Document collection usage

#### 1.3 Exception Handling Improvements
- [ ] **Enhance validation error responses** 🔧 LOW PRIORITY
  - [ ] Fix null errors in validation response (currently returns `null`)
  - [ ] Include field names in error messages
  - [ ] Add error codes for frontend mapping
  - [ ] Example:
    ```json
    {
      "statusCode": 400,
      "message": "Validation failed",
      "errors": {
        "Username": ["Must be at least 6 characters", "Can only contain letters and numbers"],
        "Password": ["Must contain uppercase letter", "Must contain special character"]
      }
    }
    ```

#### 1.4 Mark Phase 4 as Complete
- [ ] **Update plano_migracao.md** ✅ HIGH PRIORITY
  - [ ] Mark Phase 4 checklist items as complete
  - [ ] Add notes about fixes implemented
  - [ ] Update timeline/progress

---

## 🏗️ SHORT-TERM IMPROVEMENTS (Next 2 Weeks)

### 2. **Integration Testing** 🧪

- [ ] **Setup integration test infrastructure** 🔥 HIGH PRIORITY
  - [ ] Add `WebApplicationFactory<Program>` setup
  - [ ] Configure in-memory database or TestContainers
  - [ ] Setup test authentication/authorization
  - [ ] Create base integration test class

- [ ] **Write integration tests for critical flows** 🔥 HIGH PRIORITY
  - [ ] Registration → Login → Get Profile flow
  - [ ] Login → Execute Scan → Get Results flow
  - [ ] Admin user management flow
  - [ ] PDF export flow
  - [ ] Rate limiting tests
  - [ ] Authentication/authorization tests

- [ ] **Add test coverage reporting** 📊 MEDIUM PRIORITY
  - [ ] Configure Coverlet for code coverage
  - [ ] Setup coverage reports in CI/CD
  - [ ] Set coverage thresholds (target: 80%+)

### 3. **API Enhancements** ✨

- [ ] **Add health check endpoint** 🔥 MEDIUM PRIORITY
  - [ ] `GET /health` → Check API status
  - [ ] `GET /health/ready` → Check dependencies (DB, external APIs)
  - [ ] Include database connectivity check
  - [ ] Include Gemini API availability check
  - [ ] Return proper status codes (200, 503)

- [ ] **Add API versioning** 📝 MEDIUM PRIORITY
  - [ ] Configure URL versioning (`/api/v1/`, `/api/v2/`)
  - [ ] Or header versioning (`Accept: application/vnd.heimdall.v1+json`)
  - [ ] Prepare for future breaking changes

- [ ] **Improve rate limiting feedback** 🔧 LOW PRIORITY
  - [ ] Add `X-RateLimit-Limit` header
  - [ ] Add `X-RateLimit-Remaining` header
  - [ ] Add `X-RateLimit-Reset` header
  - [ ] Add `Retry-After` header when rate limited

- [ ] **Add request correlation IDs** 🔧 LOW PRIORITY
  - [ ] Generate unique ID per request
  - [ ] Include in response headers (`X-Correlation-ID`)
  - [ ] Include in all logs for tracing
  - [ ] Example: `X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000`

### 4. **Performance Optimization** ⚡

- [ ] **Add response caching** 🔥 MEDIUM PRIORITY
  - [ ] Cache dashboard statistics (5 minutes)
  - [ ] Cache user statistics (2 minutes)
  - [ ] Use `ResponseCache` attribute or middleware
  - [ ] Configure cache headers (`Cache-Control`, `ETag`)

- [ ] **Optimize database queries** 🔥 MEDIUM PRIORITY
  - [ ] Add missing indexes (email, username, createdDate)
  - [ ] Use `.AsNoTracking()` for read-only queries
  - [ ] Review N+1 query problems
  - [ ] Add query logging to identify slow queries

- [ ] **Add compression** 🔧 LOW PRIORITY
  - [ ] Enable response compression (Gzip/Brotli)
  - [ ] Configure compression levels
  - [ ] Test with large JSON responses

---

## 🚀 PHASE 5 PREPARATION (Next 3-4 Weeks)

### 5. **Frontend Development - Next.js 15** 🎨

#### 5.1 Project Setup
- [ ] **Initialize Next.js 15 project** 🔥 HIGH PRIORITY
  - [ ] Create new Next.js app with TypeScript
  - [ ] Configure TailwindCSS
  - [ ] Setup shadcn/ui components
  - [ ] Configure folder structure (App Router)
  - [ ] Setup environment variables

- [ ] **Setup API client** 🔥 HIGH PRIORITY
  - [ ] Create Axios/Fetch wrapper
  - [ ] Configure base URL and interceptors
  - [ ] Handle JWT token storage (cookies only!)
  - [ ] Handle error responses
  - [ ] Add request/response logging (development)

- [ ] **Configure authentication** 🔥 HIGH PRIORITY
  - [ ] Implement login/register pages
  - [ ] Store JWT in HttpOnly cookies (via API)
  - [ ] Create auth context/provider
  - [ ] Implement protected routes
  - [ ] Add logout functionality

#### 5.2 Core Pages (11 pages total - from plano_migracao.md)
- [ ] **Public pages** 🔥 HIGH PRIORITY
  1. [ ] Login page (`/login`)
  2. [ ] Register page (`/register`)
  3. [ ] Landing/Home page (`/`)

- [ ] **Protected user pages** 🔥 HIGH PRIORITY
  4. [ ] Dashboard/Home (após login) (`/dashboard`)
  5. [ ] New Scan page (`/scan/new`)
  6. [ ] Scan History list (`/history`)
  7. [ ] Scan Detail page (`/history/[id]`)
  8. [ ] User Profile page (`/profile`)

- [ ] **Admin pages** 🔥 MEDIUM PRIORITY
  9. [ ] Admin Dashboard (`/admin/dashboard`)
  10. [ ] User Management (`/admin/users`)
  11. [ ] Logs Viewer (`/admin/logs`)

#### 5.3 UI Components
- [ ] **Design system setup** 🎨 HIGH PRIORITY
  - [ ] **MUST consult designer agent before starting!** 🚨
  - [ ] Define color palette
  - [ ] Define typography
  - [ ] Define spacing/sizing system
  - [ ] Create component library documentation

- [ ] **Core components** 🎨 HIGH PRIORITY
  - [ ] Button variants (primary, secondary, danger)
  - [ ] Input fields (text, email, password)
  - [ ] Form components
  - [ ] Card/Panel components
  - [ ] Table/DataGrid components
  - [ ] Modal/Dialog components
  - [ ] Toast/Notification components
  - [ ] Loading states/Spinners
  - [ ] Error states

- [ ] **Domain-specific components** 🎨 MEDIUM PRIORITY
  - [ ] ScanResultCard
  - [ ] FindingsList
  - [ ] TechnologyBadge
  - [ ] RiskIndicator
  - [ ] ScanStatusBadge
  - [ ] UserAvatar
  - [ ] StatisticsCard

#### 5.4 State Management
- [ ] **Choose state solution** 🔧 MEDIUM PRIORITY
  - [ ] React Context (simpler, built-in)
  - [ ] Zustand (lightweight, recommended)
  - [ ] Redux Toolkit (if complex state needed)
  - [ ] TanStack Query (for server state - RECOMMENDED)

- [ ] **Implement state management** 🔧 MEDIUM PRIORITY
  - [ ] Global auth state
  - [ ] User profile state
  - [ ] Scan history state
  - [ ] Admin data state

#### 5.5 Testing & Validation
- [ ] **Browser automation testing** 🧪 HIGH PRIORITY (MANDATORY per CLAUDE.md)
  - [ ] Use MCP Claude-in-Chrome or Puppeteer
  - [ ] Test all user flows
  - [ ] Capture screenshots
  - [ ] Test responsive design (mobile + desktop)
  - [ ] Document visual changes

---

## 🔒 SECURITY & COMPLIANCE (Ongoing)

### 6. **Security Hardening** 🛡️

- [ ] **Review security headers** 🔥 HIGH PRIORITY
  - [ ] Add `X-Content-Type-Options: nosniff`
  - [ ] Add `X-Frame-Options: DENY`
  - [ ] Add `X-XSS-Protection: 1; mode=block`
  - [ ] Configure CSP (Content Security Policy)
  - [ ] Add `Strict-Transport-Security` (HSTS)

- [ ] **Audit JWT implementation** 🔥 HIGH PRIORITY
  - [ ] Review token expiration (currently 24h)
  - [ ] Implement refresh tokens (optional)
  - [ ] Add token revocation (optional)
  - [ ] Test token tampering scenarios

- [ ] **Review rate limiting** 🔥 MEDIUM PRIORITY
  - [ ] Test bypass attempts
  - [ ] Test distributed rate limiting (if needed)
  - [ ] Review limits (85/min global, 4/min scan)

- [ ] **Input validation audit** 🔧 MEDIUM PRIORITY
  - [ ] Review all FluentValidation rules
  - [ ] Test SQL injection attempts
  - [ ] Test XSS attempts
  - [ ] Test path traversal attempts

---

## 📊 OBSERVABILITY & MONITORING (Nice to Have)

### 7. **Logging & Monitoring** 📈

- [ ] **Enhance structured logging** 🔧 MEDIUM PRIORITY
  - [ ] Add Serilog sinks (file, console, database)
  - [ ] Configure log levels per environment
  - [ ] Add correlation IDs to all logs
  - [ ] Include user context in logs

- [ ] **Add application metrics** 📊 LOW PRIORITY
  - [ ] Request duration metrics
  - [ ] Error rate metrics
  - [ ] Active users metrics
  - [ ] Scan completion metrics

- [ ] **Setup monitoring dashboards** 📊 LOW PRIORITY
  - [ ] Grafana dashboards (if using Prometheus)
  - [ ] Azure Application Insights (if Azure)
  - [ ] CloudWatch (if AWS)

---

## 🚀 DEPLOYMENT & CI/CD (Future)

### 8. **Deployment Preparation** 🚢

- [ ] **Dockerization** 🐳 LOW PRIORITY
  - [ ] Create optimized Dockerfile (multi-stage build)
  - [ ] Create docker-compose.yml (API + DB + Frontend)
  - [ ] Test container builds
  - [ ] Optimize image size

- [ ] **CI/CD Pipeline** 🔄 LOW PRIORITY
  - [ ] Setup GitHub Actions or Azure DevOps
  - [ ] Automate tests on PR
  - [ ] Automate builds on merge
  - [ ] Automate deployments (staging/production)

- [ ] **Environment configuration** ⚙️ LOW PRIORITY
  - [ ] Development environment
  - [ ] Staging environment
  - [ ] Production environment
  - [ ] Configure secrets management

---

## 📝 DOCUMENTATION (Ongoing)

### 9. **Documentation Tasks** 📚

- [ ] **API Documentation** 📖 MEDIUM PRIORITY
  - [x] Phase 4 Testing Guide ✅
  - [x] Critical Fixes Report ✅
  - [ ] Admin Endpoints Guide
  - [ ] PDF Export Guide
  - [ ] Image Upload Guide
  - [ ] Error Handling Guide

- [ ] **Architecture Documentation** 🏛️ LOW PRIORITY
  - [ ] System architecture diagram
  - [ ] Database schema diagram
  - [ ] API flow diagrams
  - [ ] Deployment architecture

- [ ] **Developer Onboarding** 👨‍💻 LOW PRIORITY
  - [ ] Setup guide (local development)
  - [ ] Contribution guidelines
  - [ ] Code review checklist
  - [ ] Testing guidelines

---

## 🎯 SUCCESS METRICS

### Phase 4 (Current)
- [x] All authentication endpoints working ✅
- [x] All user endpoints working ✅
- [x] All scan endpoints working ✅
- [x] Critical bugs fixed ✅
- [ ] All admin endpoints tested (80%)
- [ ] Integration tests written (0%)
- [ ] API documentation complete (60%)

### Phase 5 (Next)
- [ ] 11 pages implemented (0/11)
- [ ] All pages tested with browser automation (0%)
- [ ] Responsive design verified (0%)
- [ ] Design system approved (0%)

### Overall Project Health
- Code Coverage: TBD (target: 80%+)
- Build Status: ✅ Passing
- API Status: ✅ 100% endpoints working
- Known Bugs: 0 critical, 0 high, 0 medium

---

## 🔄 WORKFLOW REMINDERS (from CLAUDE.md)

### ⚠️ MANDATORY RULES - DO NOT SKIP

1. **✅ Mark completed tasks** in `plano_migracao.md` after EVERY task
2. **🌐 Use browser automation** (MCP Chrome/Puppeteer) after ANY frontend change
3. **🧪 Test ALL endpoints** after backend sprints + create testing guide (`.MD`)
4. **🚫 NO Docker** for development - use `dotnet build/run` only
5. **🎨 Consult designer agent** for ALL UI/design decisions - NEVER improvise
6. **📋 Follow the plan** - `plano_migracao.md` is the source of truth

---

## 📅 ESTIMATED TIMELINE

| Phase | Duration | Priority | Status |
|-------|----------|----------|--------|
| Phase 4 Completion | 2-3 days | 🔥 HIGH | 95% Done |
| Integration Tests | 3-5 days | 🔥 HIGH | Not Started |
| Phase 5 Setup | 2-3 days | 🔥 HIGH | Not Started |
| Phase 5 Development | 25-35 days | 🔥 HIGH | Not Started (BOTTLENECK) |
| Security Hardening | 2-3 days | 🔥 MEDIUM | Not Started |
| Deployment | 3-5 days | 🔧 LOW | Not Started |

**Total Estimated Time Remaining**: ~40-55 days

---

## 🚀 NEXT SESSION PRIORITIES

**What to do NEXT (in order)**:

1. **Test admin endpoints** (2-3 hours)
   - Create admin user in DB
   - Test all 4 admin endpoints
   - Document results

2. **Test file upload** (1-2 hours)
   - Test profile image upload
   - Validate image handling
   - Document process

3. **Test PDF export** (1-2 hours)
   - Test single and bulk PDF export
   - Validate PDF structure
   - Document process

4. **Update plano_migracao.md** (30 minutes)
   - Mark Phase 4 tasks complete
   - Add notes about fixes

5. **Start Phase 5 planning** (1 hour)
   - Consult designer agent for UI design
   - Plan Next.js project structure
   - Define component library

---

**Created**: 2026-02-07  
**Last Updated**: 2026-02-07  
**Maintained By**: dotnet-backend-expert agent  
**Review Frequency**: Weekly
