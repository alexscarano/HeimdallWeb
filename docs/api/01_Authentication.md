# 🔐 Authentication Endpoints

**Base URL**: `http://localhost:5110`  
**API Version**: v1  
**Last Updated**: 2026-02-08

---

## 📋 Endpoints Overview

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/auth/register` | Register new user account | ❌ No |
| POST | `/api/v1/auth/login` | Login and obtain JWT token | ❌ No |
| POST | `/api/v1/auth/logout` | Logout and clear authentication cookie | ✅ Yes |

---

## 1. POST /api/v1/auth/register

**Description**: Register a new user account and receive a JWT token.

**Authentication**: Not required  
**Rate Limiting**: Not applied  
**Authorization**: Public endpoint

### Request

**Headers**:
```
Content-Type: application/json
```

**Body**:
```json
{
  "email": "string",      // Required, valid email format, max 100 chars
  "username": "string",   // Required, 6-30 chars, alphanumeric + hyphens/underscores
  "password": "string"    // Required, 8-100 chars, must contain: uppercase, lowercase, number, special char
}
```

### Response Success

**Status**: `201 Created`

```json
{
  "userId": 10,
  "username": "testdoc1770559796",
  "email": "testdoc1770559796@example.com",
  "userType": 1,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true
}
```

**Headers**:
- `Location: /api/v1/users/{userId}/profile`

**Note**: The JWT token is also set in an HttpOnly cookie (`authHeimdallCookie`) with:
- HttpOnly: `true`
- Secure: `true` (HTTPS only in production)
- SameSite: `Strict`
- Expiration: 24 hours

### Example curl (Happy Path)

```bash
# Register a new user
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "username": "newuser123",
    "password": "Test@1234"
  }'
```

**Response**:
```json
{
  "userId": 10,
  "username": "newuser123",
  "email": "newuser@example.com",
  "userType": 1,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true
}
```

### Error Scenarios

#### 1. Invalid Email Format

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "invalidemail",
    "username": "testuser",
    "password": "Test@1234"
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

#### 2. Username Too Short (< 6 characters)

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "abc",
    "password": "Test@1234"
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

#### 3. Weak Password (missing uppercase, number, or special char)

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "123456"
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

**Password Requirements**:
- Minimum 8 characters
- Maximum 100 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one number (0-9)
- At least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)

---

#### 4. Duplicate Email

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alexandrescarano@gmail.com",
    "username": "testduplicate",
    "password": "Test@1234"
  }'
```

**Response**: `HTTP 409 Conflict`
```json
{
  "statusCode": 409,
  "message": "An account with this email already exists",
  "errors": null
}
```

---

## 2. POST /api/v1/auth/login

**Description**: Authenticate user and receive JWT token.

**Authentication**: Not required  
**Rate Limiting**: Not applied  
**Authorization**: Public endpoint

### Request

**Headers**:
```
Content-Type: application/json
```

**Body**:
```json
{
  "emailOrLogin": "string",  // Required, email or username
  "password": "string"       // Required
}
```

### Response Success

**Status**: `200 OK`

```json
{
  "userId": 10,
  "username": "testdoc1770559796",
  "email": "testdoc1770559796@example.com",
  "userType": 1,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true
}
```

**Cookie Set**: `authHeimdallCookie` (HttpOnly, Secure, SameSite=Strict, 24h expiration)

**User Types**:
- `1` - Regular User
- `2` - Admin User

### Example curl (Happy Path)

```bash
# Login with email
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrLogin": "newuser@example.com",
    "password": "Test@1234"
  }'
```

**Alternative**: Login with username
```bash
curl -X POST http://localhost:5110/api/v1/auth/login \
  -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrLogin": "newuser123",
    "password": "Test@1234"
  }'
```

**Response**:
```json
{
  "userId": 10,
  "username": "newuser123",
  "email": "newuser@example.com",
  "userType": 1,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true
}
```

**Using the cookie in subsequent requests**:
```bash
# Use the saved cookie for authenticated requests
curl -b cookies.txt http://localhost:5110/api/v1/users/10/profile
```

### Error Scenarios

#### 1. Invalid Credentials

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrLogin": "wrong@example.com",
    "password": "WrongPassword"
  }'
```

**Response**: `HTTP 401 Unauthorized`
```json
{
  "statusCode": 401,
  "message": "Invalid credentials",
  "errors": null
}
```

---

#### 2. Empty/Missing Fields

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrLogin": "test@example.com",
    "password": ""
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

## 3. POST /api/v1/auth/logout

**Description**: Log out the current user and clear the authentication cookie.

**Authentication**: Required (JWT cookie or Bearer token)  
**Rate Limiting**: Not applied  
**Authorization**: Any authenticated user

### Request

**Headers**:
```
Cookie: authHeimdallCookie=<token>
```

**Alternative** (Bearer token):
```
Authorization: Bearer <token>
```

**Body**: No body required

### Response Success

**Status**: `204 No Content`

**Body**: Empty

**Cookie**: `authHeimdallCookie` is deleted (expires set to 1970-01-01)

### Example curl (Happy Path)

```bash
# Logout using saved cookie
curl -X POST http://localhost:5110/api/v1/auth/logout \
  -b cookies.txt
