# 🌐 HeimdallWeb API - Overview

**Version**: 1.0  
**Base URL**: `http://localhost:5110`  
**API Prefix**: `/api/v1`  
**Last Updated**: 2026-02-08

---

## 📖 Introduction

HeimdallWeb API provides comprehensive security scanning capabilities for web applications. The API enables:
- Automated security vulnerability scanning
- User authentication and authorization
- Scan history management
- PDF report generation
- Admin dashboard and user management

**Technology Stack**:
- ASP.NET Core 8.0 (Minimal APIs)
- PostgreSQL database
- JWT authentication (HttpOnly cookies)
- QuestPDF for report generation
- Google Gemini AI for vulnerability analysis

---

## 🔗 Quick Links

### API Documentation
- [01 - Authentication Endpoints](./01_Authentication.md) - Login, register, logout
- [02 - Users Endpoints](./02_Users.md) - Profile management, statistics, deletion
- [03 - Scans Endpoints](./03_Scans.md) - Execute scans, list histories
- [04 - History Endpoints](./04_History.md) - Detailed results, findings, technologies, PDF export
- [05 - Admin Endpoints](./05_Admin.md) - Dashboard, user management, status control

### Getting Started
```bash
# 1. Register a new account
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"yourname","email":"you@example.com","password":"YourPass@123"}'

# 2. Login to get JWT cookie
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","password":"YourPass@123"}'

# 3. Execute a security scan
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}'

# 4. Get scan results
curl -b cookies.txt http://localhost:5110/api/v1/scans?page=1&pageSize=10
```

---

## 🔐 Authentication

### Authentication Methods

**1. Cookie-based (Recommended)**:
- JWT token stored in HttpOnly cookie: `authHeimdallCookie`
- Automatically sent with every request
- Protected against XSS attacks
- Expires after 24 hours

**2. Bearer Token (Alternative)**:
- Include JWT in `Authorization` header
- Format: `Authorization: Bearer <token>`
- Useful for mobile apps, API clients

### How to Authenticate

**Step 1: Login**
```bash
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'
```

**Response**:
```json
{
  "userId": 1,
  "username": "yourname",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userType": 1
}
```

**Step 2: Use Cookie for Subsequent Requests**
```bash
# Cookie automatically included
curl -b cookies.txt http://localhost:5110/api/v1/users/1/profile
```

**Or Use Bearer Token**
```bash
# Extract token from login response
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Include in Authorization header
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5110/api/v1/users/1/profile
```

### Cookie Configuration

```
Name: authHeimdallCookie
HttpOnly: true
Secure: true (production only)
SameSite: Strict
Max-Age: 86400 seconds (24 hours)
Path: /
```

**Security Features**:
- **HttpOnly**: Prevents JavaScript access (XSS protection)
- **Secure**: HTTPS only in production
- **SameSite=Strict**: CSRF protection
- **24h expiration**: Automatic logout after 1 day

### JWT Token Structure

**Decoded payload**:
```json
{
  "sub": "1",                    // User ID
  "unique_name": "yourname",     // Username
  "email": "user@example.com",   // Email
  "role": "1",                   // UserType (1=Regular, 2=Admin)
  "exp": 1707401234,             // Expiration timestamp
  "iss": "HeimdallWeb",          // Issuer
  "aud": "HeimdallWebUsers"      // Audience
}
```

**Decode JWT** (bash):
```bash
# Extract payload (second segment)
TOKEN="eyJhbGci..."
echo $TOKEN | cut -d'.' -f2 | base64 -d 2>/dev/null | jq '.'
```

### Authentication Errors

| Status Code | Scenario | Solution |
|-------------|----------|----------|
| `401 Unauthorized` | No token provided | Login first (`POST /api/v1/auth/login`) |
| `401 Unauthorized` | Token expired | Login again to refresh token |
| `401 Unauthorized` | Invalid token | Clear cookies, login again |
| `403 Forbidden` | Insufficient privileges | Endpoint requires admin role |

---

## 🚦 Rate Limiting

### Global Rate Limit
- **Limit**: 85 requests per minute
- **Scope**: Per IP address
- **Window**: Sliding 60-second window
- **Applies to**: All endpoints

### Scan-Specific Rate Limit
- **Limit**: 4 requests per minute
- **Scope**: Per IP address
- **Window**: Sliding 60-second window
- **Applies to**: `POST /api/v1/scans` only
- **Policy Name**: `ScanPolicy`

### Rate Limit Behavior

**When limit exceeded**:
```
HTTP 429 Too Many Requests
Retry-After: 60
```

