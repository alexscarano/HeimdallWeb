# HeimdallWeb API - Endpoint Testing Guide

**Date Created**: 2026-02-07  
**API Version**: v1  
**Base URL**: `http://localhost:5110`

---

## 📋 Test Summary

| Status | Count | Endpoints |
|--------|-------|-----------|
| ✅ PASSED | 8/15 | Register, Login, Get Profile, Get Statistics, Update User, Execute Scan, Get Scans, Unauthorized Access |
| ❌ FAILED | 7/15 | Get History Detail, Get Findings, Get Technologies, Logout, Invalid Registration, Duplicate Email, Invalid Login |

---

## 🚨 Critical Issues Found

### Issue #1: Read-Only Collection Exception in ScanHistory
**Affected Endpoints**:
- `GET /api/v1/scan-histories/{id}`
- `GET /api/v1/scan-histories/{id}/findings`
- `GET /api/v1/scan-histories/{id}/technologies`

**Error**: `System.NotSupportedException: Collection is read-only`

**Root Cause**: The `ScanHistory` entity uses `IReadOnlyCollection<IASummary>` but EF Core is trying to add items during materialization.

**Location**: `/src/HeimdallWeb.Infrastructure/Repositories/ScanHistoryRepository.cs:25`

**Fix Required**: Change the navigation property in `ScanHistory` entity from `IReadOnlyCollection<IASummary>` to `List<IASummary>` or `ICollection<IASummary>`.

---

### Issue #2: Missing Exception Handling Middleware
**Affected Endpoints**: All endpoints returning validation/business logic errors

**Problem**: FluentValidation exceptions and business exceptions are returning HTTP 500 instead of proper status codes (400, 401, 409).

**Expected Behavior**:
- FluentValidation errors → HTTP 400 Bad Request
- Authentication failures → HTTP 401 Unauthorized  
- Duplicate resource → HTTP 409 Conflict

**Fix Required**: Add global exception handling middleware in `Program.cs`.

---

### Issue #3: Logout Returning Wrong Status Code
**Endpoint**: `POST /api/v1/auth/logout`

**Current**: Returns HTTP 200 OK  
**Expected**: Returns HTTP 204 No Content

**Fix Required**: The endpoint is correctly using `Results.NoContent()` but something is wrapping it. Need to investigate middleware pipeline.

---

## 📝 Detailed Test Results

### 1. AUTHENTICATION ENDPOINTS

#### ✅ POST /api/v1/auth/register
**Purpose**: Register a new user account

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "username": "testuser123",
    "password": "Test@1234"
  }'
```

**Expected Response**: `HTTP 201 Created`
```json
{
  "userId": 3,
  "username": "testuser123",
  "email": "testuser@example.com",
  "userType": 1,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true
}
```

**Validation Rules**:
- Email: Required, valid format, max 100 chars
- Username: Required, 6-30 chars, alphanumeric + hyphens/underscores
- Password: Required, 8-100 chars, must contain uppercase, lowercase, number, special char

**Error Scenarios**:
| Scenario | Expected HTTP | Current HTTP | Status |
|----------|---------------|--------------|--------|
| Invalid email format | 400 | 500 | ❌ |
| Short username (<6 chars) | 400 | 500 | ❌ |
| Weak password | 400 | 500 | ❌ |
| Duplicate email | 409 | 500 | ❌ |

---

#### ✅ POST /api/v1/auth/login
**Purpose**: Authenticate user and receive JWT token

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrLogin": "testuser@example.com",
    "password": "Test@1234"
  }'
```

**Expected Response**: `HTTP 200 OK`
```json
{
  "userId": 3,
  "username": "testuser123",
  "email": "testuser@example.com",
  "userType": 1,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true
}
```

**Cookie Set**: `authHeimdallCookie` (HttpOnly, Secure, SameSite=Strict, 24h expiration)

**Error Scenarios**:
| Scenario | Expected HTTP | Current HTTP | Status |
|----------|---------------|--------------|--------|
| Invalid credentials | 401 | 500 | ❌ |
| Missing email/password | 400 | 500 | ❌ |
| Inactive account | 403 | ? | ⚠️ Not tested |

---

#### ❌ POST /api/v1/auth/logout
**Purpose**: Log out user and clear authentication cookie

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/logout \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Expected Response**: `HTTP 204 No Content` (no body)  
**Current Response**: `HTTP 200 OK` (empty body)

**Issue**: Status code mismatch

---

### 2. USER ENDPOINTS

#### ✅ GET /api/v1/users/{id}/profile
**Purpose**: Get user profile information

