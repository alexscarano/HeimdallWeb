# Admin Endpoints - Testing Guide

**Date Created**: 2026-02-07  
**API Version**: v1  
**Base URL**: `http://localhost:5110`

---

## 📋 Test Summary

| Endpoint | Method | Status | HTTP Code |
|----------|--------|--------|-----------|
| Admin Dashboard | GET /api/v1/dashboard/admin | ✅ PASSED | 200 |
| Users List | GET /api/v1/dashboard/users | ✅ PASSED | 200 |
| Toggle User Status | PATCH /api/v1/admin/users/{id}/status | ✅ PASSED | 200 |
| Delete User | DELETE /api/v1/admin/users/{id} | ✅ PASSED | 204 |

**Overall Result**: ✅ **4/4 endpoints passing (100%)**

---

## 🔐 Prerequisites

### Admin User Credentials
To test admin endpoints, you must authenticate as an admin user (UserType = 2).

**Test Admin Account**:
- Email: `alexandrescarano@gmail.com`
- Password: `Admin@123`
- UserType: `2` (Admin)

### Authentication
All admin endpoints require JWT authentication via HttpOnly cookie.

```bash
# Login as admin
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrLogin": "alexandrescarano@gmail.com",
    "password": "Admin@123"
  }'

# Use cookies in subsequent requests
curl -b cookies.txt http://localhost:5110/api/v1/dashboard/admin
```

---

## 📊 1. GET /api/v1/dashboard/admin - Admin Dashboard Statistics

**Purpose**: Retrieve comprehensive dashboard statistics for admins including user stats, scan stats, and recent logs.

### Request

```bash
GET /api/v1/dashboard/admin?logPage=1&logPageSize=10
Authorization: Required (Admin only)
```

**Query Parameters**:
- `logPage` (optional, default: 1) - Page number for logs pagination
- `logPageSize` (optional, default: 10, max: 50) - Number of logs per page
- `logLevel` (optional) - Filter logs by level (Info, Warning, Error)
- `logStartDate` (optional) - Filter logs from date (ISO 8601)
- `logEndDate` (optional) - Filter logs to date (ISO 8601)

### Response

**Status**: `200 OK`

```json
{
  "userStats": {
    "totalUsers": 6,
    "activeUsers": 6,
    "blockedUsers": 0,
    "adminUsers": 1,
    "regularUsers": 5
  },
  "scanStats": {
    "totalScans": 10,
    "completedScans": 9,
    "incompleteScans": 1,
    "totalFindings": 32,
    "criticalFindings": 3,
    "highFindings": 8,
    "mediumFindings": 8,
    "lowFindings": 5,
    "infoFindings": 8
  },
  "logs": {
    "items": [
      {
        "logId": 102,
        "timestamp": "2026-02-07T22:28:24.000Z",
        "level": "Info",
        "source": "RegisterUserCommandHandler",
        "message": "New user registered",
        "code": "USER_REGISTERED",
        "userId": 6,
        "username": "admintestuser",
        "remoteIp": "127.0.0.1"
      }
    ],
    "totalCount": 102,
    "page": 1,
    "pageSize": 10,
    "totalPages": 11
  }
}
```

### Error Scenarios

| Scenario | HTTP Code | Response |
|----------|-----------|----------|
| Not authenticated | 401 | `{"statusCode":401,"message":"Unauthorized"}` |
| Not admin (UserType != 2) | 403 | `{"statusCode":403,"message":"Insufficient permissions"}` |

### Test Example

```bash
curl -b cookies.txt "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=10" | jq '.'
```

---

## 👥 2. GET /api/v1/dashboard/users - Users List with Filtering

**Purpose**: Retrieve paginated list of all users with advanced filtering options.

### Request

```bash
GET /api/v1/dashboard/users?page=1&pageSize=10&search=john&isActive=true&isAdmin=false
Authorization: Required (Admin only)
```

**Query Parameters**:
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page
- `search` (optional) - Search by username or email
- `isActive` (optional) - Filter by active status (true/false)
- `isAdmin` (optional) - Filter by admin status (true/false)
- `createdFrom` (optional) - Filter users created after date (ISO 8601)
- `createdTo` (optional) - Filter users created before date (ISO 8601)

### Response

**Status**: `200 OK`