**Best Practices**:
1. **Add delays between scans**:
   ```bash
   # Safe batch scanning (4 scans = 3 × 15s delays)
   curl -X POST http://localhost:5110/api/v1/scans -b cookies.txt -d '{"target":"site1.com"}'
   sleep 15
   curl -X POST http://localhost:5110/api/v1/scans -b cookies.txt -d '{"target":"site2.com"}'
   sleep 15
   curl -X POST http://localhost:5110/api/v1/scans -b cookies.txt -d '{"target":"site3.com"}'
   ```

2. **Implement exponential backoff**:
   ```bash
   retry_count=0
   max_retries=3
   
   while [ $retry_count -lt $max_retries ]; do
     response=$(curl -X POST http://localhost:5110/api/v1/scans -b cookies.txt -d '{"target":"example.com"}')
     if [ $? -eq 0 ]; then
       break
     fi
     sleep $((2 ** retry_count))
     retry_count=$((retry_count + 1))
   done
   ```

3. **Monitor rate limit headers** (if implemented):
   - `X-RateLimit-Limit`: Total allowed requests
   - `X-RateLimit-Remaining`: Remaining requests
   - `X-RateLimit-Reset`: Timestamp when limit resets

---

## 📊 Pagination

### Standard Pagination

**Query Parameters**:
- `page` (integer, default: 1, min: 1)
- `pageSize` (integer, default: 10, min: 1, max: 100)

**Example Request**:
```bash
curl -b cookies.txt \
  "http://localhost:5110/api/v1/scans?page=2&pageSize=20"
```

**Response Structure**:
```json
{
  "items": [...],
  "page": 2,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

**Navigation**:
```bash
# First page
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=1&pageSize=10"

# Next page
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=2&pageSize=10"

# Last page calculation
TOTAL_PAGES=$(curl -s -b cookies.txt "http://localhost:5110/api/v1/scans?page=1&pageSize=10" | jq '.totalPages')
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=$TOTAL_PAGES&pageSize=10"
```

### Endpoints with Pagination

| Endpoint | Default pageSize | Max pageSize |
|----------|-----------------|--------------|
| `GET /api/v1/scans` | 10 | 100 |
| `GET /api/v1/dashboard/users` | 10 | 100 |
| `GET /api/v1/dashboard/admin` (logs) | 10 | 50 |

---

## ❌ Error Handling

### Standard Error Response

All errors follow this structure:
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

**Fields**:
- `statusCode`: HTTP status code
- `message`: Human-readable error description
- `errors`: Object with field-specific validation errors (or `null`)

### HTTP Status Codes

| Code | Meaning | Common Causes |
|------|---------|---------------|
| `200 OK` | Success | Request completed successfully |
| `201 Created` | Resource created | User registered, scan executed |
| `204 No Content` | Success, no body | User deleted by admin |
| `400 Bad Request` | Validation error | Invalid input, missing required fields |
| `401 Unauthorized` | Authentication required | No token, expired token, invalid credentials |
| `403 Forbidden` | Insufficient privileges | Regular user accessing admin endpoint |
| `404 Not Found` | Resource not found | User/scan doesn't exist |
| `409 Conflict` | Resource conflict | Duplicate email/username |
| `429 Too Many Requests` | Rate limit exceeded | Too many scans/requests |
| `500 Internal Server Error` | Server error | Unexpected error (check logs) |

### Validation Errors (400)

**Example: Registration with invalid data**
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"ab","email":"invalid","password":"weak"}'
```

**Response**:
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "username": ["Username must be at least 3 characters"],
    "email": ["Invalid email format"],
    "password": ["Password must contain uppercase, lowercase, number, and special character"]
  }
}
```

### Common Error Scenarios

**1. Missing Authentication**:
```bash
curl http://localhost:5110/api/v1/scans
# Response: HTTP 401 (empty body)
```

**2. Invalid Token**:
```bash
curl -H "Authorization: Bearer invalid_token" \
  http://localhost:5110/api/v1/scans
# Response: HTTP 401
```

**3. Admin Endpoint as Regular User**:
```bash
curl -b regular_user_cookies.txt \
  http://localhost:5110/api/v1/dashboard/admin
# Response: HTTP 403
# {"statusCode":403,"message":"Admin access required","errors":null}
```

**4. Resource Not Found**:
```bash
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/99999
# Response: HTTP 404
# {"statusCode":404,"message":"Scan history with ID 99999 not found","errors":null}
```

**5. Duplicate Email**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","email":"existing@example.com","password":"Pass@123"}'
# Response: HTTP 409
# {"statusCode":409,"message":"Email already registered","errors":null}
```

---

## 📂 Data Models

### User Model