**Request**:
```bash
curl http://localhost:5110/api/v1/users/3/profile \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Expected Response**: `HTTP 200 OK`
```json
{
  "userId": 3,
  "username": "testuser123",
  "email": "testuser@example.com",
  "userType": 1,
  "isActive": true,
  "profileImage": null,
  "createdDate": "2026-02-07T22:02:20Z"
}
```

**Authorization**: Requires valid JWT token. Users can only access their own profile (or admins can access any profile).

---

#### ✅ GET /api/v1/users/{id}/statistics
**Purpose**: Get user scan statistics

**Request**:
```bash
curl http://localhost:5110/api/v1/users/3/statistics \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Expected Response**: `HTTP 200 OK`
```json
{
  "totalScans": 1,
  "completedScans": 0,
  "pendingScans": 1,
  "totalFindings": 0,
  "criticalFindings": 0,
  "highFindings": 0,
  "mediumFindings": 0,
  "lowFindings": 0,
  "infoFindings": 0
}
```

---

#### ✅ PUT /api/v1/users/{id}
**Purpose**: Update user profile

**Request**:
```bash
curl -X PUT http://localhost:5110/api/v1/users/3 \
  -H "Content-Type: application/json" \
  -H "Cookie: authHeimdallCookie=<token>" \
  -d '{
    "newUsername": "updateduser",
    "newEmail": "updated@example.com"
  }'
```

**Expected Response**: `HTTP 200 OK`
```json
{
  "userId": 3,
  "username": "updateduser",
  "email": "updated@example.com",
  "message": "User updated successfully"
}
```

**Authorization**: Users can only update their own profile.

---

#### ⚠️ POST /api/v1/users/{id}/profile-image
**Purpose**: Upload profile image

**Not Tested**: Binary data handling requires special testing

---

#### ⚠️ DELETE /api/v1/users/{id}
**Purpose**: Delete user account

**Not Tested**: Destructive operation

---

### 3. SCAN ENDPOINTS

#### ✅ POST /api/v1/scans
**Purpose**: Execute a security scan on a target URL

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -H "Content-Type: application/json" \
  -H "Cookie: authHeimdallCookie=<token>" \
  -d '{
    "target": "https://example.com"
  }'
```

**Expected Response**: `HTTP 201 Created`
```json
{
  "historyId": 12,
  "target": "https://example.com",
  "createdDate": "2026-02-07T22:02:45Z",
  "status": "Pending",
  "message": "Scan initiated successfully"
}
```

**Rate Limiting**: 4 requests/minute per IP (ScanPolicy)

**Validation**: Target must be a valid URL

---

#### ✅ GET /api/v1/scans
**Purpose**: Get user's scan history (paginated)

**Request**:
```bash
curl "http://localhost:5110/api/v1/scans?page=1&pageSize=10" \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Expected Response**: `HTTP 200 OK`
```json
{
  "items": [
    {
      "historyId": 12,
      "target": "https://example.com",
      "createdDate": "2026-02-07T22:02:45Z",
      "duration": null,
      "hasCompleted": false,
      "overallRisk": null
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Query Parameters**:
- `page` (optional, default: 1)
- `pageSize` (optional, default: 10, max: 100)

---

### 4. HISTORY ENDPOINTS

#### ❌ GET /api/v1/scan-histories/{id}
**Purpose**: Get detailed scan history by ID

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/12 \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Current Response**: `HTTP 500 Internal Server Error`

**Error**: `System.NotSupportedException: Collection is read-only`

**Expected Response**: `HTTP 200 OK` with scan details, findings, technologies, AI summary

---

#### ❌ GET /api/v1/scan-histories/{id}/findings
**Purpose**: Get findings (vulnerabilities) for a scan

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/12/findings \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Current Response**: `HTTP 500 Internal Server Error`

**Error**: Same read-only collection issue

**Expected Response**: `HTTP 200 OK`
```json
[
  {
    "findingId": 1,
    "type": "Security Header Missing",
    "description": "Missing X-Content-Type-Options header",
    "severity": "Medium",
    "evidence": "Response headers...",
    "recommendation": "Add X-Content-Type-Options: nosniff"
  }
]
```

---

#### ❌ GET /api/v1/scan-histories/{id}/technologies
**Purpose**: Get detected technologies for a scan

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/12/technologies \
  -H "Cookie: authHeimdallCookie=<token>"
```

**Current Response**: `HTTP 500 Internal Server Error`

**Error**: Same read-only collection issue

**Expected Response**: `HTTP 200 OK`
```json
[
  {
    "technologyId": 1,
    "technologyName": "Nginx",
    "version": "1.21.0",
    "category": "Web Server",
    "description": "Detected from Server header"
  }
]
```