```json
{
  "users": [
    {
      "userId": 6,
      "username": "admintestuser",
      "email": "admintest@example.com",
      "userType": 1,
      "isActive": true,
      "profileImage": null,
      "createdAt": "2026-02-07T22:28:24.688Z",
      "scanCount": 0,
      "findingsCount": 0
    },
    {
      "userId": 5,
      "username": "fixtest123",
      "email": "fixtest@example.com",
      "userType": 1,
      "isActive": true,
      "profileImage": "data:image/png;base64,...",
      "createdAt": "2026-02-07T22:14:20.816Z",
      "scanCount": 1,
      "findingsCount": 0
    }
  ],
  "totalCount": 6,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**UserType Values**:
- `1` - Regular User
- `2` - Admin User

### Filter Examples

**Get all active users**:
```bash
curl -b cookies.txt "http://localhost:5110/api/v1/dashboard/users?isActive=true"
```

**Search for specific user**:
```bash
curl -b cookies.txt "http://localhost:5110/api/v1/dashboard/users?search=john"
```

**Get only admin users**:
```bash
curl -b cookies.txt "http://localhost:5110/api/v1/dashboard/users?isAdmin=true"
```

**Get users created in last week**:
```bash
WEEK_AGO=$(date -u -d '7 days ago' +%Y-%m-%dT%H:%M:%SZ)
curl -b cookies.txt "http://localhost:5110/api/v1/dashboard/users?createdFrom=$WEEK_AGO"
```

---

## 🔄 3. PATCH /api/v1/admin/users/{id}/status - Toggle User Active/Inactive

**Purpose**: Activate or deactivate a user account. Inactive users cannot log in.

### Request

```bash
PATCH /api/v1/admin/users/{id}/status
Authorization: Required (Admin only)
Content-Type: application/json

{
  "isActive": false
}
```

**Path Parameters**:
- `id` (required) - User ID to toggle status

**Request Body**:
```json
{
  "isActive": false  // true to activate, false to deactivate
}
```

### Response

**Status**: `200 OK`

```json
{
  "userId": 6,
  "username": "admintestuser",
  "isActive": false
}
```

### Business Rules

1. **Cannot deactivate self**: Admins cannot deactivate their own account
2. **Cannot toggle other admins**: Regular admins cannot deactivate other admin accounts
3. **Inactive users cannot login**: Deactivated users get `403 Forbidden` on login attempt

### Error Scenarios

| Scenario | HTTP Code | Response |
|----------|-----------|----------|
| Try to deactivate self | 400 | `{"statusCode":400,"message":"Validation failed for UserType: Cannot block/unblock admin users"}` |
| Try to toggle another admin | 400 | `{"statusCode":400,"message":"Validation failed for UserType: Cannot block/unblock admin users"}` |
| User not found | 404 | `{"statusCode":404,"message":"User not found"}` |
| Not authenticated | 401 | `{"statusCode":401,"message":"Unauthorized"}` |
| Not admin | 403 | `{"statusCode":403,"message":"Insufficient permissions"}` |

### Test Examples

**Deactivate a user**:
```bash
curl -b cookies.txt -X PATCH \
  http://localhost:5110/api/v1/admin/users/6/status \
  -H "Content-Type: application/json" \
  -d '{"isActive": false}'
```

**Reactivate a user**:
```bash
curl -b cookies.txt -X PATCH \
  http://localhost:5110/api/v1/admin/users/6/status \
  -H "Content-Type: application/json" \
  -d '{"isActive": true}'
```

**Try to deactivate self (should fail)**:
```bash
# Get your own user ID from login response
ADMIN_ID=2

curl -b cookies.txt -X PATCH \
  http://localhost:5110/api/v1/admin/users/$ADMIN_ID/status \
  -H "Content-Type: application/json" \
  -d '{"isActive": false}'

# Expected: HTTP 400 with validation error
```

---

## 🗑️ 4. DELETE /api/v1/admin/users/{id} - Delete User (Admin Action)

**Purpose**: Permanently delete a user account and all associated data (scans, findings, logs).

### Request

```bash
DELETE /api/v1/admin/users/{id}
Authorization: Required (Admin only)
```

**Path Parameters**:
- `id` (required) - User ID to delete

### Response

**Status**: `204 No Content` (no response body)

### Business Rules

1. **Cascade deletion**: Deletes all user data:
   - User account
   - Scan histories
   - Findings
   - Technologies
   - AI summaries
   - Audit logs (optional, may be preserved)
   
2. **Cannot delete self**: Admins cannot delete their own account
3. **Cannot delete other admins**: Only super admins can delete admin accounts
4. **Irreversible action**: No undo, data is permanently deleted

### Error Scenarios

| Scenario | HTTP Code | Response |
|----------|-----------|----------|
| Try to delete self | 400 | `{"statusCode":400,"message":"Cannot delete your own account"}` |
| Try to delete another admin | 403 | `{"statusCode":403,"message":"Cannot delete admin users"}` |
| User not found | 404 | `{"statusCode":404,"message":"User not found"}` |
| Not authenticated | 401 | `{"statusCode":401,"message":"Unauthorized"}` |
| Not admin | 403 | `{"statusCode":403,"message":"Insufficient permissions"}` |

### Test Examples

**Delete a user**:
```bash
curl -b cookies.txt -X DELETE \
  http://localhost:5110/api/v1/admin/users/6 \
  -w "\nHTTP_STATUS:%{http_code}\n"

# Expected: HTTP 204
```

**Verify deletion**:
```bash
curl -b cookies.txt \
  http://localhost:5110/api/v1/users/6/profile \
  -w "\nHTTP_STATUS:%{http_code}\n"

# Expected: HTTP 404 (Not Found)
```

---

## 🔒 Authorization Model

### JWT Claims Used

Admin endpoints extract the following claims from JWT:

```csharp
// User ID (for audit logging)
var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

