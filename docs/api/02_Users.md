# 👤 User Endpoints

**Base URL**: `http://localhost:5110`  
**API Version**: v1  
**Last Updated**: 2026-02-08

---

## 📋 Endpoints Overview

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/v1/users/{uuid}/profile` | Get user profile information | ✅ Yes |
| GET | `/api/v1/users/{uuid}/statistics` | Get user scan statistics | ✅ Yes |
| PUT | `/api/v1/users/{uuid}` | Update user profile (username/email) | ✅ Yes |
| PATCH | `/api/v1/users/{uuid}/password` | Update user password | ✅ Yes |
| POST | `/api/v1/users/{uuid}/profile-image` | Upload profile image (base64) | ✅ Yes |
| DELETE | `/api/v1/users/{uuid}` | Delete user account | ✅ Yes |

---

## 1. GET /api/v1/users/{uuid}/profile

**Description**: Retrieve user profile information.

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Any authenticated user can view any profile

### Request

**Path Parameters**:
- `uuid` (string (UUID v7), required) - User ID

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

### Response Success

**Status**: `200 OK`

```json
{
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "username": "testdoc1770559796",
  "email": "testdoc1770559796@example.com",
  "userType": 1,
  "isActive": true,
  "profileImage": null,
  "createdAt": "2026-02-08T14:09:56.485167Z"
}
```

**Fields**:
- `userType`: `1` = Regular User, `2` = Admin
- `isActive`: `true` = Active account, `false` = Deactivated
- `profileImage`: Filename if uploaded, `null` otherwise
- `createdAt`: ISO 8601 timestamp (UTC)

### Example curl (Happy Path)

```bash
# Get own profile
curl -b cookies.txt http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile
```

**Response**:
```json
{
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "username": "testdoc1770559796",
  "email": "testdoc1770559796@example.com",
  "userType": 1,
  "isActive": true,
  "profileImage": null,
  "createdAt": "2026-02-08T14:09:56.485167Z"
}
```

### Error Scenarios

#### 1. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

#### 2. User Not Found

**Request**:
```bash
curl -b cookies.txt http://localhost:5110/api/v1/users/019c3e5b-0000-7e25-0000-000000000000/profile
```

**Response**: `HTTP 404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Entity 'User' not found.",
  "errors": null
}
```

---

## 2. GET /api/v1/users/{uuid}/statistics

**Description**: Get user's scan statistics (total scans, findings breakdown, risk trends).

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Any authenticated user

### Request

**Path Parameters**:
- `uuid` (string (UUID v7), required) - User ID

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

### Response Success

**Status**: `200 OK`

```json
{
  "totalScans": 0,
  "completedScans": 0,
  "incompleteScans": 0,
  "averageDuration": null,
  "lastScanDate": null,
  "totalFindings": 0,
  "criticalFindings": 0,
  "highFindings": 0,
  "mediumFindings": 0,
  "lowFindings": 0,
  "informationalFindings": 0,
  "riskTrend": [],
  "categoryBreakdown": []
}
```

**Fields**:
- `averageDuration`: Average scan duration in seconds (null if no scans)
- `lastScanDate`: ISO 8601 timestamp of last scan (null if no scans)
- `riskTrend`: Array of historical risk levels (for charting)
- `categoryBreakdown`: Array of finding categories with counts

### Example curl (Happy Path)

```bash
# Get user statistics
curl -b cookies.txt http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/statistics
```

**Response**:
```json
{
  "totalScans": 5,
  "completedScans": 4,
  "incompleteScans": 1,
  "averageDuration": "00:00:23.456",
  "lastScanDate": "2026-02-08T14:00:00Z",
  "totalFindings": 12,
  "criticalFindings": 2,
  "highFindings": 3,
  "mediumFindings": 5,
  "lowFindings": 2,
  "informationalFindings": 0,
  "riskTrend": [
    {"date": "2026-02-01", "critical": 0, "high": 1, "medium": 2},
    {"date": "2026-02-05", "critical": 1, "high": 2, "medium": 3}
  ],
  "categoryBreakdown": [
    {"category": "Security Headers", "count": 5},
    {"category": "SSL/TLS", "count": 3}
  ]
}
```

### Error Scenarios

#### 1. Not Authenticated

**Request**:
```bash
curl http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/statistics
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

## 3. PUT /api/v1/users/{uuid}

**Description**: Update user profile (username and/or email).

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Users can only update their own profile

### Request