---

#### ⚠️ GET /api/v1/scan-histories/{id}/export
**Purpose**: Export single scan as PDF

**Not Tested**: Binary PDF response

---

#### ⚠️ GET /api/v1/scan-histories/export
**Purpose**: Export all user scans as PDF

**Not Tested**: Binary PDF response

---

#### ⚠️ DELETE /api/v1/scan-histories/{id}
**Purpose**: Delete a scan history

**Not Tested**: Destructive operation

---

### 5. DASHBOARD/ADMIN ENDPOINTS

#### ⚠️ GET /api/v1/dashboard/admin
**Purpose**: Get admin dashboard statistics

**Not Tested**: Requires admin privileges (UserType = 2)

**Authorization**: Admin only

---

#### ⚠️ GET /api/v1/dashboard/users
**Purpose**: Get users list with filtering/pagination

**Not Tested**: Requires admin privileges

**Authorization**: Admin only

---

#### ⚠️ PATCH /api/v1/admin/users/{id}/status
**Purpose**: Toggle user active/inactive status

**Not Tested**: Requires admin privileges

**Authorization**: Admin only

---

#### ⚠️ DELETE /api/v1/admin/users/{id}
**Purpose**: Delete user by admin

**Not Tested**: Requires admin privileges

**Authorization**: Admin only

---

## 🔐 Authentication & Authorization

### JWT Token Structure
```json
{
  "sub": "3",
  "unique_name": "testuser123",
  "email": "testuser@example.com",
  "role": "1",
  "nbf": 1770501900,
  "exp": 1770545100,
  "iat": 1770501900,
  "iss": "HeimdallWeb",
  "aud": "HeimdallWebUsers"
}
```

### Cookie Configuration
- **Name**: `authHeimdallCookie`
- **HttpOnly**: `true` (prevents JavaScript access)
- **Secure**: `true` (HTTPS only in production)
- **SameSite**: `Strict`
- **Expiration**: 24 hours from login

### Rate Limiting
- **Global**: 85 requests/minute per IP
- **Scan Endpoints**: 4 requests/minute per IP (named policy "ScanPolicy")

---

## ⚙️ How to Run Tests

### Prerequisites
1. API running on `http://localhost:5110`
2. MySQL database accessible
3. `curl` and `jq` installed

### Run All Tests
```bash
chmod +x /tmp/complete_api_test.sh
/tmp/complete_api_test.sh
```

### Manual Test Examples

**Register User**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","username":"testuser","password":"Test@1234"}'
```

**Login**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"emailOrLogin":"user@test.com","password":"Test@1234"}'
```

**Get Profile** (using saved cookie):
```bash
curl http://localhost:5110/api/v1/users/1/profile \
  -b cookies.txt
```

**Execute Scan**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}'
```

---

## 🐛 Known Issues & TODO

### High Priority
1. **Fix Read-Only Collection Exception** in `ScanHistory` entity
   - File: `src/HeimdallWeb.Domain/Entities/ScanHistory.cs`
   - Change `IReadOnlyCollection<IASummary>` → `List<IASummary>`

2. **Add Global Exception Handling Middleware**
   - File: `src/HeimdallWeb.WebApi/Program.cs`
   - Map validation exceptions to HTTP 400
   - Map authentication failures to HTTP 401
   - Map business rule violations to HTTP 409

3. **Fix Logout Status Code**
   - Investigate why `Results.NoContent()` returns 200 instead of 204

### Medium Priority
4. Test admin endpoints with admin user
5. Test profile image upload
6. Test PDF export functionality
7. Test DELETE operations
8. Add integration tests for all endpoints

### Low Priority
9. Add request/response examples to Swagger docs
10. Add rate limiting headers to responses
11. Improve error messages in validation responses

---

## 📊 Testing Metrics

**Total Endpoints**: ~20  
**Tested**: 15  
**Passed**: 8 (53%)  
**Failed**: 7 (47%)  
**Not Tested**: 5

**Test Coverage by Category**:
- Authentication: 3/4 (75%)
- User Management: 3/5 (60%)
- Scans: 2/2 (100%)
- History: 0/6 (0%) - blocked by entity issue
- Dashboard/Admin: 0/4 (0%) - requires admin user

---

## 📚 Related Documentation

- [Migration Plan](../plano_migracao.md)
- [Architecture Guide](../docs/architecture/)
- [CLAUDE.md](../CLAUDE.md) - Development workflow rules

---

**Last Updated**: 2026-02-07 22:03:00 UTC  
**Tested By**: dotnet-backend-expert agent  
**Next Review**: After fixing critical issues