// User Type (for authorization)
var userType = context.User.FindFirst(ClaimTypes.Role)?.Value;
// "1" = Regular User
// "2" = Admin User
```

### Permission Matrix

| Endpoint | Regular User | Admin |
|----------|--------------|-------|
| GET /api/v1/dashboard/admin | ❌ 403 | ✅ 200 |
| GET /api/v1/dashboard/users | ❌ 403 | ✅ 200 |
| PATCH /api/v1/admin/users/{id}/status | ❌ 403 | ✅ 200* |
| DELETE /api/v1/admin/users/{id} | ❌ 403 | ✅ 204* |

*With business rule validations (cannot modify self or other admins)

---

## 🧪 Automated Testing Script

Save as `test_admin_endpoints.sh`:

```bash
#!/bin/bash
API="http://localhost:5110"
ADMIN_EMAIL="alexandrescarano@gmail.com"
ADMIN_PASS="Admin@123"

# Login
curl -s -c cookies.txt -X POST "$API/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"emailOrLogin\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASS\"}"

# Test 1: Dashboard
echo -e "\n=== Test 1: Admin Dashboard ==="
curl -s -b cookies.txt "$API/api/v1/dashboard/admin?logPage=1&logPageSize=5" | jq '.userStats, .scanStats'

# Test 2: Users List
echo -e "\n=== Test 2: Users List ==="
curl -s -b cookies.txt "$API/api/v1/dashboard/users?page=1&pageSize=5" | jq '{totalCount, users: [.users[0]]}'

# Test 3: Toggle Status (get first regular user)
TEST_USER=$(curl -s -b cookies.txt "$API/api/v1/dashboard/users" | jq -r '.users[] | select(.userType == 1) | .userId' | head -1)

if [ -n "$TEST_USER" ]; then
  echo -e "\n=== Test 3: Toggle User $TEST_USER Status ==="
  curl -s -b cookies.txt -X PATCH "$API/api/v1/admin/users/$TEST_USER/status" \
    -H "Content-Type: application/json" \
    -d '{"isActive":false}' | jq '.'
  
  # Toggle back
  curl -s -b cookies.txt -X PATCH "$API/api/v1/admin/users/$TEST_USER/status" \
    -H "Content-Type: application/json" \
    -d '{"isActive":true}' | jq '.'
fi

# Cleanup
rm cookies.txt
```

Run with:
```bash
chmod +x test_admin_endpoints.sh
./test_admin_endpoints.sh
```

---

## 📝 Notes & Best Practices

### Security Considerations

1. **Admin actions are logged**: All admin operations create audit log entries
2. **JWT validation**: Token must be valid and not expired
3. **Role-based authorization**: Endpoints verify UserType claim
4. **Business rule enforcement**: Cannot modify own account or other admins

### Performance Tips

1. **Use pagination**: Always specify `pageSize` for large user lists
2. **Filter early**: Use `search`, `isActive`, `isAdmin` to reduce dataset
3. **Cache dashboard data**: Dashboard stats can be cached for 1-2 minutes

### Common Pitfalls

1. **Missing UserType claim**: Early versions used `"UserType"` claim name instead of `ClaimTypes.Role` ✅ **FIXED**
2. **Response structure**: Users list returns `.users` array, not `.items`
3. **Soft delete vs hard delete**: Current implementation is hard delete (cascade)

---

## 🐛 Troubleshooting

### Issue: "One or more validation errors occurred"

**Cause**: Missing or invalid `UserType` claim in JWT

**Solution**: Ensure TokenService adds `ClaimTypes.Role`:
```csharp
new Claim(ClaimTypes.Role, ((int)user.UserType).ToString())
```

### Issue: "Cannot block/unblock admin users"

**Cause**: Attempting to toggle status of admin user (including self)

**Solution**: This is expected behavior - only regular users can be toggled

### Issue: 403 Forbidden on admin endpoints

**Cause**: User is not an admin (UserType != 2)

**Solution**: Login with admin credentials or promote user to admin in database

---

## ✅ Test Results

**Tested On**: 2026-02-07  
**API Version**: v1  
**Database**: MySQL  
**Test User**: alexandrescarano@gmail.com (Admin)

| Test Case | Result | Notes |
|-----------|--------|-------|
| Admin login | ✅ PASS | UserType=2 verified |
| Get dashboard stats | ✅ PASS | All stats populated |
| Get users list | ✅ PASS | Pagination working |
| Toggle user status (deactivate) | ✅ PASS | HTTP 200, user deactivated |
| Toggle user status (activate) | ✅ PASS | HTTP 200, user reactivated |
| Toggle own status (should fail) | ✅ PASS | HTTP 400 with validation error |
| Delete user | ✅ PASS | HTTP 204, cascade deletion verified |
| Delete verification | ✅ PASS | HTTP 404 on deleted user profile |

**Overall**: ✅ **100% PASSING** (8/8 test cases)

---

**Last Updated**: 2026-02-07  
**Tested By**: dotnet-backend-expert agent  
**Status**: ✅ **All admin endpoints production-ready**
