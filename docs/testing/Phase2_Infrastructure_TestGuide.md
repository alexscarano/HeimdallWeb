# Phase 2: Infrastructure Layer - Testing Guide

**Created:** 2026-02-05
**Phase:** 2 - Infrastructure Layer
**Purpose:** Manual testing guide for verifying PostgreSQL migration, repositories, and SQL VIEWs

---

## 📋 Overview

This guide provides step-by-step instructions for testing the Infrastructure Layer implementation, including:

- PostgreSQL database setup
- EF Core migrations execution
- SQL VIEWs creation and validation (14 views)
- Repository CRUD operations testing
- Performance validation (JSONB GIN index, AsNoTracking)

**Estimated Time:** 8-10 hours
**Critical Section:** SQL VIEWs testing (4 hours - highest risk)

---

## ⚠️ Prerequisites

Before starting, ensure you have:

- ✅ Phase 1 (Domain Layer) completed and tested
- ✅ PostgreSQL 14+ installed locally
- ✅ .NET 10 SDK installed
- ✅ EF Core CLI tools: `dotnet tool install --global dotnet-ef`
- ✅ Connection string configured in `appsettings.json`

---

## 🗄️ Part 1: PostgreSQL Setup (30 minutes)

### Step 1: Install PostgreSQL

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib

# Verify installation
psql --version
# Expected: psql (PostgreSQL) 14.x or higher
```

**macOS:**
```bash
brew install postgresql@14
brew services start postgresql@14
```

**Windows:**
Download from: https://www.postgresql.org/download/windows/

---

### Step 2: Create Database and User

```bash
# Access PostgreSQL as superuser
sudo -u postgres psql

# Inside psql prompt:
```

```sql
-- Create database
CREATE DATABASE heimdallweb;

-- Create user
CREATE USER heimdall WITH PASSWORD 'your_secure_password_here';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE heimdallweb TO heimdall;

-- Connect to database
\c heimdallweb

-- Grant schema privileges
GRANT ALL ON SCHEMA public TO heimdall;

-- Exit psql
\q
```

**✅ Verification:**
```bash
psql -h localhost -U heimdall -d heimdallweb -c "SELECT version();"
# Expected: PostgreSQL version information
```

---

### Step 3: Configure Connection String

Edit `src/HeimdallWeb.WebApi/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AppDbConnectionString": "Host=localhost;Database=heimdallweb;Username=heimdall;Password=your_secure_password_here;Port=5432"
  }
}
```

**⚠️ Security Note:** Never commit real passwords to Git. Use User Secrets:

```bash
cd src/HeimdallWeb.WebApi
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AppDbConnectionString" "Host=localhost;Database=heimdallweb;Username=heimdall;Password=your_password;Port=5432"
```

---

## 🚀 Part 2: EF Core Migrations (1 hour)

### Step 1: Create Initial Migration

```bash
# From repository root
dotnet ef migrations add InitialPostgreSQLMigration \
    --project src/HeimdallWeb.Infrastructure \
    --startup-project src/HeimdallWeb.WebApi \
    --context AppDbContext \
    --output-dir Data/Migrations
```

**✅ Expected Output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

**Verify Migration Files:**
```bash
ls src/HeimdallWeb.Infrastructure/Data/Migrations/
# Expected:
# - 20260205xxxxxx_InitialPostgreSQLMigration.cs
# - 20260205xxxxxx_InitialPostgreSQLMigration.Designer.cs
# - AppDbContextModelSnapshot.cs
```

---

### Step 2: Review Migration SQL (Optional)

Generate SQL script to review before applying:

```bash
dotnet ef migrations script \
    --project src/HeimdallWeb.Infrastructure \
    --startup-project src/HeimdallWeb.WebApi \
    --context AppDbContext \
    --output migration.sql
```

**Review `migration.sql`:**
- ✅ 7 tables created (`tb_user`, `tb_history`, etc.)
- ✅ Columns use snake_case (`user_id`, `created_at`, etc.)
- ✅ `raw_json_result` column type is `jsonb`
- ✅ GIN index on `raw_json_result`: `CREATE INDEX ix_tb_history_raw_json_result ON tb_history USING GIN(raw_json_result);`
- ✅ Foreign key constraints defined
- ✅ Indexes on: `email`, `target`, `user_id`, `history_id`, `created_at`

---

### Step 3: Apply Migration to Database

```bash
dotnet ef database update \
    --project src/HeimdallWeb.Infrastructure \
    --startup-project src/HeimdallWeb.WebApi \
    --context AppDbContext