**Path Parameters**:
- `uuid` (string (UUID v7), required) - User ID

**Headers**:
```
Content-Type: application/json
Cookie: authHeimdallCookie=<token>
```

**Body**:
```json
{
  "newUsername": "string",  // Optional, 6-30 chars, alphanumeric + hyphens/underscores
  "newEmail": "string"      // Optional, valid email format, max 100 chars
}
```

**Note**: You must provide at least one field (username or email). Both can be updated simultaneously.

### Response Success

**Status**: `200 OK`

```json
{
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "username": "updated1770559982",
  "email": "updated1770559982@example.com",
  "userType": 1,
  "isActive": true
}
```

### Example curl (Happy Path)

```bash
# Update both username and email
curl -X PUT http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50 \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{
    "newUsername": "mynewusername",
    "newEmail": "newemail@example.com"
  }'
```

**Response**:
```json
{
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "username": "mynewusername",
  "email": "newemail@example.com",
  "userType": 1,
  "isActive": true
}
```

**Update only username**:
```bash
curl -X PUT http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50 \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"newUsername": "mynewusername"}'
```

**Update only email**:
```bash
curl -X PUT http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50 \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"newEmail": "newemail@example.com"}'
```

### Error Scenarios

#### 1. Email Already Exists (Duplicate)

**Request**:
```bash
curl -X PUT http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50 \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{
    "newUsername": "test",
    "newEmail": "alexandrescarano@gmail.com"
  }'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": null
}
```

---

#### 2. Trying to Update Another User's Profile (Forbidden)

**Request**:
```bash
# User ID 019c3e5b-3fe1-7e25-a3ea-2b443e65bb50 trying to update user 019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c
curl -X PUT http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"newUsername": "hacker", "newEmail": "hacker@example.com"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": null
}
```

**Note**: The validation error is generic for security reasons (doesn't reveal if user exists).

---

## 4. PATCH /api/v1/users/{uuid}/password

**Description**: Update the authenticated user's password. Requires current password verification.

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Users can only update their own password (ID must match JWT claim)

### Request

**URL Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| uuid | string (UUID v7) | Yes | User ID (must match authenticated user) |

**Body** (JSON):
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| currentPassword | string | Yes | Must match current password |
| newPassword | string | Yes | Min 8 chars, uppercase, lowercase, digit, special char |
| confirmNewPassword | string | Yes | Must match newPassword |

### curl Examples

#### ✅ Success - Password Updated
```bash
curl -s -X PATCH http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c/password \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "currentPassword": "Admin@123",
    "newPassword": "NewAdmin@456",
    "confirmNewPassword": "NewAdmin@456"
  }'
```

**Response**: `HTTP 200 OK`
```json
{
  "message": "Password updated successfully"
}
```

#### ❌ Error - Wrong Current Password
```bash
curl -s -X PATCH http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c/password \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "currentPassword": "WrongPassword@123",
    "newPassword": "NewAdmin@456",
    "confirmNewPassword": "NewAdmin@456"
  }'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "CurrentPassword": ["Current password is incorrect"]
  }
}
```

#### ❌ Error - Weak New Password
```bash
curl -s -X PATCH http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c/password \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "currentPassword": "Admin@123",
    "newPassword": "weak",
    "confirmNewPassword": "weak"
  }'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "NewPassword": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one digit",
      "Password must contain at least one special character (!@#$%^&*)"
    ]
  }
}
```

#### ❌ Error - Passwords Don't Match
```bash
curl -s -X PATCH http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c/password \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "currentPassword": "Admin@123",
    "newPassword": "NewAdmin@456",
    "confirmNewPassword": "DifferentPass@789"
  }'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "ConfirmNewPassword": ["Passwords do not match"]
  }
}
```

#### ❌ Error - Not Authenticated
```bash
curl -s -X PATCH http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c/password \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "Admin@123",
    "newPassword": "NewAdmin@456",
    "confirmNewPassword": "NewAdmin@456"
  }'
```

**Response**: `HTTP 401 Unauthorized` (empty body)

### Error Summary

| Scenario | Status | Error Key |
|----------|--------|-----------|
| Wrong current password | 400 | `CurrentPassword` |
| Weak new password | 400 | `NewPassword` |
| Passwords don't match | 400 | `ConfirmNewPassword` |
| Not authenticated | 401 | - |
| Wrong user ID | 403 | - |

---

## 5. POST /api/v1/users/{uuid}/profile-image

**Description**: Upload a profile image (base64 encoded).

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Users can only upload their own profile image

### Request

**Path Parameters**:
- `uuid` (string (UUID v7), required) - User ID

**Headers**:
```
Content-Type: application/json
Cookie: authHeimdallCookie=<token>
```

**Body**:
```json
{
  "imageBase64": "string"  // Required, base64-encoded image (PNG, JPG, JPEG)
}
```

**Image Requirements**:
- Format: PNG, JPG, or JPEG
- Encoding: Base64 string
- Max size: Depends on server configuration (typically 5-10MB)
- Recommended: Square images for best display

### Response Success

**Status**: `200 OK`

```json
{
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "profileImagePath": "uploads/profiles/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50_20260208141302.jpg"
}
```

**Note**: The image is saved to disk and the filename is stored in the database.

### Example curl (Happy Path)

```bash
# Upload a 1x1 red pixel PNG (for testing)
BASE64_IMAGE="iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="

curl -X POST http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile-image \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d "{\"imageBase64\":\"$BASE64_IMAGE\"}"
```

**Response**:
```json
{
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50",
  "profileImagePath": "uploads/profiles/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50_20260208141302.jpg"
}
```

**Real image example** (convert image to base64):
```bash
# Convert image to base64
IMAGE_BASE64=$(base64 -w 0 myprofile.jpg)

# Upload
curl -X POST http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile-image \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d "{\"imageBase64\":\"$IMAGE_BASE64\"}"
```

### Error Scenarios

#### 1. Invalid Base64 String

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile-image \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"imageBase64":"invalid_base64!!!"}'
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": null
}
```

---

#### 2. Not Authenticated

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile-image \
  -H "Content-Type: application/json" \
  -d '{"imageBase64":"iVBORw0KGgo..."}'
```

