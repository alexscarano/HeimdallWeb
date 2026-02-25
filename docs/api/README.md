# 📚 HeimdallWeb API Documentation

Complete API documentation for HeimdallWeb security scanning application.

---

## 📖 Documentation Files

### Quick Start
- **[00_API_OVERVIEW.md](./00_API_OVERVIEW.md)** - Start here! General API information, authentication, rate limiting, error handling, common use cases

### Endpoint Documentation
- **[01_Authentication.md](./01_Authentication.md)** - Register, login, logout
- **[02_Users.md](./02_Users.md)** - Profile management, statistics, image upload, account deletion
- **[03_Scans.md](./03_Scans.md)** - Execute security scans, list histories
- **[04_History.md](./04_History.md)** - Detailed results, findings, technologies, PDF export
- **[05_Admin.md](./05_Admin.md)** - Admin dashboard, user management (admin only)

### Postman Collection
- **[HeimdallWeb_API_Collection.postman_collection.json](./HeimdallWeb_API_Collection.postman_collection.json)** - Import into Postman for testing

---

## 🚀 Quick Start Guide

### 1. Import Postman Collection

```bash
# Download collection
wget http://localhost:5110/docs/api/HeimdallWeb_API_Collection.postman_collection.json

# Or copy from this directory
cp HeimdallWeb_API_Collection.postman_collection.json ~/Downloads/
```

**Import in Postman:**
1. Open Postman
2. Click "Import" button
3. Select `HeimdallWeb_API_Collection.postman_collection.json`
4. Collection will appear in left sidebar

**Set Environment Variables:**
- `base_url`: `http://localhost:5110`
- `admin_email`: Your admin email
- `admin_password`: Your admin password

### 2. Using curl

**Register & Login:**
```bash
# 1. Register
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test@1234"}'

# 2. Login (save cookie)
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@1234"}'

# 3. Execute scan
curl -X POST http://localhost:5110/api/v1/scans \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"target":"https://example.com"}'
```

### 3. Authentication

**Two methods available:**

**A. Cookie-based (Recommended):**
```bash
# Login creates HttpOnly cookie
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -d '{"email":"user@example.com","password":"Pass@123"}'

# Use cookie in subsequent requests
curl -b cookies.txt http://localhost:5110/api/v1/scans?page=1&pageSize=10
```

**B. Bearer Token:**
```bash
# Get token from login response
TOKEN=$(curl -s -X POST http://localhost:5110/api/v1/auth/login \
  -d '{"email":"user@example.com","password":"Pass@123"}' | jq -r '.token')

# Use token in Authorization header
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5110/api/v1/scans?page=1&pageSize=10
```

---

## 📊 API Statistics

**Total Endpoints:** 20  
**Categories:** 5 (Auth, Users, Scans, History, Admin)  
**Authentication:** JWT (HttpOnly cookie or Bearer token)  
**Testing Status:** ✅ All endpoints tested (2026-02-08)

### Endpoints by Category

| Category | Endpoints | Auth Required | Admin Only |
|----------|-----------|---------------|------------|
| Authentication | 3 | ❌ (Login/Register only) | ❌ |
| Users | 5 | ✅ | ❌ |
| Scans | 2 | ✅ | ❌ |
| History | 6 | ✅ | ❌ |
| Admin | 4 | ✅ | ✅ |

---

## 🔍 What Can You Do?

### Regular Users
✅ Register and login  
✅ Execute security scans (rate limit: 4/minute)  
✅ View scan histories and detailed results  
✅ Export scans to PDF reports  
✅ Manage profile (update, upload image, delete account)  
✅ View personal statistics  

### Admin Users
✅ All regular user capabilities  
✅ View admin dashboard with system statistics  
✅ List and search all users  
✅ Activate/deactivate user accounts  
✅ Delete users without password  
✅ Access all scans (any user)  
✅ View system-wide audit logs  

---

## 🛠️ Development Tips

### Using jq for JSON Parsing