```

**✅ Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20260205xxxxxx_InitialPostgreSQLMigration'.
Done.
```

---

### Step 4: Verify Database Schema

```bash
psql -h localhost -U heimdall -d heimdallweb
```

```sql
-- List all tables
\dt

-- Expected output:
--  Schema |      Name       | Type  |  Owner
-- --------+-----------------+-------+---------
--  public | tb_user         | table | heimdall
--  public | tb_history      | table | heimdall
--  public | tb_finding      | table | heimdall
--  public | tb_technology   | table | heimdall
--  public | tb_ia_summary   | table | heimdall
--  public | tb_log          | table | heimdall
--  public | tb_user_usage   | table | heimdall

-- Verify tb_user structure
\d tb_user

-- Expected columns:
-- - user_id (integer, PK)
-- - username (text)
-- - email (text, unique index)
-- - password_hash (text)
-- - user_type (integer)
-- - is_active (boolean)
-- - created_at (timestamp)
-- - updated_at (timestamp, nullable)
-- - profile_image (text, nullable)

-- Verify JSONB column in tb_history
\d tb_history

-- Expected:
-- - raw_json_result (jsonb)
-- - GIN index: ix_tb_history_raw_json_result

-- List indexes
\di

-- Expected GIN index:
-- ix_tb_history_raw_json_result | USING gin (raw_json_result)

-- Exit
\q
```

**✅ All 7 tables created with correct schema**

---

## 📊 Part 3: SQL VIEWs Creation (4 hours - CRITICAL)

**⚠️ HIGHEST RISK SECTION:** SQL VIEWs were converted from MySQL to PostgreSQL syntax. Manual testing is essential.

### Overview of VIEWs

| # | VIEW Name | Purpose | Complexity |
|---|-----------|---------|------------|
| 01 | vw_dashboard_user_stats | User counts (total, active, inactive) | Low |
| 02 | vw_dashboard_scan_stats | Scan counts (total, last 7d, last 30d) | Medium |
| 03 | vw_dashboard_logs_overview | Log counts by level | Low |
| 04 | vw_dashboard_recent_activity | Recent scans with user info | Medium |
| 05 | vw_dashboard_scan_trend_daily | Scans per day (last 30d) | High |
| 06 | vw_dashboard_user_registration_trend | User registrations per day (last 30d) | High |
| 07 | vw_user_scan_summary | User's scan statistics | Medium |
| 08 | vw_user_findings_summary | User's findings by severity | Medium |
| 09 | vw_user_risk_trend | User's risk level over time | High |
| 10 | vw_user_category_breakdown | User's findings by category | Medium |
| 11 | vw_admin_ia_summary_stats | AI summary statistics | Medium |
| 12 | vw_admin_risk_distribution_daily | Risk distribution per day | High |
| 13 | vw_admin_top_categories | Top 10 finding categories | Low |
| 14 | vw_admin_most_vulnerable_targets | Most vulnerable scan targets | Medium |

---

### Step 1: Create All VIEWs

Connect to PostgreSQL:
```bash
psql -h localhost -U heimdall -d heimdallweb
```

Execute each VIEW creation script:

```sql
-- 01 - Dashboard User Stats
\i src/HeimdallWeb.Infrastructure/Data/Views/01_vw_dashboard_user_stats.sql

-- 02 - Dashboard Scan Stats
\i src/HeimdallWeb.Infrastructure/Data/Views/02_vw_dashboard_scan_stats.sql

-- 03 - Dashboard Logs Overview
\i src/HeimdallWeb.Infrastructure/Data/Views/03_vw_dashboard_logs_overview.sql

-- 04 - Dashboard Recent Activity
\i src/HeimdallWeb.Infrastructure/Data/Views/04_vw_dashboard_recent_activity.sql

-- 05 - Dashboard Scan Trend Daily
\i src/HeimdallWeb.Infrastructure/Data/Views/05_vw_dashboard_scan_trend_daily.sql

-- 06 - Dashboard User Registration Trend
\i src/HeimdallWeb.Infrastructure/Data/Views/06_vw_dashboard_user_registration_trend.sql

-- 07 - User Scan Summary
\i src/HeimdallWeb.Infrastructure/Data/Views/07_vw_user_scan_summary.sql

-- 08 - User Findings Summary
\i src/HeimdallWeb.Infrastructure/Data/Views/08_vw_user_findings_summary.sql

-- 09 - User Risk Trend
\i src/HeimdallWeb.Infrastructure/Data/Views/09_vw_user_risk_trend.sql

-- 10 - User Category Breakdown
\i src/HeimdallWeb.Infrastructure/Data/Views/10_vw_user_category_breakdown.sql

-- 11 - Admin IA Summary Stats
\i src/HeimdallWeb.Infrastructure/Data/Views/11_vw_admin_ia_summary_stats.sql

-- 12 - Admin Risk Distribution Daily
\i src/HeimdallWeb.Infrastructure/Data/Views/12_vw_admin_risk_distribution_daily.sql

-- 13 - Admin Top Categories
\i src/HeimdallWeb.Infrastructure/Data/Views/13_vw_admin_top_categories.sql

-- 14 - Admin Most Vulnerable Targets
\i src/HeimdallWeb.Infrastructure/Data/Views/14_vw_admin_most_vulnerable_targets.sql
```

