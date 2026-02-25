# Phase 3 - Application Layer Testing Guide

**Last Updated:** 2026-02-06
**Purpose:** Manual testing guide for all 19 Command and Query Handlers
**Pre-requisite:** Phase 4 (WebAPI) must be completed to test these handlers via HTTP endpoints

---

## Overview

This guide provides testing scenarios for all **19 handlers** (9 Commands + 10 Queries) implemented in Phase 3.

**Handler Types:**
- **Commands (9):** Write operations (create, update, delete)
- **Queries (10):** Read operations (get, list, export)

**Testing Approach:**
- All tests require WebAPI endpoints (Phase 4)
- Use Postman, curl, or REST Client extensions
- Validate both success and error scenarios
- Check response status codes, payloads, and side effects

---

## Authentication Setup

Most endpoints require authentication. First, obtain a JWT token:

### POST /api/v1/auth/login
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "admin@heimdall.com",
    "password": "Admin123!@#"
  }'
```

**Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "username": "admin",
  "userType": 2,
  "isActive": true
}
```

**Use the token in subsequent requests:**
```bash
Authorization: Bearer <token>
```

---

## Command Handlers (9 total)

### 1. LoginCommand

**Endpoint:** `POST /api/v1/auth/login`

**Handler:** `LoginCommandHandler`

**Request:**
```json
{
  "emailOrUsername": "user@example.com",
  "password": "Password123!@#"
}
```

**Success Scenarios:**

| Test Case | Input | Expected Response | Status Code |
|-----------|-------|-------------------|-------------|
| Valid email + password | Valid credentials | JWT token + user info | 200 OK |
| Valid username + password | Username instead of email | JWT token + user info | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Invalid email format | Validation error | 400 Bad Request |
| Wrong password | Authentication error | 401 Unauthorized |
| Non-existent user | Authentication error | 401 Unauthorized |
| Inactive user | Authentication error | 401 Unauthorized |
| Empty fields | Validation error | 400 Bad Request |

**Side Effects:**
- Audit log entry created (USER_LOGIN or USER_LOGIN_FAILED)

---

### 2. RegisterUserCommand

**Endpoint:** `POST /api/v1/auth/register`

**Handler:** `RegisterUserCommandHandler`

**Request:**
```json
{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "StrongPass123!@#"
}
```

**Success Scenarios:**

| Test Case | Input | Expected Response | Status Code |
|-----------|-------|-------------------|-------------|
| Valid registration | Unique username/email | JWT token + user info | 201 Created |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Duplicate username | Conflict error | 409 Conflict |
| Duplicate email | Conflict error | 409 Conflict |
| Weak password | Validation error | 400 Bad Request |
| Invalid email format | Validation error | 400 Bad Request |
| Username too short | Validation error | 400 Bad Request |

**Password Requirements:**
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

**Side Effects:**
- New user created in database
- JWT token generated
- Audit log entry (USER_REGISTERED)

---

### 3. ExecuteScanCommand

**Endpoint:** `POST /api/v1/scans`

**Handler:** `ExecuteScanCommandHandler`

**Request:**
```json
{
  "target": "https://example.com",
  "userId": 1
}
```

**Success Scenarios:**

| Test Case | Input | Expected Response | Status Code |
|-----------|-------|-------------------|-------------|
| Valid HTTP URL | http://example.com | Scan results with findings | 201 Created |
| Valid HTTPS URL | https://example.com | Scan results with findings | 201 Created |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Invalid URL format | Validation error | 400 Bad Request |
| Non-HTTP(S) protocol | Validation error | 400 Bad Request |
| User exceeds rate limit (5/day) | Rate limit error | 429 Too Many Requests |
| Inactive user | Forbidden error | 403 Forbidden |
| Non-existent user | Not found error | 404 Not Found |

**Side Effects:**
- ScanHistory record created
- 7 scanners executed (HeaderScanner, SslScanner, PortScanner, etc.)
- Findings and Technologies created
- AI analysis via Gemini API
- IASummary created
- UserUsage tracking updated
- Audit logs created (7 event types)

**Timeout:** 75 seconds max

---

### 4. UpdateUserCommand

