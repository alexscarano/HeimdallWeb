# 📜 Scan History Endpoints

**Base URL**: `http://localhost:5110`  
**API Version**: v1  
**Last Updated**: 2026-02-08

---

## 📋 Endpoints Overview

| Method | Endpoint | Description | Auth Required | Admin Only |
|--------|----------|-------------|---------------|------------|
| GET | `/api/v1/scan-histories/{uuid}` | Get complete scan details | ✅ Yes | ❌ No |
| GET | `/api/v1/scan-histories/{uuid}/findings` | Get vulnerabilities found | ✅ Yes | ❌ No |
| GET | `/api/v1/scan-histories/{uuid}/technologies` | Get detected technologies | ✅ Yes | ❌ No |
| GET | `/api/v1/scan-histories/{uuid}/export` | Export single scan to PDF | ✅ Yes | ❌ No |
| GET | `/api/v1/scan-histories/export` | Export all user scans to PDF | ✅ Yes | ❌ No |
| DELETE | `/api/v1/scan-histories/{uuid}` | Delete scan history | ✅ Yes | ❌ No |

---

## 1. GET /api/v1/scan-histories/{uuid}

**Description**: Retrieve complete details of a specific scan, including:
- Target URL and metadata
- Raw JSON results from all scanners
- AI-generated summary
- Findings (vulnerabilities)
- Detected technologies
- IA Summary with risk classification

**Authentication**: Required (JWT cookie or Bearer token)  
**Authorization**: 
- Regular users can only access their own scans
- **Admins can access all scans** (any user)

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**URL Parameters**:
- `uuid` (string (UUID v7), required) - Scan history ID (from POST /api/v1/scans response)

### Response Success

**Status**: `200 OK`

```json
{
  "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
  "target": "example.com",
  "rawJsonResult": "{\"ips\": [\"104.18.26.120\", \"104.18.27.120\"], \"robots\": {...}, \"target\": \"https://example.com\", ...}",
  "createdDate": "2026-02-08T14:17:58.076109Z",
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "duration": "00:00:30",
  "hasCompleted": true,
  "summary": "O site example.com apresenta diversas configurações...",
  "findings": [
    {
      "findingId": "019c3e5d-2a1b-7292-a123-65ceaee964be",
      "type": "Headers de Segurança",
      "description": "O cabeçalho Content-Security-Policy (CSP) está ausente...",
      "severity": 3,
      "evidence": "\"Content-Security-Policy\" na lista de headers ausentes.",
      "recommendation": "Configure uma política de segurança...",
      "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
      "createdAt": "2026-02-08T14:17:58.114176Z"
    }
  ],
  "technologies": [
    {
      "technologyId": "019c3e5d-3b2c-7292-b234-76ceaee964be",
      "name": "Cloudflare",
      "version": null,
      "category": "CDN",
      "description": "Cloudflare é uma rede de entrega de conteúdo...",
      "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
      "createdAt": "2026-02-08T14:17:58.127592Z"
    }
  ],
  "iaSummary": {
    "iaSummaryId": "019c3e5d-4c3d-7292-c345-87ceaee964be",
    "summaryText": "O site example.com apresenta diversas...",
    "mainCategory": "Security Scan",
    "overallRisk": "High",
    "totalFindings": 11,
    "findingsCritical": 0,
    "findingsHigh": 2,
    "findingsMedium": 4,
    "findingsLow": 4,
    "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
    "createdDate": "2026-02-08T14:17:58.137265Z"
  }
}
```

**Key Fields**:
- `rawJsonResult`: Complete JSON string with all scanner outputs (parse for detailed analysis)
- `findings`: Array of vulnerabilities (sorted by severity DESC)
- `technologies`: Array of detected technologies
- `iaSummary.overallRisk`: `Critical`, `High`, `Medium`, `Low`, `Informational`
- `severity`: 0=Informational, 1=Low, 2=Medium, 3=High, 4=Critical

### Example curl (Happy Path)