**✅ Expected for each:**
```
CREATE VIEW
```

**❌ If errors occur:**
- Check PostgreSQL syntax (DATE_SUB → INTERVAL conversion)
- Verify column names match tb_* tables
- Check EXTRACT(EPOCH FROM ...) for duration calculations

---

### Step 2: Verify VIEWs Exist

```sql
-- List all views
\dv

-- Expected output: 14 views listed
-- vw_dashboard_user_stats
-- vw_dashboard_scan_stats
-- ... (all 14)
```

---

### Step 3: Test Each VIEW with Sample Data

**⚠️ IMPORTANT:** VIEWs will return empty results if database has no data. Insert test data first.

#### Insert Test Data

```sql
-- Insert test user
INSERT INTO tb_user (username, email, password_hash, user_type, is_active, created_at)
VALUES ('testuser', 'test@example.com', 'hashed_password', 1, true, NOW());

-- Get user_id (should be 1)
SELECT user_id FROM tb_user WHERE email = 'test@example.com';

-- Insert test scan history
INSERT INTO tb_history (target, raw_json_result, created_date, user_id, duration, has_completed, summary)
VALUES (
    'https://example.com',
    '{"scan_id": "test-123", "findings": []}'::jsonb,
    NOW(),
    1,
    INTERVAL '30 seconds',
    true,
    'Test scan summary'
);

-- Get history_id
SELECT history_id FROM tb_history WHERE target = 'https://example.com';

-- Insert test finding
INSERT INTO tb_finding (type, description, severity, evidence, recommendation, history_id)
VALUES (
    'Security Header Missing',
    'X-Frame-Options header not found',
    2,  -- Medium severity
    'Response headers: {...}',
    'Add X-Frame-Options: DENY header',
    1   -- Replace with actual history_id
);

-- Insert test log
INSERT INTO tb_log (timestamp, level, source, message, user_id, code)
VALUES (
    NOW(),
    'Information',
    'ScanService',
    'Scan initiated for target: https://example.com',
    1,
    0  -- INIT_SCAN from LogEventCode
);
```

---

#### Test Each VIEW

**01 - vw_dashboard_user_stats:**
```sql
SELECT * FROM vw_dashboard_user_stats;

-- Expected:
-- total_users | active_users | inactive_users
-- ------------|--------------|---------------
--      1      |      1       |       0
```

**02 - vw_dashboard_scan_stats:**
```sql
SELECT * FROM vw_dashboard_scan_stats;

-- Expected:
-- total_scans | scans_last_7_days | scans_last_30_days | avg_duration_seconds
-- ------------|-------------------|--------------------|-----------------------
--      1      |         1         |          1         |         30.00
```

**03 - vw_dashboard_logs_overview:**
```sql
SELECT * FROM vw_dashboard_logs_overview;

-- Expected:
-- log_level    | count
-- -------------|-------
-- Information  |   1
```

**04 - vw_dashboard_recent_activity:**
```sql
SELECT * FROM vw_dashboard_recent_activity LIMIT 5;

-- Expected columns:
-- history_id, target, created_date, username, has_completed, duration_seconds
```

**05 - vw_dashboard_scan_trend_daily:**
```sql
SELECT * FROM vw_dashboard_scan_trend_daily ORDER BY scan_date DESC LIMIT 7;

-- Expected: Daily scan counts for last 30 days (most will be 0)
```