**Endpoint:** `PUT /api/v1/users/{id}`

**Handler:** `UpdateUserCommandHandler`

**Request:**
```json
{
  "userId": 1,
  "username": "updateduser",
  "email": "updated@example.com",
  "profileImagePath": "/images/profile.jpg"
}
```

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Update username | Updated user info | 200 OK |
| Update email | Updated user info | 200 OK |
| Update profile image | Updated user info | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Duplicate username | Conflict error | 409 Conflict |
| Duplicate email | Conflict error | 409 Conflict |
| User not found | Not found error | 404 Not Found |
| Update other user's profile | Forbidden error | 403 Forbidden |

**Side Effects:**
- User record updated in database
- Audit log entry (USER_UPDATED)

---

### 5. DeleteUserCommand

**Endpoint:** `DELETE /api/v1/users/{id}`

**Handler:** `DeleteUserCommandHandler`

**Request:**
```json
{
  "userId": 1,
  "password": "CurrentPassword123!@#",
  "confirmDelete": true
}
```

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Valid deletion | Success message | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Wrong password | Unauthorized error | 401 Unauthorized |
| confirmDelete = false | Validation error | 400 Bad Request |
| User not found | Not found error | 404 Not Found |

**Side Effects:**
- User record deleted (CASCADE deletes related records)
- All scan histories deleted
- All findings deleted
- All technologies deleted
- Audit log entry (USER_DELETED)

---

### 6. UpdateProfileImageCommand

**Endpoint:** `POST /api/v1/users/{id}/profile-image`

**Handler:** `UpdateProfileImageCommandHandler`

**Request:**
```json
{
  "userId": 1,
  "profileImagePath": "/images/new-profile.jpg"
}
```

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Upload new image | Updated user with image path | 200 OK |
| Remove image (null path) | Updated user without image | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Invalid file path | Validation error | 400 Bad Request |
| User not found | Not found error | 404 Not Found |
| Update other user's image | Forbidden error | 403 Forbidden |

---

### 7. DeleteScanHistoryCommand

**Endpoint:** `DELETE /api/v1/scan-histories/{id}`

**Handler:** `DeleteScanHistoryCommandHandler`

**Request:**
```json
{
  "historyId": 1,
  "userId": 1
}
```

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Delete own scan | Success message | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Scan not found | Not found error | 404 Not Found |
| Delete other user's scan | Forbidden error | 403 Forbidden |

**Side Effects:**
- ScanHistory record deleted (CASCADE)
- Related findings deleted
- Related technologies deleted
- Audit log entry (SCAN_DELETED)

---

### 8. ToggleUserStatusCommand (Admin Only)

**Endpoint:** `PATCH /api/v1/admin/users/{id}/status`

**Handler:** `ToggleUserStatusCommandHandler`

**Request:**
```json
{
  "userId": 2,
  "isActive": false,
  "requestingUserId": 1
}
```

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Block user | Updated user status | 200 OK |
| Unblock user | Updated user status | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Non-admin user | Forbidden error | 403 Forbidden |
| User not found | Not found error | 404 Not Found |
| Block another admin | Validation error | 400 Bad Request |
| Block self | Validation error | 400 Bad Request |

**Side Effects:**
- User IsActive status updated
- Audit log entry (USER_STATUS_CHANGED)

---

### 9. DeleteUserByAdminCommand (Admin Only)

**Endpoint:** `DELETE /api/v1/admin/users/{id}`

**Handler:** `DeleteUserByAdminCommandHandler`

**Request:**
```json
{
  "userId": 2,
  "requestingUserId": 1
}
```

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Delete user as admin | Success message | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Non-admin user | Forbidden error | 403 Forbidden |
| User not found | Not found error | 404 Not Found |
| Delete another admin | Validation error | 400 Bad Request |
| Delete self | Validation error | 400 Bad Request |

**Side Effects:**
- User record deleted (CASCADE)
- All related records deleted
- Audit log entry (USER_DELETED_BY_ADMIN)

---

## Query Handlers (10 total)

### 10. GetScanHistoryByIdQuery

**Endpoint:** `GET /api/v1/scan-histories/{id}`