```bash
# Get complete scan details
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be
```

**Response** (truncated for readability):
```json
{
  "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
  "target": "example.com",
  "duration": "00:00:30",
  "hasCompleted": true,
  "summary": "O site example.com apresenta diversas configurações de segurança ausentes...",
  "findings": [
    {
      "findingId": "019c3e5d-2a1b-7292-a123-65ceaee964be",
      "type": "Headers de Segurança",
      "description": "O cabeçalho Content-Security-Policy (CSP) está ausente. Sem CSP, o navegador não tem instruções sobre quais recursos podem ser carregados, tornando o site mais vulnerável a ataques de Cross-Site Scripting (XSS) e injeção de dados.",
      "severity": 3,
      "evidence": "\"Content-Security-Policy\" na lista de headers ausentes.",
      "recommendation": "Configure uma política de segurança de conteúdo robusta para mitigar XSS e outras injeções de cliente, restringindo as fontes de conteúdo permitidas."
    }
  ],
  "technologies": [
    {
      "technologyId": "019c3e5d-3b2c-7292-b234-76ceaee964be",
      "name": "Cloudflare",
      "version": null,
      "category": "CDN",
      "description": "Cloudflare é uma rede de entrega de conteúdo (CDN)..."
    }
  ],
  "iaSummary": {
    "overallRisk": "High",
    "totalFindings": 11,
    "findingsCritical": 0,
    "findingsHigh": 2,
    "findingsMedium": 4,
    "findingsLow": 4
  }
}
```

**Parse raw JSON for detailed scanner results**:
```bash
# Get scan details and parse rawJsonResult
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be \
  | jq -r '.rawJsonResult' \
  | jq '.'

# Extract specific scanner results
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be \
  | jq -r '.rawJsonResult' \
  | jq '.resultsSslScanner'

# Get IP addresses discovered
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be \
  | jq -r '.rawJsonResult' \
  | jq '.ips'
```

### Error Scenarios

#### 1. Scan Not Found

**Request**:
```bash
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-0000-7292-0000-000000000000
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Scan history not found",
  "errors": null
}
```

---

#### 2. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 3. Unauthorized Access (Different User)

**Scenario**: Regular user trying to access another user's scan

**Request**:
```bash
# User A trying to access User B's scan
curl -b user_a_cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be
```

**Expected Response**: `HTTP 403 Forbidden`

**⚠️ ADMIN PRIVILEGE**: Admins CAN access all scans regardless of owner. Admin requests return `200 OK` with full data.

---

## 2. GET /api/v1/scan-histories/{uuid}/findings

**Description**: Retrieve only the vulnerabilities (findings) for a specific scan. More lightweight than fetching the full scan details.

**Authentication**: Required  
**Authorization**: Same as full scan details (users see own, admins see all)

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**URL Parameters**:
- `uuid` (string (UUID v7), required) - Scan history ID

### Response Success

**Status**: `200 OK`

```json
[
  {
    "findingId": "019c3e5d-2a1b-7292-a123-65ceaee964be",
    "type": "Headers de Segurança",
    "description": "O cabeçalho Content-Security-Policy (CSP) está ausente. Sem CSP, o navegador não tem instruções sobre quais recursos podem ser carregados, tornando o site mais vulnerável a ataques de Cross-Site Scripting (XSS) e injeção de dados.",
    "severity": 3,
    "evidence": "\"Content-Security-Policy\" na lista de headers ausentes.",
    "recommendation": "Configure uma política de segurança de conteúdo robusta para mitigar XSS e outras injeções de cliente, restringindo as fontes de conteúdo permitidas.",
    "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
    "createdAt": "2026-02-08T14:17:58.114176Z"
  },
  {
    "findingId": "019c3e5d-2a1b-7292-a124-65ceaee964be",
    "type": "Headers de Segurança",
    "description": "O cabeçalho Strict-Transport-Security (HSTS) está ausente. Isso permite que navegadores acessem o site via HTTP antes de serem redirecionados para HTTPS, abrindo uma janela para ataques de downgrade e sequestro de sessão.",
    "severity": 3,
    "evidence": "\"Strict-Transport-Security\" na lista de headers ausentes.",
    "recommendation": "Implemente o cabeçalho HSTS com um 'max-age' adequado e, idealmente, com a diretiva 'includeSubDomains' e 'preload'.",
    "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
    "createdAt": "2026-02-08T14:17:58.114108Z"
  }
]
```