**06 - vw_dashboard_user_registration_trend:**
```sql
SELECT * FROM vw_dashboard_user_registration_trend ORDER BY registration_date DESC LIMIT 7;

-- Expected: Daily user registrations for last 30 days
```

**07 - vw_user_scan_summary:**
```sql
SELECT * FROM vw_user_scan_summary WHERE user_id = 1;

-- Expected:
-- user_id | total_scans | completed_scans | avg_duration_seconds
-- --------|-------------|-----------------|----------------------
--    1    |      1      |        1        |        30.00
```

**08 - vw_user_findings_summary:**
```sql
SELECT * FROM vw_user_findings_summary WHERE user_id = 1;

-- Expected:
-- user_id | total_findings | critical | high | medium | low | informational
-- --------|----------------|----------|------|--------|-----|---------------
--    1    |       1        |    0     |  0   |   1    |  0  |       0
```

**09 - vw_user_risk_trend:**
```sql
SELECT * FROM vw_user_risk_trend WHERE user_id = 1 ORDER BY scan_date DESC LIMIT 5;

-- Expected: Risk levels over time for user scans
```

**10 - vw_user_category_breakdown:**
```sql
SELECT * FROM vw_user_category_breakdown WHERE user_id = 1;

-- Expected:
-- user_id | category_name           | count | percentage
-- --------|------------------------|-------|------------
--    1    | Security Header Missing |   1   |  100.00
```

**11 - vw_admin_ia_summary_stats:**
```sql
SELECT * FROM vw_admin_ia_summary_stats;

-- Expected: AI summary statistics (may be empty if no IA summaries)
```

**12 - vw_admin_risk_distribution_daily:**
```sql
SELECT * FROM vw_admin_risk_distribution_daily ORDER BY summary_date DESC LIMIT 7;

-- Expected: Risk distribution per day
```

**13 - vw_admin_top_categories:**
```sql
SELECT * FROM vw_admin_top_categories LIMIT 10;

-- Expected:
-- category_name           | total_findings | percentage
-- ----------------------|----------------|------------
-- Security Header Missing |       1        |   100.00
```

**14 - vw_admin_most_vulnerable_targets:**
```sql
SELECT * FROM vw_admin_most_vulnerable_targets LIMIT 10;

-- Expected:
-- target              | scan_count | total_findings | avg_severity
-- --------------------|------------|----------------|---------------
-- https://example.com |      1     |       1        |    2.00
```

---

### Step 4: Compare with MySQL Results (Optional)

If you have access to the old MySQL database:

**MySQL:**
```sql
-- Connect to old database
mysql -u root -p heimdallweb

-- Run same queries
SELECT * FROM vw_dashboard_user_stats;
SELECT * FROM vw_dashboard_scan_stats;
-- ... etc
```

**Compare results:**
- Row counts should match
- Numeric calculations should be identical
- Date ranges should align

**⚠️ Known differences:**
- Duration calculations: MySQL uses `TIME_TO_SEC()`, PostgreSQL uses `EXTRACT(EPOCH FROM)`
- Date intervals: MySQL `DATE_SUB()` vs PostgreSQL `NOW() - INTERVAL`
- Both should produce same results, but verify manually

---

### Step 5: Performance Testing VIEWs

Test query performance on larger datasets:

```sql
-- Enable timing
\timing

-- Test complex view
SELECT * FROM vw_dashboard_scan_trend_daily;

-- Expected: < 100ms for small dataset, < 500ms for 10k+ scans
```

**If slow:**
- Check indexes exist: `\di`
- Analyze tables: `ANALYZE tb_history; ANALYZE tb_user;`
- Verify GIN index on JSONB: `\d tb_history`

---

## 🗃️ Part 4: Repository Testing (2 hours)

### Test Setup

Create integration test project or use direct repository instantiation.

**Option 1: Integration Tests (Recommended)**

Create test file: `tests/HeimdallWeb.IntegrationTests/RepositoryTests.cs`