**Handler:** `GetScanHistoryByIdQueryHandler`

**Request Parameters:**
- `historyId` (path parameter)
- `requestingUserId` (from JWT token)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get own scan | Full scan details | 200 OK |
| Admin gets any scan | Full scan details | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Scan not found | Not found error | 404 Not Found |
| Get other user's scan | Forbidden error | 403 Forbidden |

**Response Structure:**
```json
{
  "historyId": 1,
  "target": "https://example.com",
  "rawJsonResult": "{...}",
  "createdDate": "2024-01-01T00:00:00Z",
  "userId": 1,
  "duration": "00:01:23",
  "hasCompleted": true,
  "summary": "Scan completed successfully",
  "findings": [
    {
      "findingId": 1,
      "type": "Security Header Missing",
      "description": "X-Frame-Options header not set",
      "severity": "Medium",
      "evidence": "Header not found in response",
      "recommendation": "Add X-Frame-Options: DENY"
    }
  ],
  "technologies": [
    {
      "technologyId": 1,
      "name": "Nginx",
      "version": "1.21.0",
      "category": "Web Server"
    }
  ],
  "iaSummary": {
    "summaryText": "Overall security posture is moderate...",
    "mainCategory": "Web Application",
    "overallRisk": "Medium",
    "totalFindings": 5
  }
}
```

---

### 11. GetUserScanHistoriesQuery

**Endpoint:** `GET /api/v1/users/{id}/scan-histories?page=1&pageSize=10`

**Handler:** `GetUserScanHistoriesQueryHandler`

**Request Parameters:**
- `userId` (path parameter)
- `page` (query parameter, default: 1)
- `pageSize` (query parameter, default: 10, max: 50)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get page 1 | Paginated list | 200 OK |
| Get page 2 | Paginated list | 200 OK |
| Large page size (50) | Max 50 items | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Page size > 50 | Clamped to 50 | 200 OK |
| User not found | Not found error | 404 Not Found |
| Negative page | Validation error | 400 Bad Request |

**Response Structure:**
```json
{
  "items": [
    {
      "historyId": 1,
      "target": "https://example.com",
      "createdDate": "2024-01-01T00:00:00Z",
      "duration": "00:01:23",
      "hasCompleted": true,
      "findingsCount": 5,
      "technologiesCount": 3
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### 12. GetFindingsByHistoryIdQuery

**Endpoint:** `GET /api/v1/scan-histories/{id}/findings`

**Handler:** `GetFindingsByHistoryIdQueryHandler`

**Request Parameters:**
- `historyId` (path parameter)
- `requestingUserId` (from JWT token)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get findings | List ordered by severity DESC | 200 OK |
| No findings | Empty array | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Scan not found | Not found error | 404 Not Found |
| Get other user's findings | Forbidden error | 403 Forbidden |

**Ordering:** Critical → High → Medium → Low → Informational

---

### 13. GetTechnologiesByHistoryIdQuery

**Endpoint:** `GET /api/v1/scan-histories/{id}/technologies`

**Handler:** `GetTechnologiesByHistoryIdQueryHandler`

**Request Parameters:**
- `historyId` (path parameter)
- `requestingUserId` (from JWT token)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get technologies | List ordered by category, name | 200 OK |
| No technologies | Empty array | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Scan not found | Not found error | 404 Not Found |
| Get other user's technologies | Forbidden error | 403 Forbidden |

**Ordering:** Category ASC, Name ASC

---

### 14. ExportHistoryPdfQuery

**Endpoint:** `GET /api/v1/users/{id}/scan-histories/export`

**Handler:** `ExportHistoryPdfQueryHandler`

**Request Parameters:**
- `userId` (path parameter)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Export all scans | PDF binary data | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| User not found | Not found error | 404 Not Found |
| No scan histories | Not found error | 404 Not Found |

**Response:**
```json
{
  "pdfData": "<base64-encoded-pdf>",
  "fileName": "Historico_2024-01-01_12-30-00.pdf",
  "contentType": "application/pdf",
  "fileSize": 1024000
}
```

**Headers:**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="Historico_2024-01-01_12-30-00.pdf"
```

---