```json
{
  "userId": 1,
  "username": "alexandrescarano",
  "email": "alexandrescarano@gmail.com",
  "userType": 2,
  "isActive": true,
  "profileImage": "uploads/profiles/1_20260207230250.jpg",
  "createdAt": "2026-02-06T19:41:42.60754Z"
}
```

**Fields**:
- `userType`: `1` = Regular, `2` = Admin
- `isActive`: `true` = Active, `false` = Blocked
- `profileImage`: Relative path or `null`

### Scan History Model

```json
{
  "historyId": 16,
  "target": "example.com",
  "createdDate": "2026-02-08T14:17:58.076109Z",
  "duration": "00:00:30",
  "hasCompleted": true,
  "summary": "O site example.com apresenta diversas...",
  "findingsCount": 11,
  "technologiesCount": 1
}
```

**Fields**:
- `hasCompleted`: `true` = Scan finished, `false` = Failed/timeout
- `duration`: Format `HH:MM:SS`
- `summary`: AI-generated Portuguese text

### Finding Model

```json
{
  "findingId": 56,
  "type": "Headers de Segurança",
  "description": "O cabeçalho Content-Security-Policy (CSP) está ausente...",
  "severity": 3,
  "evidence": "\"Content-Security-Policy\" na lista de headers ausentes.",
  "recommendation": "Configure uma política de segurança...",
  "historyId": 16,
  "createdAt": "2026-02-08T14:17:58.114176Z"
}
```

**Severity Levels**:
- `4` - Critical
- `3` - High
- `2` - Medium
- `1` - Low
- `0` - Informational

### Technology Model

```json
{
  "technologyId": 28,
  "name": "Cloudflare",
  "version": null,
  "category": "CDN",
  "description": "Cloudflare é uma rede de entrega...",
  "historyId": 16,
  "createdAt": "2026-02-08T14:17:58.127592Z"
}
```

**Categories**: `CDN`, `Web Server`, `Framework`, `CMS`, `Programming Language`, `Database`, `Security`

---

## 🎯 Common Use Cases

### 1. User Registration & First Scan

```bash
#!/bin/bash

# 1. Register
REGISTER=$(curl -s -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","email":"newuser@example.com","password":"Secure@123"}')

USER_ID=$(echo $REGISTER | jq -r '.userId')
echo "Registered user ID: $USER_ID"

# 2. Login
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"newuser@example.com","password":"Secure@123"}'

# 3. Execute scan
SCAN=$(curl -s -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}')

HISTORY_ID=$(echo $SCAN | jq -r '.historyId')
echo "Scan started with ID: $HISTORY_ID"

# 4. Wait for scan to complete (30 seconds)
echo "Waiting for scan to complete..."
sleep 30

# 5. Get results
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/$HISTORY_ID
```

### 2. Batch Scanning Multiple Targets

```bash
#!/bin/bash
TARGETS=(
  "https://example.com"
  "https://google.com"
  "https://github.com"
)

# Login
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'

# Scan each target (respect rate limit: 4/min = 15s intervals)
for target in "${TARGETS[@]}"; do
  echo "Scanning $target..."
  
  SCAN=$(curl -s -X POST http://localhost:5110/api/v1/scans \
    -b cookies.txt \
    -H "Content-Type: application/json" \
    -d "{\"target\":\"$target\"}")
  
  HISTORY_ID=$(echo $SCAN | jq -r '.historyId')
  echo "→ Scan ID: $HISTORY_ID"
  
  # Wait 15 seconds to avoid rate limit
  sleep 15
done

echo "All scans queued. Wait 30-60 seconds for completion."
```

### 3. Export All Scans to PDF

```bash
#!/bin/bash

# Login
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'

# Export all scans to PDF
curl -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/export \
  -o all-scans-$(date +%Y%m%d).pdf

echo "Exported to all-scans-$(date +%Y%m%d).pdf"
```

### 4. Admin: Monitor System Health

```bash
#!/bin/bash

# Admin login
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c admin_cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin@123"}'

# Get dashboard stats
DASHBOARD=$(curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=20")

# Extract key metrics
echo "=== SYSTEM HEALTH ==="
echo "Total Users: $(echo $DASHBOARD | jq '.userStats.totalUsers')"
echo "Active Users: $(echo $DASHBOARD | jq '.userStats.activeUsers')"
echo "Total Scans: $(echo $DASHBOARD | jq '.scanStats.totalScans')"
echo "Critical Findings: $(echo $DASHBOARD | jq '.scanStats.criticalFindings')"

# Check for recent errors
ERRORS=$(echo $DASHBOARD | jq '.logs.items[] | select(.level == "Error")')
if [ -n "$ERRORS" ]; then
  echo ""
  echo "⚠️ RECENT ERRORS:"
  echo "$ERRORS" | jq '{timestamp, source, message}'
fi
```