```bash
# Extract specific fields
curl -s -b cookies.txt http://localhost:5110/api/v1/scans?page=1&pageSize=10 \
  | jq '.items[].target'

# Filter findings by severity
curl -s -b cookies.txt http://localhost:5110/api/v1/scan-histories/16/findings \
  | jq '[.[] | select(.severity >= 3)]'

# Count total scans
curl -s -b cookies.txt http://localhost:5110/api/v1/scans?page=1&pageSize=1 \
  | jq '.totalCount'
```

### Batch Operations

```bash
# Scan multiple targets (respect rate limit)
TARGETS=("site1.com" "site2.com" "site3.com")
for target in "${TARGETS[@]}"; do
  curl -X POST http://localhost:5110/api/v1/scans \
    -b cookies.txt \
    -d "{\"target\":\"https://$target\"}"
  sleep 15  # 4/min = 15s between scans
done
```

### Error Handling

```bash
# Check response status code
STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
  -b cookies.txt http://localhost:5110/api/v1/scans?page=1&pageSize=10)

if [ $STATUS -eq 200 ]; then
  echo "Success"
elif [ $STATUS -eq 401 ]; then
  echo "Unauthorized - please login"
elif [ $STATUS -eq 429 ]; then
  echo "Rate limit exceeded - wait 60 seconds"
fi
```

---

## ✅ Quality Assurance

All endpoints tested and validated on **2026-02-08**.  
**No known issues** - API is production-ready.

### Previous Issues (All Fixed)

**Fixed on 2026-02-08:**

1. ✅ Validation errors were returning 500 → Now return proper 400 Bad Request
2. ✅ Missing query parameters caused 500 → Now use sensible defaults
3. ✅ Authorization errors returned 400 → Now return correct 403 Forbidden

**See detailed fixes in**: [Validation Issues Fix Summary](/tmp/fix_summary.md)

---

## 📌 Rate Limiting

### Global Limit
- **85 requests/minute** per IP (all endpoints)

### Scan Limit
- **4 requests/minute** per IP (POST /api/v1/scans only)
- **Recommended:** 15-second delay between scans

**Exceeded limit:**
```
HTTP 429 Too Many Requests
Retry-After: 60
```

---

## 📄 Response Formats

### Pagination Response
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Error Response
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password too weak"]
  }
}
```

### Success Response (Create)
```json
{
  "userId": 1,
  "username": "testuser",
  "email": "test@example.com",
  "token": "eyJhbGci...",
  "userType": 1
}
```

---

## 🔗 Related Documentation

### Project Documentation
- [Main README](../../README.md) - Project overview
- [Migration Plan](../../plano_migracao.md) - Architecture migration details
- [Testing Guides](../testing/) - Endpoint testing procedures

### External Resources
- [JWT.io](https://jwt.io) - Decode and verify JWT tokens
- [Postman Learning](https://learning.postman.com/) - Postman tutorials
- [curl Documentation](https://curl.se/docs/) - curl command reference

---

## 📞 Support

### Documentation Issues
If you find errors or missing information in this documentation:
1. Check the corresponding `.md` file for details
2. Test the endpoint using Postman Collection
3. Review error responses for clues

### API Issues
For API bugs or unexpected behavior:
1. Check [Known Issues](#-known-issues) section
2. Review server logs for errors
3. Verify authentication and authorization

---

## 📅 Last Updated

**Date:** 2026-02-08  
**API Version:** 1.0  
**Documentation Version:** 1.0.0  
**Tested Environment:** localhost:5110 (Development)

---

## ✅ Testing Summary

All 20 endpoints tested on **2026-02-08**:

| Category | Endpoints | Success Rate | Issues |
|----------|-----------|--------------|--------|
| Authentication | 3 | 100% ✅ | None |
| Users | 5 | 100% ✅ | None |
| Scans | 2 | 100% ✅ | None (all fixed) |
| History | 6 | 100% ✅ | None |
| Admin | 4 | 100% ✅ | None (all fixed) |

**Overall Success Rate:** 100% (20/20 fully compliant) ✅

---

**Happy Testing! 🚀**