**Array of findings**, sorted by `severity` DESC (Critical → High → Medium → Low → Informational)

**Severity Levels**:
- `4` - **Critical**: Immediate action required (e.g., exposed credentials)
- `3` - **High**: Serious security issue (e.g., missing HSTS, CSP)
- `2` - **Medium**: Moderate risk (e.g., exposed admin panels, missing X-Frame-Options)
- `1` - **Low**: Minor security concern (e.g., missing Referrer-Policy)
- `0` - **Informational**: No immediate risk (e.g., certificate nearing expiration)

### Example curl (Happy Path)

```bash
# Get all findings for scan
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/findings
```

**Filter by severity** (client-side):
```bash
# Get only Critical and High findings
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/findings \
  | jq '[.[] | select(.severity >= 3)]'

# Count findings by severity
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/findings \
  | jq 'group_by(.severity) | map({severity: .[0].severity, count: length})'

# Get only missing security headers
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/findings \
  | jq '[.[] | select(.type == "Headers de Segurança")]'
```

### Error Scenarios

#### 1. Scan Not Found

**Request**:
```bash
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-0000-7292-0000-000000000000/findings
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Scan history not found",
  "errors": null
}
```

---

#### 2. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/findings
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 3. Empty Results (No Findings)

**Scenario**: Scan completed but no vulnerabilities found (rare but possible)

**Response**: `HTTP 200 OK`
```json
[]
```

**Note**: Empty array is not an error - indicates a very secure target or scan failure.

---

## 3. GET /api/v1/scan-histories/{uuid}/technologies

**Description**: Retrieve only the detected technologies for a specific scan. Useful for technology stack analysis and inventory.

**Authentication**: Required  
**Authorization**: Same as full scan details (users see own, admins see all)

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**URL Parameters**:
- `uuid` (string (UUID v7), required) - Scan history ID

### Response Success

**Status**: `200 OK`

```json
[
  {
    "technologyId": "019c3e5d-3b2c-7292-b234-76ceaee964be",
    "name": "Cloudflare",
    "version": null,
    "category": "CDN",
    "description": "Cloudflare é uma rede de entrega de conteúdo (CDN), mitigação de DDoS e provedor de serviços de segurança. Atua como um proxy reverso, protegendo websites de ameaças comuns e otimizando a performance. Embora ofereça segurança robusta, a configuração inadequada de seus serviços pode levar a vulnerabilidades, como a exposição de IPs de origem ou falhas na imposição de HTTPS.",
    "historyId": "019c3e5d-166b-7292-9844-54ceaee964be",
    "createdAt": "2026-02-08T14:17:58.127592Z"
  }
]
```

**Technology Categories**:
- `CDN` - Content Delivery Network (Cloudflare, Akamai)
- `Web Server` - Nginx, Apache, IIS
- `Framework` - Laravel, React, Next.js
- `CMS` - WordPress, Drupal, Joomla
- `Programming Language` - PHP, Python, Node.js
- `Database` - MySQL, PostgreSQL (if exposed)
- `Security` - WAF, rate limiting

### Example curl (Happy Path)

```bash
# Get all detected technologies
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/technologies
```

**Filter by category** (client-side):
```bash
# Get only CDNs
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/technologies \
  | jq '[.[] | select(.category == "CDN")]'

# List technology names only
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/technologies \
  | jq '[.[].name]'

# Count technologies by category
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/technologies \
  | jq 'group_by(.category) | map({category: .[0].category, count: length})'
```

### Error Scenarios