```csharp
using HeimdallWeb.Infrastructure.Data;
using HeimdallWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class RepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;

    public RepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=heimdallweb_test;Username=heimdall;Password=test")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

**Option 2: Manual Testing (Simpler)**

Use PostgreSQL directly to verify repository methods.

---

### Test 1: UserRepository CRUD

```csharp
[Fact]
public async Task UserRepository_CRUD_Success()
{
    var repo = new UserRepository(_context);

    // CREATE
    var email = EmailAddress.Create("newuser@example.com");
    var user = User.Create(
        username: "newuser",
        email: email,
        passwordHash: "hashed_password_123",
        userType: UserType.Default
    );

    var created = await repo.AddAsync(user);
    await _context.SaveChangesAsync();

    Assert.NotEqual(0, created.UserId);

    // READ by ID
    var fetched = await repo.GetByIdAsync(created.UserId);
    Assert.NotNull(fetched);
    Assert.Equal("newuser", fetched.Username);

    // READ by Email
    var fetchedByEmail = await repo.GetByEmailAsync(email);
    Assert.NotNull(fetchedByEmail);
    Assert.Equal(created.UserId, fetchedByEmail.UserId);

    // UPDATE
    fetched.UpdatePassword("new_hashed_password");
    await repo.UpdateAsync(fetched);
    await _context.SaveChangesAsync();

    var updated = await repo.GetByIdAsync(created.UserId);
    Assert.Equal("new_hashed_password", updated.PasswordHash);

    // EXISTS check
    var exists = await repo.ExistsByEmailAsync(email);
    Assert.True(exists);

    // GET ALL
    var all = await repo.GetAllAsync();
    Assert.Contains(all, u => u.UserId == created.UserId);
}
```

**Manual Test (SQL):**
```sql
-- INSERT
INSERT INTO tb_user (username, email, password_hash, user_type, is_active, created_at)
VALUES ('testuser2', 'test2@example.com', 'hashed', 1, true, NOW())
RETURNING user_id;

-- SELECT by ID
SELECT * FROM tb_user WHERE user_id = 2;

-- SELECT by email
SELECT * FROM tb_user WHERE email = 'test2@example.com';

-- UPDATE
UPDATE tb_user SET password_hash = 'new_hash' WHERE user_id = 2;

-- DELETE (if implemented)
DELETE FROM tb_user WHERE user_id = 2;
```

---

### Test 2: ScanHistoryRepository with JSONB

**Test JSONB storage and retrieval:**

```csharp
[Fact]
public async Task ScanHistoryRepository_JSONB_Success()
{
    var repo = new ScanHistoryRepository(_context);
    var userRepo = new UserRepository(_context);

    // Create user first
    var user = await userRepo.AddAsync(/* ... */);
    await _context.SaveChangesAsync();

    // Create scan with JSONB
    var target = ScanTarget.Create("https://jsonb-test.com");
    var duration = ScanDuration.Create(TimeSpan.FromSeconds(45));

    var scan = ScanHistory.Create(
        target: target,
        userId: user.UserId,
        rawJsonResult: "{\"test\": \"jsonb_data\", \"nested\": {\"key\": \"value\"}}"
    );
    scan.CompleteScan(duration, "JSONB test summary");

    var created = await repo.AddAsync(scan);
    await _context.SaveChangesAsync();

    // Fetch and verify JSONB
    var fetched = await repo.GetByIdAsync(created.HistoryId);
    Assert.Contains("jsonb_data", fetched.RawJsonResult);

    // Test JSONB query (if implemented in repository)
    // var byTarget = await repo.GetByTargetAsync(target);
    // Assert.NotEmpty(byTarget);
}
```

**Manual Test (SQL - JSONB operations):**
```sql
-- Insert with JSONB
INSERT INTO tb_history (target, raw_json_result, created_date, user_id, has_completed)
VALUES (
    'https://jsonb-test.com',
    '{"scan_id": "abc123", "findings": [{"severity": 3, "type": "XSS"}], "metadata": {"timestamp": "2024-01-01"}}'::jsonb,
    NOW(),
    1,
    true
);

-- Query JSONB with operators
SELECT
    history_id,
    target,
    raw_json_result->>'scan_id' AS scan_id,
    raw_json_result->'findings' AS findings,
    raw_json_result->'metadata'->>'timestamp' AS timestamp
FROM tb_history
WHERE raw_json_result @> '{"scan_id": "abc123"}'::jsonb;

-- Test GIN index performance
EXPLAIN ANALYZE
SELECT * FROM tb_history
WHERE raw_json_result @> '{"findings": [{"severity": 3}]}'::jsonb;

-- Expected: Index Scan using ix_tb_history_raw_json_result (GIN index used)
```

---

### Test 3: Finding Repository with Severity Enum

```sql
-- Insert with severity enum (stored as integer)
INSERT INTO tb_finding (type, description, severity, evidence, recommendation, history_id)
VALUES (
    'SQL Injection',
    'SQL injection vulnerability detected',
    4,  -- Critical
    'Query: SELECT * FROM users WHERE id = '' OR 1=1--',
    'Use parameterized queries',
    1
);