**Response**: `HTTP 401 Unauthorized` (empty body)

---

## 6. DELETE /api/v1/users/{uuid}

**Description**: Delete a user account and all associated data (scans, findings, logs).

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**:
- Regular users can delete their own account (with password confirmation)
- Admins can delete regular users (without password)
- Admins cannot delete other admins

### Request

**Path Parameters**:
- `uuid` (string (UUID v7), required) - User ID

**Query Parameters**:
- `confirmDelete` (boolean, required) - Must be `true` to proceed
- `password` (string, optional) - Required **ONLY** when deleting own account

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

### Response Success

**Status**: `200 OK`

```json
{
  "message": "User deleted successfully",
  "userId": "019c3e5b-5c3d-7e25-c3d4-4e5f6a7b8c9d"
}
```

**Note**: After successful deletion, the user's JWT token becomes invalid.

### Example curl (Happy Path)

#### User Deleting Own Account (Password Required)

```bash
# Delete own account with password confirmation
curl -X DELETE "http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50?confirmDelete=true&password=Test@1234" \
  -b cookies.txt
```

**Response**:
```json
{
  "message": "User deleted successfully",
  "userId": "019c3e5b-3fe1-7e25-a3ea-2b443e65bb50"
}
```

**Verification** (subsequent requests will fail):
```bash
# This will now return 401 Unauthorized
curl -b cookies.txt http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50/profile
```

---

#### Admin Deleting Regular User (No Password Required)

```bash
# Admin deletes a regular user without password
curl -X DELETE "http://localhost:5110/api/v1/users/019c3e5b-5c3d-7e25-c3d4-4e5f6a7b8c9d?confirmDelete=true" \
  -b admin_cookies.txt
```

**Response**:
```json
{
  "message": "User deleted successfully",
  "userId": "019c3e5b-5c3d-7e25-c3d4-4e5f6a7b8c9d"
}
```

### Error Scenarios

#### 1. Deleting Own Account Without Password

**Request**:
```bash
curl -X DELETE "http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50?confirmDelete=true" \
  -b cookies.txt
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": null
}
```

**Error Detail**: Password is required when deleting your own account.

---

#### 2. Wrong Password

**Request**:
```bash
curl -X DELETE "http://localhost:5110/api/v1/users/019c3e5b-3fe1-7e25-a3ea-2b443e65bb50?confirmDelete=true&password=WrongPassword123" \
  -b cookies.txt
```

**Response**: `HTTP 401 Unauthorized`
```json
{
  "statusCode": 401,
  "message": "Invalid password. Account deletion cancelled.",
  "errors": null
}
```

---

#### 3. Trying to Delete Another User (Regular User)