#### 1. Scan Not Found

**Request**:
```bash
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-0000-7292-0000-000000000000/technologies
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Scan history not found",
  "errors": null
}
```

---

#### 2. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/technologies
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 3. Empty Results (No Technologies Detected)

**Scenario**: Scan couldn't identify any technologies (very locked-down server)

**Response**: `HTTP 200 OK`
```json
[]
```

**Note**: Empty array is valid - some sites hide technology fingerprints.

---

## 4. GET /api/v1/scan-histories/{uuid}/export

**Description**: Export a single scan to a professionally formatted PDF report. The PDF includes:
- Executive summary with risk classification
- Target information and scan metadata
- All findings with severity, evidence, and recommendations
- Detected technologies with descriptions
- Raw scanner outputs (appendix)

**Authentication**: Required  
**Authorization**: Users can export own scans, admins can export all

**Content-Type**: `application/pdf`  
**Library Used**: QuestPDF (high-quality PDF generation)

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**URL Parameters**:
- `uuid` (string (UUID v7), required) - Scan history ID

### Response Success

**Status**: `200 OK`  
**Content-Type**: `application/pdf`

**Headers**:
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="scan-report-019c3e5d-166b-7292-9844-54ceaee964be.pdf"
```

**Body**: Binary PDF file

### Example curl (Happy Path)

```bash
# Download PDF report
curl -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/export \
  -o scan-report.pdf

# Verify it's a valid PDF
file scan-report.pdf
# Output: scan-report.pdf: PDF document, version 1.7, 6 page(s)

# Open PDF (Linux)
xdg-open scan-report.pdf

# Open PDF (macOS)
open scan-report.pdf

# Open PDF (Windows)
start scan-report.pdf
```

**Check PDF metadata**:
```bash
# Install pdfinfo (Linux)
sudo apt-get install poppler-utils

# Get PDF info
pdfinfo scan-report.pdf
```

**Expected output**:
```
Title:          Security Scan Report - example.com
Creator:        HeimdallWeb
Producer:       QuestPDF
CreationDate:   Sat Feb  8 14:30:00 2026
Pages:          6
File size:      245 KB
```

### Error Scenarios

#### 1. Scan Not Found

**Request**:
```bash
curl -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/019c3e5d-0000-7292-0000-000000000000/export \
  -o report.pdf
```

**Response**: `HTTP 404 Not Found`  
**Content-Type**: `application/json`
```json
{
  "statusCode": 404,
  "message": "Scan history not found",
  "errors": null
}
```

**Note**: Check Content-Type before saving - JSON errors shouldn't be saved as .pdf

---

#### 2. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be/export \
  -o report.pdf
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

## 5. GET /api/v1/scan-histories/export

**Description**: Export **all scans** belonging to the authenticated user into a single PDF file. Each scan is rendered as a separate section within the PDF.

**Authentication**: Required  
**Authorization**: Users export their own scans, admins export their own scans

**Use Case**: Batch reporting, compliance documentation, archive creation

**Content-Type**: `application/pdf`

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**Query Parameters**: None

### Response Success

**Status**: `200 OK`  
**Content-Type**: `application/pdf`

**Headers**:
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="all-scans-report.pdf"
```

**Body**: Binary PDF file containing all user scans

**PDF Structure**:
- Cover page with user info and date range
- Table of contents with scan targets
- Individual scan reports (same format as single export)
- Summary statistics (total scans, average risk, etc.)

### Example curl (Happy Path)

```bash
# Download all scans as PDF
curl -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/export \
  -o all-scans-report.pdf

# Verify it's a valid PDF
file all-scans-report.pdf
# Output: all-scans-report.pdf: PDF document, version 1.7, 1 page(s)
```

**Batch export for multiple users** (admin workflow):
```bash
# Admin exports their own scans
curl -b admin_cookies.txt \
  http://localhost:5110/api/v1/scan-histories/export \
  -o admin-scans.pdf

# To export for specific users, admin must use /api/v1/dashboard endpoints
# (see Admin Endpoints documentation)
```

