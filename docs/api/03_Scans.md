# 🔍 Scan Endpoints

**Base URL**: `http://localhost:5110`  
**API Version**: v1  
**Last Updated**: 2026-02-08

---

## 📋 Endpoints Overview

| Method | Endpoint | Description | Auth Required | Rate Limit |
|--------|----------|-------------|---------------|------------|
| POST | `/api/v1/scans` | Execute security scan on target URL | ✅ Yes | 4/min per IP |
| GET | `/api/v1/scans` | List user's scan histories (paginated) | ✅ Yes | ❌ No |

---

## 1. POST /api/v1/scans

**Description**: Execute a comprehensive security scan on a target URL. The scan includes:
- SSL/TLS certificate validation
- HTTP security headers analysis
- Port scanning (common web ports)
- HTTP → HTTPS redirect check
- robots.txt analysis
- Sensitive path detection
- Technology detection
- AI-powered vulnerability analysis (Gemini API)

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: **4 requests per minute per IP** (ScanPolicy)  
**Authorization**: Any authenticated user

**Scan Duration**: Approximately 20-30 seconds (varies by target)

### Request

**Headers**:
```
Content-Type: application/json
Cookie: authHeimdallCookie=<token>
```

**Body**:
```json
{
  "target": "string"  // Required, valid URL (http:// or https://)
}
```

**Target Requirements** (Fail-Fast Validation):
- ✅ **Valid URL format**: Must be a properly formatted URL (e.g., `https://example.com`)
- ✅ **Public domain only**: Only public domains/URLs accepted (e.g., `example.com`, `api.example.com`)
- ✅ **Domain must exist**: DNS resolution check (rejects non-existent domains instantly)
- ✅ **Protocol**: HTTP or HTTPS (auto-added if omitted)
- ✅ **Custom ports**: Supported (e.g., `https://example.com:8443`)
- ❌ **IP addresses NOT allowed**: `8.8.8.8`, `192.168.1.1` rejected - system resolves IPs via DNS automatically
- ❌ **Localhost NOT allowed**: `localhost`, `127.0.0.1`, `http://localhost` all rejected
- ❌ **Maximum length**: 500 characters

**Validation is IMMEDIATE** (~200ms) - invalid domains are rejected **before** scan execution (no 75-second timeout).

**Error messages are SINGLE and SPECIFIC** - no duplicates, clear indication of what's wrong.

### Response Success

**Status**: `201 Created`

```json
{
  "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
  "target": "https://example.com",
  "summary": "O site example.com apresenta diversas configurações de segurança ausentes ou inadequadas...",
  "duration": "00:00:30.2018947",
  "hasCompleted": true,
  "createdDate": "2026-02-08T14:17:58.19036Z"
}
```

**Headers**:
- `Location: /api/v1/scan-histories/{historyId}`

**Fields**:
- `historyId`: Unique scan ID (UUID v7, use to retrieve detailed results)
- `target`: Normalized target URL
- `summary`: AI-generated summary (Portuguese, from Gemini API)
- `duration`: Scan execution time (format: `HH:MM:SS.mmmmmmm`)
- `hasCompleted`: `true` if scan finished successfully
- `createdDate`: ISO 8601 timestamp (UTC)

### Example curl (Happy Path)

```bash
# Execute scan on example.com
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}'
```

**Response**:
```json
{
  "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
  "target": "https://example.com",
  "summary": "O site example.com apresenta diversas configurações de segurança ausentes ou inadequadas, incluindo a falta de vários cabeçalhos de segurança críticos, a exposição de portas de painel de controle, e a ausência de redirecionamento HTTP para HTTPS. O certificado SSL é válido, mas próximo da expiração.",
  "duration": "00:00:30.2018947",
  "hasCompleted": true,
  "createdDate": "2026-02-08T14:17:58.19036Z"
}
```

**Retrieve full scan results**:
```bash
# Get detailed findings, technologies, and AI analysis
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be
```

**Different target examples**:
```bash
# Scan with specific port
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com:8443"}'

# Scan HTTP site (will check HTTPS redirect)
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"http://example.com"}'

# Scan subdomain
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://api.example.com"}'

# Scan domain without protocol (https:// auto-added)
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"example.com"}'
```

### Error Scenarios