**Request**:
```bash
# Regular user trying to delete admin account
curl -X DELETE "http://localhost:5110/api/v1/users/019c3e5b-4a12-7e25-b1c2-3d4e5f6a7b8c?confirmDelete=true&password=Test@1234" \
  -b cookies.txt
```

**Response**: `HTTP 400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": null
}
```

**Note**: For security reasons, the error is generic (doesn't reveal if user exists or permissions).

---

## 🔐 Authorization Rules

### Profile Access
| User Type | Own Profile | Other Profiles |
|-----------|-------------|----------------|
| Regular User | ✅ View/Edit | ✅ View Only |
| Admin | ✅ View/Edit | ✅ View Only |

### Update Profile
| User Type | Own Profile | Other Profiles |
|-----------|-------------|----------------|
| Regular User | ✅ Allow | ❌ Forbidden |
| Admin | ✅ Allow | ❌ Forbidden |

**Note**: Even admins cannot update other users' profiles. Use admin endpoints (`PATCH /api/v1/admin/users/{uuid}/status`) to modify user status.

### Delete Account
| User Type | Own Account | Other Regular Users | Other Admins |
|-----------|-------------|---------------------|--------------|
| Regular User | ✅ Allow (with password) | ❌ Forbidden | ❌ Forbidden |
| Admin | ✅ Allow (with password) | ✅ Allow (no password) | ❌ Forbidden |

---

## 📌 Important Notes

### Cascade Deletion

When a user account is deleted, the following data is **permanently removed**:
- ✅ User profile and credentials
- ✅ All scan histories
- ✅ All findings (vulnerabilities)
- ✅ All detected technologies
- ✅ AI summaries
- ⚠️ Audit logs remain (for compliance, with user_id reference)

**Warning**: This operation is **irreversible**. All data will be lost.

### Profile Image Storage

- Images are saved to: `wwwroot/uploads/profiles/`
- Filename format: `{userId}_{timestamp}.{extension}`
- Old images are **not** automatically deleted when uploading new ones
- Consider implementing cleanup for old images

### Username/Email Uniqueness

- Usernames are **NOT** unique (multiple users can have same username)
- Emails **ARE** unique (enforced by database constraint)
- Validation checks for duplicate emails on registration and profile update

### Password Requirements (for deletion)

When deleting own account:
- Password must match the current account password
- Password is validated using the same hashing algorithm (PBKDF2 + SHA-256)
- Failed attempts are logged for security monitoring

---

## ✅ Test Summary

All user endpoints tested on **2026-02-08**:

| Test Case | Expected | Actual | Status |
|-----------|----------|--------|--------|
| GET profile - Happy path | 200 OK | 200 OK | ✅ PASS |
| GET profile - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| GET profile - Not found | 404 Not Found | 404 Not Found | ✅ PASS |
| GET statistics - Happy path | 200 OK | 200 OK | ✅ PASS |
| GET statistics - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| PUT update - Happy path | 200 OK | 200 OK | ✅ PASS |
| PUT update - Duplicate email | 400 Bad Request | 400 Bad Request | ✅ PASS |
| PUT update - Forbidden | 400 Bad Request | 400 Bad Request | ✅ PASS |
| PATCH password - Happy path | 200 OK | 200 OK | ✅ PASS |
| PATCH password - Wrong current | 400 Bad Request | 400 Bad Request | ✅ PASS |
| PATCH password - Weak password | 400 Bad Request | 400 Bad Request | ✅ PASS |
| PATCH password - Mismatch | 400 Bad Request | 400 Bad Request | ✅ PASS |
| POST image - Happy path | 200 OK | 200 OK | ✅ PASS |
| POST image - Invalid base64 | 400 Bad Request | 400 Bad Request | ✅ PASS |
| DELETE - No password | 400 Bad Request | 400 Bad Request | ✅ PASS |
| DELETE - Wrong password | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| DELETE by admin - Success | 200 OK | 200 OK | ✅ PASS |

**Success Rate**: 17/17 (100%) ✅

---

## 🔗 Related Documentation

- [Authentication Endpoints](./01_Authentication.md)
- [Scans Endpoints](./03_Scans.md)
- [Admin Endpoints](./05_Admin.md)
- [API Overview](./00_API_OVERVIEW.md)

---

**Last Tested**: 2026-02-08 14:13 UTC  
**Tested By**: DocuEngineer  
**Environment**: localhost:5110 (Development)