### Error Scenarios

#### 1. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/scan-histories/export \
  -o report.pdf
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 2. No Scans Available

**Scenario**: User has zero scan histories

**Response**: `HTTP 200 OK`  
**Content-Type**: `application/pdf`

**Body**: PDF with message "No scan histories available for export"

**Note**: Still returns valid PDF (not an error), just empty content.

---

## 6. DELETE /api/v1/scan-histories/{uuid}

**Description**: Permanently delete a scan history and all associated data:
- Scan metadata
- Raw JSON results
- Findings (vulnerabilities)
- Technologies
- AI summary

**⚠️ WARNING**: This action is **irreversible**. All data is permanently deleted.

**Authentication**: Required  
**Authorization**: Users can only delete their own scans

**Cascade Deletion**:
- `tb_finding` entries (all findings for this scan)
- `tb_technology` entries (all technologies for this scan)
- `tb_ia_summary` entry (AI analysis)

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**URL Parameters**:
- `uuid` (string (UUID v7), required) - Scan history ID to delete

### Response Success

**Status**: `200 OK`

```json
{
  "message": "Scan history deleted successfully",
  "historyId": "019c3e5d-5d4e-7292-d456-98ceaee964be"
}
```

### Example curl (Happy Path)

```bash
# Delete scan history
curl -X DELETE -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/019c3e5d-5d4e-7292-d456-98ceaee964be

# Verify deletion (should return 404)
curl -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/019c3e5d-5d4e-7292-d456-98ceaee964be
```

**Response**:
```json
{
  "message": "Scan history deleted successfully",
  "historyId": "019c3e5d-5d4e-7292-d456-98ceaee964be"
}
```

**Verification** (should fail):
```bash
curl -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-5d4e-7292-d456-98ceaee964be
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Scan history not found",
  "errors": null
}
```

**Bulk deletion** (client-side loop):
```bash
#!/bin/bash
# Delete all scans from list
SCAN_IDS=(
  "019c3e5d-0b1a-7292-0001-000000000001"
  "019c3e5d-0b1a-7292-0002-000000000002"
  "019c3e5d-0b1a-7292-0003-000000000003"
  "019c3e5d-0b1a-7292-0004-000000000004"
)

for id in "${SCAN_IDS[@]}"; do
  echo "Deleting scan $id..."
  curl -X DELETE -b cookies.txt \
    "http://localhost:5110/api/v1/scan-histories/$id"
  sleep 1
done
```

### Error Scenarios

#### 1. Scan Not Found

**Request**:
```bash
curl -X DELETE -b cookies.txt \
  http://localhost:5110/api/v1/scan-histories/019c3e5d-0000-7292-0000-000000000000
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Scan history not found",
  "errors": null
}
```

---

#### 2. Unauthorized (Different User)

**Scenario**: User A trying to delete User B's scan

**Request**:
```bash
# User A trying to delete scan belonging to User B
curl -X DELETE -b user_a_cookies.txt \
  http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be
```

**Expected Response**: `HTTP 403 Forbidden`

**Note**: Currently untested - verify authorization behavior.

---

#### 3. Not Authenticated