#### 1. Invalid Domain (Non-Existent)

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://this-domain-does-not-exist-12345.com"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Target": [
      "Target domain does not exist or cannot be resolved. Please verify the domain is correct."
    ]
  }
}
```

**Why**: DNS resolution fails immediately (~200ms) - domain doesn't exist. This prevents wasting 75 seconds on an unreachable target.

---

#### 2. Invalid URL Format

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"not-a-valid-url"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Target": [
      "Target must be a valid URL or IP address",
      "Target domain does not exist or cannot be resolved. Please verify the domain is correct."
    ]
  }
}
```

---

#### 3. Empty Target

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":""}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Target": [
      "Target URL or IP address is required",
      "Target must be a valid URL or IP address",
      "Target domain does not exist or cannot be resolved. Please verify the domain is correct."
    ]
  }
}
```

---

#### 4. Target Too Long

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://very-long-domain-name-'$(python3 -c "print('a'*500)")'.com"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Target": [
      "Target URL is too long (max 500 characters)"
    ]
  }
}
```

---

#### 5. Localhost (Not Accepted)

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"localhost"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Target": [
      "Target must be a valid domain or URL (localhost and IP addresses not accepted)"
    ]
  }
}
```

**Why**: Localhost (and `127.0.0.1`, `http://localhost:5000`, etc.) is not accepted. Only public domains can be scanned. This prevents users from scanning internal/local services.

---

#### 6. IP Address (Not Accepted)

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"8.8.8.8"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Target": [
      "Target must be a valid domain or URL (localhost and IP addresses not accepted)"
    ]
  }
}
```

**Why**: IP addresses are not accepted as user input. The system resolves domain names to IP addresses automatically via DNS. Use domain names instead (e.g., `dns.google` instead of `8.8.8.8`).

---

#### 7. Not Authenticated

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}'
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 8. Rate Limit Exceeded (429)

**Scenario**: More than 4 scan requests in 1 minute from same IP

**Request** (5th request within 60 seconds):
```bash
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}'
```

**Expected Response**: `HTTP 429 Too Many Requests`

**Rate Limit Policy**:
- **Limit**: 4 requests per minute
- **Scope**: Per IP address
- **Window**: Sliding 60-second window
- **Named Policy**: `ScanPolicy`

**Recommendation**: Implement exponential backoff in clients:
```bash
# Wait 15 seconds between scans
for url in site1.com site2.com site3.com; do
  curl -X POST http://localhost:5110/api/v1/scans \
    -b cookies.txt \
    -H "Content-Type: application/json" \
    -d "{\"target\":\"https://$url\"}"
  sleep 15