### 5. Find High-Risk Scans

```bash
#!/bin/bash

# Login
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'

# Get all scans
SCANS=$(curl -s -b cookies.txt \
  "http://localhost:5110/api/v1/scans?page=1&pageSize=100")

# Filter scans with >5 findings (high risk)
echo "$SCANS" | jq '.items[] | select(.findingsCount > 5) | {target, findingsCount, createdDate}'
```

---

## 🔧 Development Tips

### Using curl with Sessions

**Save cookies to file**:
```bash
# Login and save cookies
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'

# Reuse cookies in subsequent requests
curl -b cookies.txt http://localhost:5110/api/v1/scans?page=1&pageSize=10
```

### Testing with jq

**Extract specific fields**:
```bash
# Get only usernames from user list
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100" \
  | jq -r '.users[].username'

# Count findings by severity
curl -s -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/16/findings \
  | jq 'group_by(.severity) | map({severity: .[0].severity, count: length})'
```

### Debugging Requests

**Verbose output**:
```bash
# Show full HTTP headers and response
curl -v -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass@123"}'
```

**Save response for inspection**:
```bash
# Save response body and headers
curl -D headers.txt -o response.json \
  -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/16
```

---

## 📋 Endpoint Summary

### Authentication (3 endpoints)
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login and get JWT token
- `POST /api/v1/auth/logout` - Logout and clear cookie

### Users (5 endpoints)
- `GET /api/v1/users/{id}/profile` - Get user profile
- `GET /api/v1/users/{id}/statistics` - Get user statistics
- `PUT /api/v1/users/{id}` - Update user profile
- `POST /api/v1/users/{id}/profile-image` - Upload profile image
- `DELETE /api/v1/users/{id}` - Delete user account

### Scans (2 endpoints)
- `POST /api/v1/scans` - Execute security scan
- `GET /api/v1/scans` - List scan histories (paginated)

### History (6 endpoints)
- `GET /api/v1/scan-histories/{id}` - Get scan details
- `GET /api/v1/scan-histories/{id}/findings` - Get vulnerabilities
- `GET /api/v1/scan-histories/{id}/technologies` - Get detected technologies
- `GET /api/v1/scan-histories/{id}/export` - Export scan to PDF
- `GET /api/v1/scan-histories/export` - Export all scans to PDF
- `DELETE /api/v1/scan-histories/{id}` - Delete scan history

### Admin (4 endpoints)
- `GET /api/v1/dashboard/admin` - Admin dashboard stats
- `GET /api/v1/dashboard/users` - List all users (admin)
- `PATCH /api/v1/admin/users/{id}/status` - Toggle user status
- `DELETE /api/v1/admin/users/{id}` - Delete user by admin

**Total: 20 endpoints**

---

## ✅ API Status

**All endpoints operational** - No known issues.  
**Testing**: 100% success rate (20/20 endpoints fully compliant)  
**Last Validated**: 2026-02-08

### Previous Issues (All Resolved)

Fixed on **2026-02-08**:

1. ✅ **Validation errors returning 500** → Now return proper 400 Bad Request
2. ✅ **Missing query parameters causing 500** → Now use sensible defaults  
3. ✅ **Authorization errors returning 400** → Now return correct 403 Forbidden

**Full details**: See [VALIDATION_FIXES_2026-02-08.md](./VALIDATION_FIXES_2026-02-08.md)

---

## 📞 Support & Resources

### Documentation Files
- **00_API_OVERVIEW.md** (this file) - General API information
- **01_Authentication.md** - Auth endpoints with examples
- **02_Users.md** - User management endpoints
- **03_Scans.md** - Scan execution and listing
- **04_History.md** - Detailed scan results
- **05_Admin.md** - Admin dashboard and user management

### Testing
- All endpoints tested on **2026-02-08**
- Test environment: `localhost:5110` (Development)
- Postman Collection: Available in this directory

### Best Practices
1. **Always use HTTPS in production**
2. **Implement rate limit backoff** (exponential or fixed delay)
3. **Validate input client-side** before API calls
4. **Handle 401 errors** by re-authenticating
5. **Store JWT securely** (HttpOnly cookies preferred)
6. **Monitor rate limits** to avoid 429 errors
7. **Parse raw JSON results** for detailed scan data
8. **Export compliance reports** before user deletion

---

**API Version**: 1.0  
**Documentation Version**: 1.0.0  
**Last Updated**: 2026-02-08  
**Maintainer**: HeimdallWeb Team