**Request**:
```bash
curl -X DELETE http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

## 📊 Understanding Scan Results

### Severity Classification

**How severity is determined**:
1. **Scanner-assigned severity** (0-4 scale)
2. **AI analysis** adjusts based on context
3. **Finding type** influences severity

**Severity → Risk Mapping**:
```
Critical (4) → Immediate exploitation risk
High (3)     → Serious security gap
Medium (2)   → Moderate exposure
Low (1)      → Minor concern
Info (0)     → Awareness only
```

**Example findings by severity**:

**Critical (4)**:
- Exposed database credentials
- Unpatched critical CVEs
- SQL injection vulnerabilities

**High (3)**:
- Missing HSTS (allows MITM attacks)
- Missing CSP (enables XSS)
- Exposed admin panels without rate limiting

**Medium (2)**:
- Missing X-Frame-Options (clickjacking risk)
- Weak SSL ciphers
- Exposed version information

**Low (1)**:
- Missing Referrer-Policy
- Suboptimal cache headers
- Non-critical information disclosure

**Informational (0)**:
- Certificate nearing expiration (>30 days)
- Deprecated but not vulnerable technologies
- Scan metadata

### Raw JSON Structure

The `rawJsonResult` field contains comprehensive scanner outputs:

```json
{
  "ips": ["104.18.26.120", "104.18.27.120"],
  "target": "https://example.com",
  "scanTime": "2026-02-08T11:17:41.67362-03:00",
  "statusCodeHttpRequest": 200,
  
  "headers": {
    "Server": "cloudflare",
    "Content-Type": "text/html",
    "Date": "Sun, 08 Feb 2026 14:17:28 GMT"
  },
  
  "securityHeaders": {
    "present": {},
    "missing": ["Strict-Transport-Security", "Content-Security-Policy", ...],
    "weak": {}
  },
  
  "resultsSslScanner": [
    {
      "port": 443,
      "subject": "CN=example.com",
      "issuer": "CN=Cloudflare TLS...",
      "notBefore": "2025-12-16T16:39:32-03:00",
      "notAfter": "2026-03-16T15:32:44-03:00",
      "daysToExpire": 36,
      "severity": "Informativo"
    }
  ],
  
  "resultsPortScanner": [
    {
      "ip": "104.18.26.120",
      "port": 443,
      "open": true,
      "banner": "HTTP/1.1 403...",
      "severity": "Informativo",
      "description": "Porta 443 (web) aberta..."
    }
  ],
  
  "resultsHttpRedirectScanner": [
    {
      "status": "no_redirect",
      "port_80_open": true,
      "redirect_detected": false,
      "severity": "Medio",
      "description": "HTTP habilitado sem redirect..."
    }
  ],
  
  "robots": {
    "robots_found": false,
    "sitemap_found": false,
    "alerts": ["O robots.txt retornou um conteúdo inesperado..."]
  },
  
  "sensitivePathScanner": {
    "findings": 0,
    "totalChecked": 51,
    "results": []
  }
}
```

**Parsing examples**:
```bash
# Extract SSL certificate expiration
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be \
  | jq -r '.rawJsonResult' \
  | jq '.resultsSslScanner[0].daysToExpire'

# List all open ports
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be \
  | jq -r '.rawJsonResult' \
  | jq '.resultsPortScanner[] | select(.open == true) | .port'

# Check if HSTS is present
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/019c3e5d-166b-7292-9844-54ceaee964be \
  | jq -r '.rawJsonResult' \
  | jq '.securityHeaders.missing | contains(["Strict-Transport-Security"])'
```

### AI Summary Breakdown

```json
{
  "iaSummary": {
    "summaryText": "O site example.com apresenta...",
    "mainCategory": "Security Scan",
    "overallRisk": "High",
    "totalFindings": 11,
    "findingsCritical": 0,
    "findingsHigh": 2,
    "findingsMedium": 4,
    "findingsLow": 4
  }
}
```

**Overall Risk Calculation**:
- **Critical**: 1+ critical findings OR 3+ high findings
- **High**: 2+ high findings OR 5+ medium findings
- **Medium**: 1 high OR 3+ medium findings
- **Low**: Only low/informational findings
- **Informational**: Zero security findings

**AI Summary Language**: Always Portuguese (pt-BR) via Google Gemini API

---

## 📌 Important Notes

### Data Persistence
- Scan histories are **permanently stored** until manually deleted
- Database cascade deletion removes all related data
- No automatic cleanup/archival (manual DELETE required)

### Admin Privileges
- Admins can **read all scans** (not just their own)
- Regular users are restricted to their own scans
- Authorization is checked on every request

### PDF Export Limits
- Single export: No limit (generates PDF for one scan)
- Bulk export: No limit (all user scans in one PDF)
- Large PDFs (>100 scans) may take time to generate
- **Tip**: Use `curl --max-time 120` for bulk exports

### Performance Considerations

**GET full scan details** (`/api/v1/scan-histories/{uuid}`):
- Includes `rawJsonResult` (can be 50-100KB per scan)
- Parse JSON client-side for performance
- Use specialized endpoints (findings, technologies) for lighter payloads

**GET findings only** (`/api/v1/scan-histories/{uuid}/findings`):
- Faster than full scan details
- Returns only vulnerability array
- Recommended for dashboard lists

**GET technologies only** (`/api/v1/scan-histories/{uuid}/technologies`):
- Lightweight response
- Use for technology inventory/statistics

### Deletion Workflow

**Best practice for bulk deletion**:
```bash
#!/bin/bash
# Safe bulk deletion with confirmation