-- Query by severity
SELECT * FROM tb_finding WHERE severity = 4;  -- Critical
SELECT * FROM tb_finding WHERE severity >= 3;  -- High + Critical

-- Group by severity
SELECT severity, COUNT(*) FROM tb_finding GROUP BY severity ORDER BY severity DESC;
```

---

### Test 4: UnitOfWork Transaction

**Test rollback:**
```csharp
[Fact]
public async Task UnitOfWork_Rollback_Success()
{
    var uow = new UnitOfWork(_context);

    await uow.BeginTransactionAsync();

    try
    {
        // Create user
        var user = await uow.Users.AddAsync(/* ... */);
        await uow.SaveChangesAsync();

        // Simulate error
        throw new Exception("Simulated error");

        await uow.CommitTransactionAsync();
    }
    catch
    {
        await uow.RollbackTransactionAsync();
    }

    // Verify rollback - user should not exist
    var users = await uow.Users.GetAllAsync();
    Assert.Empty(users);
}
```

**Manual Test (SQL):**
```sql
-- Start transaction
BEGIN;

-- Insert user
INSERT INTO tb_user (username, email, password_hash, user_type, is_active, created_at)
VALUES ('rollback_test', 'rollback@example.com', 'hash', 1, true, NOW());

-- Verify exists
SELECT * FROM tb_user WHERE email = 'rollback@example.com';

-- Rollback
ROLLBACK;

-- Verify doesn't exist
SELECT * FROM tb_user WHERE email = 'rollback@example.com';
-- Expected: 0 rows
```

---

## ⚡ Part 5: Performance Validation (1 hour)

### Test 1: AsNoTracking() Performance

Compare performance with and without `.AsNoTracking()`:

**With tracking (slower):**
```csharp
var users = await _context.Users.ToListAsync();  // 100ms
```

**Without tracking (faster):**
```csharp
var users = await _context.Users.AsNoTracking().ToListAsync();  // 60-70ms
```

**Expected improvement:** 30-40% faster for read-only queries.

---

### Test 2: GIN Index on JSONB

Test JSONB query performance with GIN index:

```sql
-- Disable GIN index temporarily
DROP INDEX ix_tb_history_raw_json_result;

-- Query JSONB (slow without index)
EXPLAIN ANALYZE
SELECT * FROM tb_history
WHERE raw_json_result @> '{"findings": [{"severity": 4}]}'::jsonb;
-- Expected: Seq Scan (slow)

-- Recreate GIN index
CREATE INDEX ix_tb_history_raw_json_result ON tb_history USING GIN(raw_json_result);

-- Query again (fast with index)
EXPLAIN ANALYZE
SELECT * FROM tb_history
WHERE raw_json_result @> '{"findings": [{"severity": 4}]}'::jsonb;
-- Expected: Index Scan using GIN index (fast)
```

**Performance difference:**
- Without GIN: 500ms+ on 10k rows
- With GIN: 5-10ms on 10k rows

---

### Test 3: Include() vs N+1 Queries

**Bad (N+1 problem):**
```csharp
var users = await _context.Users.ToListAsync();
foreach (var user in users)
{
    var scans = await _context.ScanHistories
        .Where(s => s.UserId == user.UserId)
        .ToListAsync();
    // 1 + N queries (slow)
}
```

**Good (Include):**
```csharp
var users = await _context.Users
    .Include(u => u.ScanHistories)
    .ToListAsync();
// 1 query (fast)
```

---

### Test 4: Connection Pooling

Verify PostgreSQL connection pooling is active:

```sql
-- Check active connections
SELECT count(*) FROM pg_stat_activity WHERE datname = 'heimdallweb';

