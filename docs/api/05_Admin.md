# 👑 Admin Endpoints

**Base URL**: `http://localhost:5110`  
**API Version**: v1  
**Last Updated**: 2026-02-08

---

## 📋 Endpoints Overview

| Method | Endpoint | Description | Auth Required | Admin Only |
|--------|----------|-------------|---------------|------------|
| GET | `/api/v1/dashboard/admin` | Get admin dashboard with stats & logs | ✅ Yes | ✅ Admin |
| GET | `/api/v1/dashboard/users` | List all users with stats (paginated) | ✅ Yes | ✅ Admin |
| PATCH | `/api/v1/admin/users/{id}/status` | Activate/deactivate user account | ✅ Yes | ✅ Admin |
| DELETE | `/api/v1/admin/users/{id}` | Delete user by admin (no password required) | ✅ Yes | ✅ Admin |

---

## 🔐 Admin Authorization

**All endpoints require admin privileges:**
- `userType = 2` (Admin role in JWT token)
- Regular users (`userType = 1`) receive `HTTP 403 Forbidden`
- Unauthenticated requests receive `HTTP 401 Unauthorized`

**How to verify admin status**:
```bash
# Decode JWT token to check role claim
TOKEN=$(curl -s -c cookies.txt \
  -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin@123"}' \
  | jq -r '.token')

# Decode JWT (requires jq and base64)
echo $TOKEN | cut -d'.' -f2 | base64 -d 2>/dev/null | jq '.role'
# Output: "2" (Admin)
```

---

## 1. GET /api/v1/dashboard/admin

**Description**: Comprehensive admin dashboard with system-wide statistics, activity monitoring, and audit logs. Provides:
- User statistics (total, active, blocked, admin/regular counts)
- Scan statistics (total, completed, findings by severity)
- Paginated audit logs with filtering
- Recent scan activity across all users
- Scan trends (time-series data)
- User registration trends (time-series data)

**Authentication**: Required (Admin only)  
**Rate Limiting**: Not applied