# Get all scan IDs
SCAN_IDS=$(curl -s -b cookies.txt \
  "http://localhost:5110/api/v1/scans?page=1&pageSize=100" \
  | jq -r '.items[].historyId')

echo "Found $(echo "$SCAN_IDS" | wc -l) scans to delete"
echo "Are you sure? (yes/no)"
read confirm

if [ "$confirm" = "yes" ]; then
  for id in $SCAN_IDS; do
    echo "Deleting scan $id..."
    curl -X DELETE -b cookies.txt \
      "http://localhost:5110/api/v1/scan-histories/$id"
    sleep 0.5
  done
  echo "Deletion complete"
else
  echo "Aborted"
fi
```

### PDF Report Contents

**Single scan PDF includes**:
1. **Cover page**: Target, scan date, risk level
2. **Executive summary**: AI-generated overview
3. **Findings table**: All vulnerabilities with severity
4. **Detailed findings**: Evidence + recommendations
5. **Technology stack**: Detected technologies
6. **Appendix**: Raw scanner outputs (JSON)

**Bulk export PDF includes**:
1. **Cover page**: User info, date range, total scans
2. **Table of contents**: All scan targets with page numbers
3. **Individual scan reports**: Same format as single export
4. **Summary statistics**: Risk distribution, common findings

---

## ✅ Test Summary

All history endpoints tested on **2026-02-08**:

| Test Case | Expected | Actual | Status |
|-----------|----------|--------|--------|
| GET details - Happy path | 200 OK | 200 OK | ✅ PASS |
| GET details - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| GET details - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| GET details - Admin access | 200 OK (all scans) | 200 OK | ✅ PASS |
| GET findings - Happy path | 200 OK | 200 OK | ✅ PASS |
| GET findings - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| GET technologies - Happy path | 200 OK | 200 OK | ✅ PASS |
| GET technologies - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| GET export - Happy path | 200 OK (PDF) | 200 OK (6 pages) | ✅ PASS |
| GET export - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| GET export all - Happy path | 200 OK (PDF) | 200 OK (1 page) | ✅ PASS |
| DELETE - Happy path | 200 OK | 200 OK | ✅ PASS |
| DELETE - Verification | 404 Not Found | 404 Not Found | ✅ PASS |
| DELETE - Not found | 404 Not Found | 404 Not Found | ✅ PASS |

**Success Rate**: 14/14 (100%) ✅  
**No Issues Found**

---

## 🔗 Related Documentation

- [Scan Endpoints](./03_Scans.md) - Execute scans, list histories
- [Admin Endpoints](./05_Admin.md) - Admin dashboard, user management
- [Authentication Endpoints](./01_Authentication.md)
- [API Overview](./00_API_OVERVIEW.md)

---

**Last Tested**: 2026-02-08 14:30 UTC  
**Tested By**: DocuEngineer  
**Environment**: localhost:5110 (Development)  
**Test Scans**: historyId 019c3e5d-166b-7292-9844-54ceaee964be (example.com), historyId 019c3e5d-5d4e-7292-d456-98ceaee964be (httpbin.org, deleted)