### 15. ExportSingleHistoryPdfQuery

**Endpoint:** `GET /api/v1/scan-histories/{id}/export`

**Handler:** `ExportSingleHistoryPdfQueryHandler`

**Request Parameters:**
- `historyId` (path parameter)
- `requestingUserId` (from JWT token)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Export single scan | PDF binary data | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Scan not found | Not found error | 404 Not Found |
| Export other user's scan | Forbidden error | 403 Forbidden |

**Filename Format:** `Scan_<sanitized-target>_<timestamp>.pdf`

---

### 16. GetUserProfileQuery

**Endpoint:** `GET /api/v1/users/{id}/profile`

**Handler:** `GetUserProfileQueryHandler`

**Request Parameters:**
- `userId` (path parameter)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get own profile | User profile data | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| User not found | Not found error | 404 Not Found |

**Response Structure:**
```json
{
  "userId": 1,
  "username": "johndoe",
  "email": "john@example.com",
  "userType": 1,
  "isActive": true,
  "profileImage": "/images/profile.jpg",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

### 17. GetUserStatisticsQuery

**Endpoint:** `GET /api/v1/users/{id}/statistics`

**Handler:** `GetUserStatisticsQueryHandler`

**Request Parameters:**
- `userId` (path parameter)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get statistics | Comprehensive stats | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| User not found | Not found error | 404 Not Found |

**Response Structure:**
```json
{
  "totalScans": 25,
  "completedScans": 23,
  "incompleteScans": 2,
  "totalFindings": 125,
  "criticalFindings": 10,
  "highFindings": 30,
  "mediumFindings": 50,
  "lowFindings": 35,
  "averageDuration": "00:01:30",
  "lastScanDate": "2024-01-01T00:00:00Z"
}
```

---

### 18. GetAdminDashboardQuery (Admin Only)

**Endpoint:** `GET /api/v1/admin/dashboard`

**Handler:** `GetAdminDashboardQueryHandler`

**Request Parameters:**
- `requestingUserId` (from JWT token)
- `logPage` (query parameter, default: 1)
- `logPageSize` (query parameter, default: 10, max: 50)
- `logLevel` (query parameter, optional: Info, Warning, Error)
- `logStartDate` (query parameter, optional)
- `logEndDate` (query parameter, optional)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get dashboard | Comprehensive admin stats | 200 OK |
| Filter logs by level | Filtered logs | 200 OK |
| Filter logs by date | Filtered logs | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Non-admin user | Forbidden error | 403 Forbidden |

**Response Structure:**
```json
{
  "userStats": {
    "totalUsers": 100,
    "activeUsers": 95,
    "blockedUsers": 5,
    "adminUsers": 2,
    "regularUsers": 98
  },
  "scanStats": {
    "totalScans": 500,
    "completedScans": 480,
    "incompleteScans": 20,
    "totalFindings": 2500,
    "criticalFindings": 100,
    "highFindings": 500
  },
  "logs": {
    "items": [...],
    "page": 1,
    "totalCount": 1000
  },
  "recentActivity": [...],
  "scanTrend": [],
  "userRegistrationTrend": []
}
```

---

### 19. GetUsersQuery (Admin Only)

**Endpoint:** `GET /api/v1/admin/users`

**Handler:** `GetUsersQueryHandler`

**Request Parameters:**
- `requestingUserId` (from JWT token)
- `page` (query parameter, default: 1)
- `pageSize` (query parameter, default: 10, max: 50)
- `search` (query parameter, optional - username/email search)
- `isActive` (query parameter, optional - filter by status)
- `isAdmin` (query parameter, optional - filter by role)
- `startDate` (query parameter, optional)
- `endDate` (query parameter, optional)

**Success Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Get all users | Paginated user list | 200 OK |
| Search by username | Filtered list | 200 OK |
| Filter by active status | Filtered list | 200 OK |
| Filter by admin role | Filtered list | 200 OK |

**Error Scenarios:**

| Test Case | Expected Response | Status Code |
|-----------|-------------------|-------------|
| Non-admin user | Forbidden error | 403 Forbidden |

**Response Structure:**
```json
{
  "items": [
    {
      "userId": 1,
      "username": "johndoe",
      "email": "john@example.com",
      "userType": 1,
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "scanCount": 25,
      "findingsCount": 125
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10
}
```

---

## Testing Workflow

### 1. Setup
1. Start WebAPI project: `dotnet run --project src/HeimdallWeb.WebApi/`
2. Verify Swagger UI: `http://localhost:5000/swagger`
3. Ensure database is running (PostgreSQL)

### 2. Authentication
1. Create test users (Register endpoint)
2. Create admin user manually in database or via seed data
3. Login and obtain JWT token
4. Store token for subsequent requests

### 3. Command Testing
1. Test each command with valid data
2. Test each command with invalid data (validation)
3. Test each command with missing auth (401)
4. Test each command with wrong user (403)
5. Verify side effects (database changes, audit logs)

### 4. Query Testing
1. Create test data using commands
2. Test each query with valid parameters
3. Test each query with pagination (page 1, 2, 3)
4. Test each query with filters
5. Verify response structure and data accuracy

### 5. Integration Testing
1. Complete user flow: Register → Login → Scan → View Results → Export PDF
2. Admin flow: Login → View Dashboard → Manage Users
3. Edge cases: Rate limiting, timeouts, concurrent requests

---

## Test Data Setup

### Create Test Users

```bash
# Regular User
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "TestPass123!@#"
  }'

# Admin User (requires database seed or manual creation)
INSERT INTO tb_user (username, email, password, user_type, is_active, created_at)
VALUES ('admin', 'admin@heimdall.com', '<hashed-password>', 2, true, NOW());
```

### Create Test Scans

```bash
# Execute scan
curl -X POST http://localhost:5000/api/v1/scans \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "target": "https://example.com",
    "userId": 1
  }'
```

---

## Common Error Codes

| Status Code | Meaning | Common Causes |
|-------------|---------|---------------|
| 400 Bad Request | Validation error | Missing fields, invalid format, business rule violation |
| 401 Unauthorized | Authentication error | Invalid credentials, expired token, missing token |
| 403 Forbidden | Authorization error | Insufficient permissions, accessing other user's data |
| 404 Not Found | Resource not found | Invalid ID, deleted resource |
| 409 Conflict | Resource conflict | Duplicate username/email, concurrent modification |
| 429 Too Many Requests | Rate limit exceeded | User exceeded scan quota (5/day) |
| 500 Internal Server Error | Server error | Unhandled exception, database error, external API failure |

---

## Performance Expectations

| Operation | Expected Duration | Notes |
|-----------|-------------------|-------|
| Login | < 200ms | Includes password hashing verification |
| Register | < 500ms | Includes password hashing |
| Execute Scan | 10-75s | Depends on target responsiveness |
| Get Scan History | < 100ms | With pagination (10 items) |
| Get Findings | < 50ms | Ordered query |
| Export PDF | 1-5s | Depends on scan count |
| Admin Dashboard | < 500ms | Multiple aggregations |

---

## Troubleshooting

### Issue: Authentication fails with valid credentials
**Check:**
- JWT secret key is configured in appsettings.json
- Token expiration time
- Password hash matches

### Issue: Scan execution times out
**Check:**
- Target URL is accessible
- Firewall rules allow outbound connections
- Gemini API key is valid
- Timeout is set to 75s

### Issue: Rate limiting triggers prematurely
**Check:**
- UserUsage table is correctly tracking requests
- Date comparison is working (timezone issues)
- Admin users are exempt from rate limiting

### Issue: PDF export fails
**Check:**
- QuestPDF license is configured
- Font files are accessible
- Scan history has required data

---

## Next Steps After Phase 3 Testing

1. **Performance Testing** - Load test with multiple concurrent users
2. **Security Testing** - Penetration testing, XSS, SQL injection attempts
3. **Integration Testing** - Automated E2E tests with Playwright or Selenium
4. **Unit Testing** - Create xUnit tests for individual handlers
5. **Documentation** - Update Swagger/OpenAPI descriptions

---

**Remember:** This guide assumes Phase 4 (WebAPI) is complete. Update endpoint URLs and authentication mechanisms based on actual WebAPI implementation.