done
```

---

## 2. GET /api/v1/scans

**Description**: Retrieve a paginated list of the authenticated user's scan histories.

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Users can only see their own scans

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**Query Parameters**:
- `page` (integer, optional, default: 1) - Page number (min: 1)
- `pageSize` (integer, optional, default: 10) - Items per page (min: 1, max: 100)

### Response Success

**Status**: `200 OK`

```json
{
  "items": [
    {
      "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
      "target": "example.com",
      "createdDate": "2026-02-08T14:17:58.076109Z",
      "duration": "00:00:30",
      "hasCompleted": true,
      "summary": "O site example.com apresenta diversas configurações...",
      "findingsCount": 11,
      "technologiesCount": 1
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Fields**:
- `items`: Array of scan summaries (ordered by `createdDate` DESC)
- `findingsCount`: Total vulnerabilities found
- `technologiesCount`: Total technologies detected
- `totalCount`: Total scans for this user
- `totalPages`: Total pages available
- `hasNextPage`: `true` if more pages exist
- `hasPreviousPage`: `true` if previous page exists

### Example curl (Happy Path)

```bash
# Get first page (10 items)
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=1&pageSize=10"
```

**Response**:
```json
{
  "items": [
    {
      "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
      "target": "example.com",
      "createdDate": "2026-02-08T14:17:58.076109Z",
      "duration": "00:00:30",
      "hasCompleted": true,
      "summary": "O site example.com apresenta diversas...",
      "findingsCount": 11,
      "technologiesCount": 1
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Custom pagination**:
```bash
# Get page 2 with 5 items per page
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=2&pageSize=5"
```

**Response**:
```json
{
  "items": [
    {"historyId": "019c3e5d-0b1a-7292-0001-000000000001", "target": "site6.com", ...},
    {"historyId": "019c3e5d-0b1a-7292-0002-000000000002", "target": "site7.com", ...},
    {"historyId": "019c3e5d-0b1a-7292-0003-000000000003", "target": "site8.com", ...},
    {"historyId": "019c3e5d-0b1a-7292-0004-000000000004", "target": "site9.com", ...},
    {"historyId": "019c3e5d-0b1a-7292-0005-000000000005", "target": "site10.com", ...}
  ],
  "page": 2,
  "pageSize": 5,
  "totalCount": 15,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

**Default parameters** (no query params):
```bash
# Uses defaults: page=1, pageSize=10
curl -b cookies.txt "http://localhost:5110/api/v1/scans"
```

**Workaround**:
```bash
# Always provide explicit parameters
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=1&pageSize=10"
```

### Error Scenarios

#### 1. Not Authenticated

**Request**:
```bash
curl "http://localhost:5110/api/v1/scans?page=1&pageSize=10"
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 2. Invalid Pagination Parameters

**Scenario**: Missing required parameters (current implementation)

**Request**:
```bash
# No query parameters
curl -b cookies.txt "http://localhost:5110/api/v1/scans"
```

**Response**: `HTTP 500 Internal Server Error`
```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred. Please try again later.",
  "errors": null
}
```

**Workaround**: Always provide explicit `page` and `pageSize`:
```bash
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=1&pageSize=10"
```

---

#### 3. Empty Results (No Scans)

**Scenario**: User has no scan histories

**Request**:
```bash
# New user with no scans
curl -b cookies.txt "http://localhost:5110/api/v1/scans?page=1&pageSize=10"
```

**Response**: `HTTP 200 OK`
```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Note**: Empty result is not an error - returns 200 with empty `items` array.

---

## 🔍 What Gets Scanned?

When you execute a scan, the following checks are performed:

### 1. SSL/TLS Certificate Analysis
- Certificate validity (expiration date)
- Certificate issuer and subject
- Certificate chain validation
- Common SSL vulnerabilities

### 2. HTTP Security Headers
Checks for presence and configuration of:
- `Strict-Transport-Security` (HSTS)
- `X-Content-Type-Options`
- `X-Frame-Options`
- `Content-Security-Policy` (CSP)
- `X-XSS-Protection`
- `Referrer-Policy`
- `Permissions-Policy`

### 3. Port Scanning
Scans common web ports:
- 80 (HTTP)
- 443 (HTTPS)
- 8080 (Alternative HTTP)
- 8443 (Alternative HTTPS)
- Control panel ports (cPanel, Plesk, etc.)

### 4. HTTP → HTTPS Redirect
- Checks if HTTP requests redirect to HTTPS
- Validates redirect status codes (301/302/307/308)

### 5. robots.txt Analysis
- Checks if `robots.txt` exists
- Analyzes disallowed paths for sensitive directories

### 6. Sensitive Path Detection
Scans for common sensitive files/directories:
- `.git/`, `.env`, `config.php`, `wp-config.php`
- Admin panels, backup files, database dumps

### 7. Technology Detection
Identifies technologies used:
- Web servers (Nginx, Apache, IIS)
- Frameworks (Laravel, React, Next.js)
- CDNs (Cloudflare, Akamai)
- CMS (WordPress, Drupal)

### 8. AI-Powered Analysis
- **Gemini API** analyzes all findings
- Generates natural language summary (Portuguese)
- Categorizes risks (Critical, High, Medium, Low)
- Provides remediation recommendations

---

## ⏱️ Scan Performance

### Typical Scan Duration
- **Fast sites** (~10s): Simple static sites, good response times
- **Average sites** (~20-30s): Most production sites
- **Slow sites** (~45-60s): Complex sites, slow DNS, multiple redirects
- **Timeout**: 75 seconds (scan aborts if exceeds)

### Optimization Tips
- Ensure target site is accessible (not blocked by firewall)
- Use HTTPS when possible (faster than HTTP + redirect check)
- Scan during low-traffic periods for faster response

---

## 📊 Rate Limiting Details

### ScanPolicy Configuration
```
Permit Limit: 4 requests
Window: 1 minute (60 seconds)
Scope: Per IP address
Queue Limit: 0 (reject immediately when limit exceeded)
```

### How It Works
1. First 4 requests within 60s → ✅ Allowed
2. 5th request within same 60s → ❌ Rejected (429)
3. After 60s from first request → Counter resets

### Best Practices
- **Batch scanning**: Add 15-second delays between scans
- **Monitor headers**: Check rate limit headers in response (if implemented)
- **Implement backoff**: Exponential backoff on 429 errors

**Example batch script**:
```bash
#!/bin/bash
targets=("site1.com" "site2.com" "site3.com" "site4.com" "site5.com")

for target in "${targets[@]}"; do
  echo "Scanning $target..."
  curl -X POST http://localhost:5110/api/v1/scans \
    -b cookies.txt \
    -H "Content-Type: application/json" \
    -d "{\"target\":\"https://$target\"}"
  
  # Wait 15 seconds to avoid rate limit (4/min = 1 every 15s)
  echo "Waiting 15s..."
  sleep 15
done
```

---

## 📌 Important Notes

### Scan Results Storage
- Scans are **permanently stored** in database
- Raw JSON results are saved (all scanner outputs)
- AI summaries are cached (no re-analysis on retrieval)
- Use `DELETE /api/v1/scan-histories/{uuid}` to remove

### Scan Ownership
- Users can **only see** their own scans
- Admins can see **all scans** via admin endpoints
- Scans are tied to user account (cascade delete on user deletion)

### Target URL Normalization
- Protocol is preserved (HTTP vs HTTPS)
- Port is removed if default (80 for HTTP, 443 for HTTPS)
- Trailing slashes are removed
- Query strings are removed

**Examples**:
- `https://example.com:443/` → `example.com`
- `http://example.com:80/path` → `example.com/path`
- `https://example.com:8443` → `example.com:8443`

### AI Summary Language
- Summaries are generated in **Portuguese (pt-BR)**
- Uses Google Gemini API (model: gemini-pro)
- Summary includes:
  - Overall security posture
  - Critical findings highlighted
  - Remediation priorities

### Failed Scans
- If scan fails (timeout, network error), `hasCompleted = false`
- Partial results may still be saved
- Duration shows time until failure
- Summary may be empty or incomplete

---

## ✅ Test Summary

All scan endpoints tested on **2026-02-08** (updated after fixes):

| Test Case | Expected | Actual | Status |
|-----------|----------|--------|--------|
| POST scan - Happy path | 201 Created | 201 Created | ✅ PASS |
| POST scan - Invalid URL | 400 Bad Request | 400 Bad Request | ✅ PASS |
| POST scan - Empty target | 400 Bad Request | 400 Bad Request | ✅ PASS |
| POST scan - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| GET scans - Happy path | 200 OK | 200 OK | ✅ PASS |
| GET scans - Pagination | 200 OK | 200 OK | ✅ PASS |
| GET scans - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| GET scans - No params | 200 OK (defaults) | 200 OK (defaults) | ✅ PASS |

**Success Rate**: 8/8 (100%) ✅  
**All Issues Fixed**: All validation errors now return proper HTTP status codes

### ✅ Issues Resolved (2026-02-08)

All previously documented issues have been fixed:

1. **✅ POST /api/v1/scans** - Invalid URL validation
   - **Fixed**: Now returns 400 Bad Request with validation message
   - **Implementation**: Enhanced URL validator with stricter domain requirements

2. **✅ POST /api/v1/scans** - Empty target validation
   - **Fixed**: Now returns 400 Bad Request with proper validation errors
   - **Implementation**: Added `ValidateAndThrowAsync()` in command handler

3. **✅ GET /api/v1/scans** - Default parameters
   - **Fixed**: Now uses defaults (page=1, pageSize=10) when params omitted
   - **Implementation**: Changed query params to nullable with default value handling

---

## 🔗 Related Documentation

- [History Endpoints](./04_History.md) - Retrieve detailed scan results
- [Authentication Endpoints](./01_Authentication.md)
- [Users Endpoints](./02_Users.md)
- [API Overview](./00_API_OVERVIEW.md)

---

**Last Tested**: 2026-02-08 14:18 UTC  
**Tested By**: DocuEngineer  
**Environment**: localhost:5110 (Development)  
**Scan Duration**: ~30 seconds (example.com)