**Use Case**: Admin overview page, system monitoring, compliance reporting

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<admin_token>
```

**Query Parameters** (all optional with defaults):
- `logPage` (integer, optional, default: 1) - Log pagination page number
- `logPageSize` (integer, optional, default: 10, max: 50) - Logs per page
- `logLevel` (string, optional) - Filter logs by level: `Info`, `Warning`, `Error`, `Critical`
- `logStartDate` (datetime, optional) - Filter logs from this date (ISO 8601)
- `logEndDate` (datetime, optional) - Filter logs until this date (ISO 8601)

### Response Success

**Status**: `200 OK`

```json
{
  "userStats": {
    "totalUsers": 8,
    "activeUsers": 8,
    "blockedUsers": 0,
    "adminUsers": 1,
    "regularUsers": 7
  },
  "scanStats": {
    "totalScans": 11,
    "completedScans": 10,
    "incompleteScans": 1,
    "totalFindings": 43,
    "criticalFindings": 3,
    "highFindings": 10,
    "mediumFindings": 12,
    "lowFindings": 9
  },
  "logs": {
    "items": [
      {
        "logId": 199,
        "timestamp": "2026-02-08T14:33:42.331924Z",
        "level": "Warning",
        "source": "ToggleUserStatusCommandHandler",
        "message": "User status toggled by admin",
        "userId": 12,
        "username": "scantest1770560247",
        "remoteIp": null
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 199,
    "totalPages": 20
  },
  "recentActivity": [
    {
      "historyId": 16,
      "target": "example.com",
      "createdDate": "2026-02-08T14:17:58.076109Z",
      "userId": 12,
      "username": "scantest1770560247",
      "hasCompleted": true,
      "findingsCount": 11
    }
  ],
  "scanTrend": [],
  "userRegistrationTrend": []
}
```

**Field Descriptions**:

**`userStats`**:
- `totalUsers`: All registered users (active + blocked)
- `activeUsers`: Users with `isActive = true`
- `blockedUsers`: Users with `isActive = false`
- `adminUsers`: Users with `userType = 2`
- `regularUsers`: Users with `userType = 1`

**`scanStats`**:
- `totalScans`: All scans across all users
- `completedScans`: Scans with `hasCompleted = true`
- `incompleteScans`: Failed/timeout scans (`hasCompleted = false`)
- `totalFindings`: Sum of all vulnerabilities found
- `criticalFindings`: Findings with `severity = 4`
- `highFindings`: Findings with `severity = 3`
- `mediumFindings`: Findings with `severity = 2`
- `lowFindings`: Findings with `severity = 1`

**`logs`**:
- Paginated audit trail of system events
- Ordered by `timestamp` DESC (newest first)
- **Log Levels**: `Info`, `Warning`, `Error`, `Critical`
- **Common Sources**: `RegisterUserCommandHandler`, `ExecuteScanCommandHandler`, `ToggleUserStatusCommandHandler`, `DeleteUserByAdminCommandHandler`

**`recentActivity`**:
- Last 10 scans across all users (non-paginated)
- Ordered by `createdDate` DESC

**`scanTrend`** & **`userRegistrationTrend`**:
- Time-series data (empty if no historical data)
- Typically shows daily/weekly/monthly aggregates

### Example curl (Happy Path)

```bash
# Get admin dashboard with default log pagination
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=10"
```

**Response** (truncated):
```json
{
  "userStats": {
    "totalUsers": 8,
    "activeUsers": 8,
    "blockedUsers": 0,
    "adminUsers": 1,
    "regularUsers": 7
  },
  "scanStats": {
    "totalScans": 11,
    "completedScans": 10,
    "incompleteScans": 1,
    "totalFindings": 43,
    "criticalFindings": 3,
    "highFindings": 10
  },
  "logs": {
    "items": [...],
    "totalCount": 199,
    "totalPages": 20
  }
}
```

**Filter logs by level**:
```bash
# Get only Warning and Error logs
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=20&logLevel=Warning"
```

**Filter logs by date range**:
```bash
# Get logs from last 7 days
START_DATE=$(date -u -d '7 days ago' +%Y-%m-%dT%H:%M:%SZ)
END_DATE=$(date -u +%Y-%m-%dT%H:%M:%SZ)

curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=50&logStartDate=${START_DATE}&logEndDate=${END_DATE}"
```

**Parse specific stats**:
```bash
# Get total users count
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=1" \
  | jq '.userStats.totalUsers'

# Get critical findings count
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=1" \
  | jq '.scanStats.criticalFindings'

# Get recent activity targets
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=1" \
  | jq '.recentActivity[].target'
```

### Error Scenarios

#### 1. Unauthorized (Regular User)

**Request**:
```bash
curl -b regular_user_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=10"
```

**Response**: `HTTP 403 Forbidden`
```json
{
  "statusCode": 403,
  "message": "Admin access required",
  "errors": null
}
```

---

#### 2. Not Authenticated

**Request**:
```bash
curl "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=10"
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 3. Invalid Pagination

**Scenario**: Negative page number or excessive pageSize

**Request**:
```bash
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=-1&logPageSize=100"
```

**Response**: `HTTP 200 OK` (auto-corrected to defaults)
- Negative/zero `logPage` → defaults to 1
- `logPageSize > 50` → capped at 50

---

## 2. GET /api/v1/dashboard/users

**Description**: Retrieve a paginated list of all users in the system with:
- User profile information
- Account status (active/blocked)
- User type (regular/admin)
- Scan and findings counts
- Advanced filtering capabilities

**Authentication**: Required (Admin only)  
**Rate Limiting**: Not applied

**Use Case**: User management interface, user search, account auditing

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<admin_token>
```

**Query Parameters** (all optional with defaults):
- `page` (integer, optional, default: 1) - Page number (min: 1)
- `pageSize` (integer, optional, default: 10, max: 100) - Items per page
- `search` (string, optional) - Search by username or email (partial match)
- `isActive` (boolean, optional) - Filter by account status (`true`/`false`)
- `isAdmin` (boolean, optional) - Filter by user type (`true`=admin, `false`=regular)
- `createdFrom` (datetime, optional) - Filter users created after this date
- `createdTo` (datetime, optional) - Filter users created before this date

### Response Success

**Status**: `200 OK`

```json
{
  "users": [
    {
      "userId": 12,
      "username": "scantest1770560247",
      "email": "scantest1770560247@example.com",
      "userType": 1,
      "isActive": true,
      "profileImage": null,
      "createdAt": "2026-02-08T14:17:27.909676Z",
      "scanCount": 1,
      "findingsCount": 11
    },
    {
      "userId": 2,
      "username": "alexandrescarano",
      "email": "alexandrescarano@gmail.com",
      "userType": 2,
      "isActive": true,
      "profileImage": "uploads/profiles/2_20260207230250.jpg",
      "createdAt": "2026-02-06T19:41:42.60754Z",
      "scanCount": 7,
      "findingsCount": 31
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 8,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Field Descriptions**:
- `userType`: `1` = Regular user, `2` = Admin
- `scanCount`: Total scans executed by this user
- `findingsCount`: Total vulnerabilities found across all user's scans
- `profileImage`: Relative path to uploaded image (or `null`)

### Example curl (Happy Path)

```bash
# Get all users (default pagination)
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10"
```

**Response**:
```json
{
  "users": [
    {
      "userId": 12,
      "username": "scantest1770560247",
      "email": "scantest1770560247@example.com",
      "userType": 1,
      "isActive": true,
      "scanCount": 1,
      "findingsCount": 11
    }
  ],
  "totalCount": 8,
  "totalPages": 1
}
```

**Search by username or email**:
```bash
# Search for users containing "test"
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&search=test"

# Search by email domain
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&search=@gmail.com"
```

**Filter by status**:
```bash
# Get only active users
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&isActive=true"

# Get only blocked users
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&isActive=false"
```

**Filter by user type**:
```bash
# Get only admins
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&isAdmin=true"

# Get only regular users
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&isAdmin=false"
```

**Filter by registration date**:
```bash
# Users registered in last 30 days
START_DATE=$(date -u -d '30 days ago' +%Y-%m-%dT%H:%M:%SZ)
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=50&createdFrom=${START_DATE}"

# Users registered in specific month (January 2026)
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=50&createdFrom=2026-01-01T00:00:00Z&createdTo=2026-01-31T23:59:59Z"
```

**Combine multiple filters**:
```bash
# Active regular users with scans, sorted by most findings
curl -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=20&isActive=true&isAdmin=false"
```

**Extract specific data**:
```bash
# List all usernames
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100" \
  | jq -r '.users[].username'

# Get users with most scans (client-side sort)
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100" \
  | jq '.users | sort_by(-.scanCount) | .[:5]'

# Count inactive users
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=1000&isActive=false" \
  | jq '.totalCount'
```

### Error Scenarios

#### 1. Unauthorized (Regular User)

**Request**:
```bash
curl -b regular_user_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10"
```

**Response**: `HTTP 403 Forbidden`
```json
{
  "statusCode": 403,
  "message": "Admin access required",
  "errors": null
}
```

---

#### 2. Not Authenticated

**Request**:
```bash
curl "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10"
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

## 3. PATCH /api/v1/admin/users/{id}/status

**Description**: Activate or deactivate a user account. When deactivated (`isActive = false`):
- User cannot login (authentication fails)
- Existing sessions are not immediately invalidated (JWT still valid until expiration)
- User's scans and data remain intact
- Can be reactivated at any time

**Authentication**: Required (Admin only)  
**Authorization**: Only admins can toggle user status

**Use Case**: Account suspension, abuse prevention, temporary access control

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<admin_token>
Content-Type: application/json
```

**URL Parameters**:
- `id` (integer, required) - User ID to update

**Body**:
```json
{
  "isActive": true  // or false
}
```

### Response Success

**Status**: `200 OK`

```json
{
  "userId": 12,
  "username": "scantest1770560247",
  "isActive": false
}
```

### Example curl (Happy Path)

```bash
# Deactivate user (block account)
curl -X PATCH -b admin_cookies.txt \
  http://localhost:5110/api/v1/admin/users/12/status \
  -H "Content-Type: application/json" \
  -d '{"isActive":false}'
```

**Response**:
```json
{
  "userId": 12,
  "username": "scantest1770560247",
  "isActive": false
}
```

**Verify user cannot login**:
```bash
# Attempt login with deactivated account
curl -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"scantest1770560247@example.com","password":"Test@1234"}'
```

**Expected Response**: `HTTP 400 Bad Request` or `HTTP 401 Unauthorized`

**Reactivate user**:
```bash
# Restore user access
curl -X PATCH -b admin_cookies.txt \
  http://localhost:5110/api/v1/admin/users/12/status \
  -H "Content-Type: application/json" \
  -d '{"isActive":true}'
```

**Response**:
```json
{
  "userId": 12,
  "username": "scantest1770560247",
  "isActive": true
}
```

**Bulk status updates** (client-side loop):
```bash
#!/bin/bash
# Deactivate multiple users
USER_IDS=(5 7 9)

for id in "${USER_IDS[@]}"; do
  echo "Deactivating user $id..."
  curl -X PATCH -b admin_cookies.txt \
    "http://localhost:5110/api/v1/admin/users/$id/status" \
    -H "Content-Type: application/json" \
    -d '{"isActive":false}'
  sleep 0.5
done
```

### Error Scenarios

#### 1. User Not Found

**Request**:
```bash
curl -X PATCH -b admin_cookies.txt \
  http://localhost:5110/api/v1/admin/users/99999/status \
  -H "Content-Type: application/json" \
  -d '{"isActive":false}'
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "User with ID 99999 not found",
  "errors": null
}
```

---

#### 2. Unauthorized (Regular User)

**Request**:
```bash
curl -X PATCH -b regular_user_cookies.txt \
  http://localhost:5110/api/v1/admin/users/12/status \
  -H "Content-Type: application/json" \
  -d '{"isActive":false}'
```

**Response**: `HTTP 403 Forbidden`
```json
{
  "statusCode": 403,
  "message": "Admin access required",
  "errors": null
}
```

---

#### 3. Not Authenticated

**Request**:
```bash
curl -X PATCH \
  http://localhost:5110/api/v1/admin/users/12/status \
  -H "Content-Type: application/json" \
  -d '{"isActive":false}'
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

## 4. DELETE /api/v1/admin/users/{id}

**Description**: **Permanently delete** a user account by admin. Unlike regular user self-deletion (`DELETE /api/v1/users/{id}`), this endpoint:
- **Does NOT require password** (admin override)
- **Cannot delete admin users** (admins cannot delete other admins)
- **Cascade deletes** all user data:
  - Scan histories
  - Findings
  - Technologies
  - AI summaries
  - Logs (associated with user)
  - Profile image

**⚠️ WARNING**: This action is **irreversible**. All user data is permanently deleted.

**Authentication**: Required (Admin only)  
**Authorization**: Admin users only (cannot delete other admins)

**Use Case**: Account removal, GDPR compliance (right to be forgotten), abuse cleanup

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<admin_token>
```

**URL Parameters**:
- `id` (integer, required) - User ID to delete

### Response Success

**Status**: `204 No Content` (empty body)

**Note**: Unlike regular user deletion which returns JSON confirmation, admin deletion returns no body (standard RESTful practice for DELETE).

### Example curl (Happy Path)

```bash
# Delete user by admin
curl -X DELETE -b admin_cookies.txt \
  http://localhost:5110/api/v1/admin/users/12 \
  -w "\nHTTP Status: %{http_code}\n"
```

**Response**:
```
HTTP Status: 204
```

**Verify deletion** (should return 404):
```bash
# Try to get deleted user from user list
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100&search=scantest" \
  | jq '.users[] | select(.userId == 12)'
# Output: (empty - user not found)
```

**Create then delete workflow**:
```bash
#!/bin/bash
# Create test user
NEW_USER=$(curl -s -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"tempuser","email":"temp@example.com","password":"Test@1234"}')
NEW_USER_ID=$(echo $NEW_USER | jq -r '.userId')

echo "Created user ID: $NEW_USER_ID"

# Perform some scans or operations...
sleep 2

# Delete by admin
echo "Deleting user $NEW_USER_ID..."
curl -X DELETE -b admin_cookies.txt \
  "http://localhost:5110/api/v1/admin/users/$NEW_USER_ID" \
  -w "\nHTTP Status: %{http_code}\n"
```

**Bulk deletion** (use with caution):
```bash
#!/bin/bash
# WARNING: Deletes all inactive regular users
# Use with extreme caution!

echo "Finding inactive users..."
INACTIVE_USERS=$(curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100&isActive=false&isAdmin=false" \
  | jq -r '.users[].userId')

echo "Found $(echo "$INACTIVE_USERS" | wc -l) inactive users"
echo "Delete all? (yes/no)"
read confirm

if [ "$confirm" = "yes" ]; then
  for id in $INACTIVE_USERS; do
    echo "Deleting user $id..."
    curl -X DELETE -b admin_cookies.txt \
      "http://localhost:5110/api/v1/admin/users/$id"
    sleep 0.5
  done
  echo "Deletion complete"
else
  echo "Aborted"
fi
```

### Error Scenarios

#### 1. User Not Found

**Request**:
```bash
curl -X DELETE -b admin_cookies.txt \
  http://localhost:5110/api/v1/admin/users/99999
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "User with ID 99999 not found",
  "errors": null
}
```

---

#### 2. Unauthorized (Regular User)

**Request**:
```bash
curl -X DELETE -b regular_user_cookies.txt \
  http://localhost:5110/api/v1/admin/users/10
```

**Response**: `HTTP 403 Forbidden`
```json
{
  "statusCode": 403,
  "message": "Admin access required",
  "errors": null
}
```

**Note**: Regular users attempting admin deletion may receive `400 Bad Request` depending on implementation.

---

#### 3. Not Authenticated

**Request**:
```bash
curl -X DELETE http://localhost:5110/api/v1/admin/users/10
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 4. Cannot Delete Admin User

**Scenario**: Admin trying to delete another admin

**Request**:
```bash
# Get admin user ID
ADMIN_ID=2

# Attempt deletion
curl -X DELETE -b admin_cookies.txt \
  "http://localhost:5110/api/v1/admin/users/${ADMIN_ID}"
```

**Expected Response**: `HTTP 403 Forbidden` or `HTTP 400 Bad Request`
```json
{
  "statusCode": 403,
  "message": "Cannot delete admin users",
  "errors": null
}
```

**Note**: Exact error message depends on implementation - verify with actual test.

---

## 📊 Admin Workflow Examples

### Daily Admin Tasks

**1. Check system health**:
```bash
# Get dashboard overview
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=20" \
  | jq '{
      totalUsers: .userStats.totalUsers,
      activeUsers: .userStats.activeUsers,
      totalScans: .scanStats.totalScans,
      criticalFindings: .scanStats.criticalFindings
    }'
```

**2. Review critical logs**:
```bash
# Get errors and warnings
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/admin?logPage=1&logPageSize=50&logLevel=Error" \
  | jq '.logs.items[] | {timestamp, source, message}'
```

**3. Find suspicious users**:
```bash
# Users with many scans but few findings (possible abuse)
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100" \
  | jq '.users[] | select(.scanCount > 10 and .findingsCount == 0)'
```

**4. Cleanup inactive accounts**:
```bash
# List users registered >90 days ago with 0 scans
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100&createdTo=$(date -u -d '90 days ago' +%Y-%m-%dT%H:%M:%SZ)" \
  | jq '.users[] | select(.scanCount == 0)'
```

### User Management Workflows

**Block abusive user**:
```bash
#!/bin/bash
# 1. Search for user
USER=$(curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=10&search=suspicious" \
  | jq '.users[0]')

USER_ID=$(echo $USER | jq -r '.userId')
echo "Found user: $(echo $USER | jq -r '.username')"

# 2. Review user activity
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/scans?page=1&pageSize=100" \
  | jq ".items[] | select(.userId == $USER_ID)"

# 3. Block user
echo "Blocking user $USER_ID..."
curl -X PATCH -b admin_cookies.txt \
  "http://localhost:5110/api/v1/admin/users/$USER_ID/status" \
  -H "Content-Type: application/json" \
  -d '{"isActive":false}'
```

**Generate compliance report**:
```bash
#!/bin/bash
# GDPR compliance: Export all user data before deletion
USER_ID=12

echo "Exporting data for user $USER_ID..."

# 1. Get user profile
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/dashboard/users?page=1&pageSize=100" \
  | jq ".users[] | select(.userId == $USER_ID)" \
  > user_${USER_ID}_profile.json

# 2. Export all scans
curl -s -b admin_cookies.txt \
  "http://localhost:5110/api/v1/scan-histories/export" \
  -o user_${USER_ID}_scans.pdf

# 3. Delete user
echo "Deleting user $USER_ID..."
curl -X DELETE -b admin_cookies.txt \
  "http://localhost:5110/api/v1/admin/users/$USER_ID"

echo "Exported to user_${USER_ID}_profile.json and user_${USER_ID}_scans.pdf"
```

---

## 📌 Important Notes

### Admin Privileges

**What admins CAN do**:
- View all users and their statistics
- Access all scan histories (any user)
- Block/unblock user accounts
- Delete regular user accounts without password
- View system-wide statistics and logs

**What admins CANNOT do**:
- Delete other admin accounts (protection against self-lock)
- Modify user passwords (users must reset via forgot password)
- Access user JWT tokens (tokens are never stored server-side)

### Security Considerations

**Admin Account Protection**:
- Admin accounts should use strong passwords (enforced at registration)
- Consider implementing 2FA for admin accounts (future enhancement)
- Audit all admin actions via system logs

**Blocking vs Deletion**:
- **Block** (`PATCH /status`): Reversible, preserves all data, prevents login
- **Delete** (`DELETE /users/{id}`): Irreversible, removes all data, GDPR compliant

**Recommendation**: Block users first, delete only after confirmation period (e.g., 30 days).

### Cascade Deletion

When admin deletes a user, the following data is **permanently removed**:
1. User record (`tb_user`)
2. All scan histories (`tb_history`)
3. All findings (`tb_finding`)
4. All technologies (`tb_technology`)
5. All AI summaries (`tb_ia_summary`)
6. User usage records (`tb_user_usage`)
7. Profile image file (from filesystem)
8. Related logs (audit trail - consider preserving for compliance)

**Database foreign key constraints** ensure referential integrity (ON DELETE CASCADE).

### Log Retention

**Default behavior**:
- Logs are retained indefinitely
- No automatic cleanup/archival
- Logs table can grow large over time

**Recommendations**:
1. Implement log rotation (archive logs >90 days)
2. Export compliance logs before user deletion
3. Monitor `tb_log` table size

### Performance Considerations

**Dashboard query performance**:
- Uses database VIEWs for aggregated stats (optimized)
- Recent activity limited to 10 items (non-paginated)
- Log pagination prevents large result sets

**User list scalability**:
- Paginated with max 100 items per page
- Indexes on `username`, `email`, `createdAt`, `isActive`
- Search uses `LIKE %term%` (may be slow with >10K users)

**Optimization tips**:
- Use specific filters (isActive, isAdmin) to reduce result set
- Avoid searching very broad terms (single character)
- Cache dashboard stats for 1-5 minutes (not implemented yet)

---

## ✅ Test Summary

All admin endpoints tested on **2026-02-08**:

| Test Case | Expected | Actual | Status |
|-----------|----------|--------|--------|
| GET dashboard - Admin | 200 OK (with params) | 200 OK | ✅ PASS |
| GET dashboard - Regular user | 403 Forbidden | 403 Forbidden | ✅ PASS |
| GET dashboard - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| GET users - Admin | 200 OK | 200 OK | ✅ PASS |
| GET users - Pagination | 200 OK | 200 OK | ✅ PASS |
| GET users - Regular user | 403 Forbidden | 403 Forbidden | ✅ PASS |
| GET users - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| PATCH status - Deactivate | 200 OK | 200 OK | ✅ PASS |
| PATCH status - Reactivate | 200 OK | 200 OK | ✅ PASS |
| PATCH status - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| PATCH status - Regular user | 403 Forbidden | 403 Forbidden | ✅ PASS |
| PATCH status - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| DELETE user - Admin | 204 No Content | 204 No Content | ✅ PASS |
| DELETE user - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| DELETE user - Regular user | 403 Forbidden | 403 Forbidden | ✅ PASS |
| DELETE user - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |

**Success Rate**: 16/16 (100%) ✅  
**All Issues Fixed**: DELETE endpoint now returns correct 403 Forbidden for unauthorized users

---

## 🔗 Related Documentation

- [Users Endpoints](./02_Users.md) - Regular user self-management
- [Scan History Endpoints](./04_History.md) - Admin can access all scans
- [Authentication Endpoints](./01_Authentication.md)
- [API Overview](./00_API_OVERVIEW.md)

---

**Last Tested**: 2026-02-08 14:34 UTC  
**Tested By**: DocuEngineer  
**Environment**: localhost:5110 (Development)  
**Admin User**: alexandrescarano@gmail.com (userId: 2, userType: 2)