-- Monitor connections during load test
-- Expected: Connection reuse, not creating new connections per request
```

---

## 📋 Testing Checklist

After completing all tests, verify:

### Database Setup
- [ ] PostgreSQL installed and running
- [ ] Database `heimdallweb` created
- [ ] User `heimdall` created with correct privileges
- [ ] Connection string configured

### Migrations
- [ ] Initial migration created successfully
- [ ] Migration applied without errors
- [ ] All 7 tables exist in database
- [ ] JSONB column type correct in `tb_history`
- [ ] GIN index exists on `raw_json_result`
- [ ] All indexes created (email, target, FKs, timestamps)

### SQL VIEWs
- [ ] All 14 VIEWs created successfully
- [ ] No SQL syntax errors
- [ ] Each VIEW returns expected columns
- [ ] Test data inserted successfully
- [ ] Each VIEW tested with SELECT query
- [ ] Results match expected output
- [ ] Complex VIEWs (05, 06, 09, 12) validated carefully
- [ ] Performance acceptable (< 500ms)

### Repositories
- [ ] UserRepository: CREATE, READ (by ID, by email), UPDATE, EXISTS
- [ ] ScanHistoryRepository: JSONB storage and retrieval
- [ ] FindingRepository: Severity enum handled correctly
- [ ] TechnologyRepository: Basic CRUD
- [ ] IASummaryRepository: Basic CRUD
- [ ] AuditLogRepository: Log level enum
- [ ] UserUsageRepository: Date-based queries

### UnitOfWork
- [ ] Transaction commit works
- [ ] Transaction rollback works
- [ ] All repositories accessible via UnitOfWork
- [ ] SaveChangesAsync() persists data

### Performance
- [ ] AsNoTracking() improves query performance
- [ ] GIN index improves JSONB queries
- [ ] Include() prevents N+1 queries
- [ ] Connection pooling active

---

## 🚨 Common Issues & Troubleshooting

### Issue 1: "relation does not exist"

**Error:** `ERROR: relation "tb_user" does not exist`

**Cause:** Migration not applied

**Solution:**
```bash
dotnet ef database update --project src/HeimdallWeb.Infrastructure --startup-project src/HeimdallWeb.WebApi
```

---

### Issue 2: "column does not exist"

**Error:** `ERROR: column "created_date" does not exist`

**Cause:** Column name mismatch (snake_case vs camelCase)

**Solution:** Verify `EntityTypeConfiguration` uses correct column names:
```csharp
builder.Property(e => e.CreatedDate)
    .HasColumnName("created_date");  // Must match database
```

---

### Issue 3: VIEW syntax error

**Error:** `ERROR: syntax error at or near "INTERVAL"`

**Cause:** MySQL syntax not converted to PostgreSQL

**Solution:** Check VIEW file for correct PostgreSQL syntax:
```sql
-- Wrong (MySQL):
DATE_SUB(NOW(), INTERVAL 7 DAY)

-- Correct (PostgreSQL):
NOW() - INTERVAL '7 days'
```

---

### Issue 4: JSONB query slow

**Cause:** GIN index missing

**Solution:**
```sql
CREATE INDEX ix_tb_history_raw_json_result ON tb_history USING GIN(raw_json_result);
```

---

### Issue 5: Connection timeout

**Error:** `Npgsql.NpgsqlException: Connection timeout`

**Cause:** PostgreSQL not running or firewall blocking

**Solution:**
```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Start if not running
sudo systemctl start postgresql

# Check pg_hba.conf allows localhost connections
sudo nano /etc/postgresql/14/main/pg_hba.conf
# Should have: host all all 127.0.0.1/32 md5
```

---

## ✅ Completion Criteria

Phase 2 testing is complete when:

1. ✅ All 7 tables exist in PostgreSQL
2. ✅ All 14 SQL VIEWs created and return correct data
3. ✅ All 7 repositories tested with CRUD operations
4. ✅ UnitOfWork transactions (commit/rollback) work
5. ✅ JSONB GIN index improves query performance
6. ✅ AsNoTracking() provides 30-40% performance improvement
7. ✅ No SQL errors in logs
8. ✅ Build succeeds with 0 warnings, 0 errors

**After completion:**
- ✅ Update `plano_migracao.md` checkboxes
- ✅ Commit changes with message: `Phase 2: Infrastructure Layer complete - PostgreSQL migration tested`
- ✅ Proceed to Phase 3: Application Layer

---

## 📚 Additional Resources

- [EF Core PostgreSQL Provider](https://www.npgsql.org/efcore/)
- [PostgreSQL JSON Functions](https://www.postgresql.org/docs/current/functions-json.html)
- [PostgreSQL GIN Indexes](https://www.postgresql.org/docs/current/gin-intro.html)
- [Migration Plan Reference](/home/alex/Documents/WindowsBkp/Dotnet/HeimdallWeb/plano_migracao.md)

---

**End of Phase 2 Testing Guide**