```

**Response**: HTTP 204 No Content (no body)

**Verification** (subsequent request should fail):
```bash
# This should now return 401 Unauthorized
curl -b cookies.txt http://localhost:5110/api/v1/users/10/profile
```

### Error Scenarios

#### 1. Not Authenticated

**Request**:
```bash
curl -X POST http://localhost:5110/api/v1/auth/logout
```

**Response**: `HTTP 401 Unauthorized`

**Body**: Empty (just HTTP status code)

---

## 🔑 JWT Token Structure

The JWT token returned by `/register` and `/login` contains the following claims:

```json
{
  "sub": "10",                              // User ID
  "unique_name": "newuser123",              // Username
  "email": "newuser@example.com",           // Email
  "role": "1",                              // UserType (1=Regular, 2=Admin)
  "nbf": 1770559796,                        // Not Before (Unix timestamp)
  "exp": 1770602996,                        // Expiration (Unix timestamp, 24h from login)
  "iat": 1770559796,                        // Issued At (Unix timestamp)
  "iss": "HeimdallWeb",                     // Issuer
  "aud": "HeimdallWebUsers"                 // Audience
}
```

**Token Expiration**: 24 hours from login/registration

---

## 📌 Important Notes

### Cookie vs Bearer Token

The API supports **two authentication methods**:

1. **HttpOnly Cookie** (Recommended for browsers):
   - Cookie name: `authHeimdallCookie`
   - Set automatically on `/login` and `/register`
   - Deleted on `/logout`
   - Secure (HTTPS only in production)
   - Protected against XSS attacks

2. **Bearer Token** (For API clients):
   - Extract `token` from `/login` or `/register` response
   - Send in `Authorization: Bearer <token>` header
   - Must be stored securely by client

### Example: Using Both Methods

**Cookie-based** (browser/curl):
```bash
# Login and save cookie
curl -c cookies.txt -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrLogin":"user@example.com","password":"Test@1234"}'

# Use cookie in subsequent requests
curl -b cookies.txt http://localhost:5110/api/v1/users/10/profile
```

**Bearer token-based** (mobile app/SPA):
```bash
# Login and extract token
TOKEN=$(curl -s -X POST http://localhost:5110/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrLogin":"user@example.com","password":"Test@1234"}' \
  | jq -r '.token')

# Use token in Authorization header
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5110/api/v1/users/10/profile
```

### Security Considerations

1. **Password Storage**: Passwords are hashed using PBKDF2 with SHA-256
2. **Token Expiration**: Tokens expire after 24 hours
3. **HTTPS Required**: In production, all endpoints require HTTPS
4. **Cookie Security**: HttpOnly + Secure + SameSite=Strict flags prevent CSRF and XSS
5. **Rate Limiting**: Consider implementing rate limiting on login/register endpoints to prevent brute-force attacks

---

## ✅ Test Summary

All authentication endpoints tested on **2026-02-08**:

| Test Case | Expected | Actual | Status |
|-----------|----------|--------|--------|
| Register - Happy path | 201 Created | 201 Created | ✅ PASS |
| Register - Invalid email | 400 Bad Request | 400 Bad Request | ✅ PASS |
| Register - Short username | 400 Bad Request | 400 Bad Request | ✅ PASS |
| Register - Weak password | 400 Bad Request | 400 Bad Request | ✅ PASS |
| Register - Duplicate email | 409 Conflict | 409 Conflict | ✅ PASS |
| Login - Happy path | 200 OK | 200 OK | ✅ PASS |
| Login - Invalid credentials | 401 Unauthorized | 401 Unauthorized | ✅ PASS |
| Login - Empty fields | 400 Bad Request | 400 Bad Request | ✅ PASS |
| Logout - Happy path | 204 No Content | 204 No Content | ✅ PASS |
| Logout - No auth | 401 Unauthorized | 401 Unauthorized | ✅ PASS |

**Success Rate**: 10/10 (100%) ✅

---

## 🔗 Related Documentation

- [Users Endpoints](./02_Users.md)
- [Scans Endpoints](./03_Scans.md)
- [API Overview](./00_API_OVERVIEW.md)

---

**Last Tested**: 2026-02-08 14:09 UTC  
**Tested By**: DocuEngineer  
**Environment**: localhost:5110 (Development)
