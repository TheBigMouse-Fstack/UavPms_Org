# Frontend API Specification Document

**Version:** 1.0  
**Last Updated:** 2026-06-15  
**Purpose:** Detailed API specification for Backend development based on Frontend requirements

## Table of Contents

1. [Overview](#overview)
2. [Base Configuration](#base-configuration)
3. [Authentication Endpoints](#authentication-endpoints)
4. [User Management Endpoints](#user-management-endpoints)
5. [Response Format](#response-format)
6. [Error Handling](#error-handling)
7. [Testing Credentials](#testing-credentials)

---

## Overview

The Frontend application requires API endpoints to support:
- **User Authentication** (Login, Logout, Token Refresh)
- **User Management** (CRUD operations for users)
- **Password Management** (Change Password, Reset Password)
- **Role-Based Access Control** (Admin, Manager, Technician, Viewer)

### Technology Stack
- **Frontend Framework:** React 19 + TypeScript + Vite
- **HTTP Client:** Axios
- **State Management:** Redux Toolkit
- **UI Library:** Ant Design v6
- **Authentication:** Bearer Token (JWT-like)

### API Base Configuration

```
Base URL: http://localhost:3000/api
Content-Type: application/json
Authentication: Bearer Token in Authorization header
```

---

## Base Configuration

### Environment Variables (for Backend URL configuration)

Frontend reads the API URL from environment variable:
```
VITE_API_URL = http://localhost:3000/api
VITE_USE_MOCK = false (set to true to use mock API during development)
```

### Request Headers

All requests must include:
```
Content-Type: application/json
Authorization: Bearer {accessToken}  (except /auth/login)
```

---

## Authentication Endpoints

### 1. Login

**Endpoint:** `POST /auth/login`

**Authentication Required:** No

**Description:** User login with username and password. Returns access and refresh tokens.

**Request Body:**
```json
{
  "username": "admin",
  "password": "admin@123"
}
```

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Đăng nhập thành công",
  "data": {
    "user": {
      "id": "1",
      "username": "admin",
      "fullName": "Nguyễn Văn Admin",
      "email": "admin@evn.com",
      "role": "Admin",
      "status": "Active",
      "mustChangePassword": false,
      "createdAt": "2026-01-01T00:00:00Z",
      "updatedAt": "2026-01-01T00:00:00Z"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    }
  },
  "success": true
}
```

**Error Response (401):**
```json
{
  "statusCode": 401,
  "message": "Sai tên đăng nhập hoặc mật khẩu",
  "success": false
}
```

**Error Response (423 - Account Locked/Inactive):**
```json
{
  "statusCode": 423,
  "message": "Tài khoản đã bị vô hiệu hóa hoặc khóa",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/userService.ts - handled via useAuth hook
const response = await axiosClient.post('/auth/login', {
  username,
  password
});
```

---

### 2. Logout

**Endpoint:** `POST /auth/logout`

**Authentication Required:** Yes (Bearer Token)

**Description:** Clear user session and invalidate tokens.

**Request Body:** Empty or null

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Đăng xuất thành công",
  "data": null,
  "success": true
}
```

**Frontend Implementation:**
```typescript
// Called from useAuth hook logout() method
await axiosClient.post('/auth/logout');
```

---

### 3. Refresh Token

**Endpoint:** `POST /auth/refresh`

**Authentication Required:** No (uses refreshToken)

**Description:** Get new access token using refresh token. Called automatically when access token expires (401 response).

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Token đã được làm mới",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  },
  "success": true
}
```

**Error Response (401 - Refresh Token Invalid):**
```json
{
  "statusCode": 401,
  "message": "Refresh token không hợp lệ hoặc đã hết hạn",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/axiosClient.ts - response interceptor handles auto-refresh
// When 401 is received on non-login endpoints:
const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
  refreshToken: storage.getRefreshToken()
});
```

**Note:** Automatic retry logic is built-in. If refresh fails, user is redirected to login page.

---

### 4. Change Password

**Endpoint:** `POST /auth/change-password`

**Authentication Required:** Yes (Bearer Token)

**Description:** Allows user to change their password. Must provide current password for verification.

**Request Body:**
```json
{
  "currentPassword": "admin@123",
  "newPassword": "newPassword@456"
}
```

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Đổi mật khẩu thành công",
  "data": {
    "id": "1",
    "username": "admin",
    "fullName": "Nguyễn Văn Admin",
    "email": "admin@evn.com",
    "role": "Admin",
    "status": "Active",
    "mustChangePassword": false,
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-06-15T10:30:00Z"
  },
  "success": true
}
```

**Error Response (400 - Invalid Current Password):**
```json
{
  "statusCode": 400,
  "message": "Không thể đổi mật khẩu",
  "success": false
}
```

**Error Response (401 - Not Authenticated):**
```json
{
  "statusCode": 401,
  "message": "Chưa đăng nhập",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/hooks/useAuth.ts - changePassword() method
await axiosClient.post('/auth/change-password', {
  currentPassword,
  newPassword
});
```

---

## User Management Endpoints

### 5. Get All Users

**Endpoint:** `GET /users`

**Authentication Required:** Yes (Bearer Token)

**Authorization Required:** Admin or Manager role

**Description:** Retrieve list of all users in the system.

**Query Parameters:** None (pagination may be added later)

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Lấy danh sách thành công",
  "data": [
    {
      "id": "1",
      "username": "admin",
      "fullName": "Nguyễn Văn Admin",
      "email": "admin@evn.com",
      "role": "Admin",
      "status": "Active",
      "mustChangePassword": false,
      "createdAt": "2026-01-01T00:00:00Z",
      "updatedAt": "2026-01-01T00:00:00Z"
    },
    {
      "id": "2",
      "username": "manager",
      "fullName": "Trần Thị Manager",
      "email": "manager@evn.com",
      "role": "Manager",
      "status": "Active",
      "mustChangePassword": false,
      "createdAt": "2026-01-02T00:00:00Z",
      "updatedAt": "2026-01-02T00:00:00Z"
    },
    {
      "id": "3",
      "username": "technician",
      "fullName": "Lê Văn Technician",
      "email": "tech@evn.com",
      "role": "Technician",
      "status": "Active",
      "mustChangePassword": false,
      "createdAt": "2026-01-03T00:00:00Z",
      "updatedAt": "2026-01-03T00:00:00Z"
    }
  ],
  "success": true
}
```

**Error Response (403 - Insufficient Permissions):**
```json
{
  "statusCode": 403,
  "message": "Không có quyền truy cập",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/userService.ts
const response = await axiosClient.get<ApiResponse<User[]>>('/users');
return response.data.data;
```

---

### 6. Create User

**Endpoint:** `POST /users`

**Authentication Required:** Yes (Bearer Token)

**Authorization Required:** Admin or Manager role

**Description:** Create a new user account. System generates username and temporary password.

**Request Body:**
```json
{
  "fullName": "Phạm Văn Test",
  "role": "Technician"
}
```

**Success Response (201):**
```json
{
  "statusCode": 201,
  "message": "Tạo tài khoản thành công",
  "data": {
    "user": {
      "id": "5",
      "username": "pham_van_test",
      "fullName": "Phạm Văn Test",
      "email": "pham_van_test@evn.com",
      "role": "Technician",
      "status": "Active",
      "mustChangePassword": true,
      "createdAt": "2026-06-15T10:45:00Z",
      "updatedAt": "2026-06-15T10:45:00Z"
    },
    "username": "pham_van_test",
    "temporaryPassword": "TempPass@123456"
  },
  "success": true
}
```

**Error Response (400 - Missing Required Fields):**
```json
{
  "statusCode": 400,
  "message": "Họ tên và vai trò là bắt buộc",
  "success": false
}
```

**Error Response (403 - Insufficient Permissions):**
```json
{
  "statusCode": 403,
  "message": "Không có quyền tạo tài khoản",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/userService.ts
const response = await axiosClient.post<ApiResponse<CreateUserResponse>>('/users', {
  fullName,
  role
});
return response.data.data;
```

**Response Type:**
```typescript
interface CreateUserResponse {
  user: User;
  username: string;
  temporaryPassword: string;
}
```

---

### 7. Update User

**Endpoint:** `PATCH /users/{id}`

**Authentication Required:** Yes (Bearer Token)

**Authorization Required:** Admin or Manager role

**Description:** Update user role or status. Cannot modify username or email.

**URL Parameters:**
- `id` (string, required): User ID to update

**Request Body:**
```json
{
  "role": "Manager",
  "status": "Inactive"
}
```

**Note:** Only include fields to update. Both fields are optional but at least one must be provided.

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Cập nhật thành công",
  "data": {
    "id": "2",
    "username": "manager",
    "fullName": "Trần Thị Manager",
    "email": "manager@evn.com",
    "role": "Manager",
    "status": "Inactive",
    "mustChangePassword": false,
    "createdAt": "2026-01-02T00:00:00Z",
    "updatedAt": "2026-06-15T10:50:00Z"
  },
  "success": true
}
```

**Error Response (404 - User Not Found):**
```json
{
  "statusCode": 404,
  "message": "Không tìm thấy người dùng",
  "success": false
}
```

**Error Response (403 - Cannot Modify Admin):**
```json
{
  "statusCode": 403,
  "message": "Không thể sửa đổi tài khoản Admin",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/userService.ts
const response = await axiosClient.patch<ApiResponse<User>>(
  `/users/${id}`,
  { role, status }
);
return response.data.data;
```

---

### 8. Reset User Password

**Endpoint:** `POST /users/{id}/reset-password`

**Authentication Required:** Yes (Bearer Token)

**Authorization Required:** Admin or Manager role

**Description:** Admin/Manager resets user password to temporary password. User must change password on next login.

**URL Parameters:**
- `id` (string, required): User ID whose password to reset

**Request Body:** Empty or null

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Đặt lại mật khẩu thành công",
  "data": {
    "username": "technician",
    "temporaryPassword": "ResetPass@789012"
  },
  "success": true
}
```

**Error Response (404 - User Not Found):**
```json
{
  "statusCode": 404,
  "message": "Không tìm thấy người dùng",
  "success": false
}
```

**Error Response (403 - Cannot Reset Admin Password):**
```json
{
  "statusCode": 403,
  "message": "Không thể đặt lại mật khẩu tài khoản Admin",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/userService.ts
const response = await axiosClient.post<ApiResponse<ResetPasswordResponse>>(
  `/users/${id}/reset-password`
);
return response.data.data;
```

**Response Type:**
```typescript
interface ResetPasswordResponse {
  username: string;
  temporaryPassword: string;
}
```

---

### 9. Delete User

**Endpoint:** `DELETE /users/{id}`

**Authentication Required:** Yes (Bearer Token)

**Authorization Required:** Admin role only

**Description:** Delete a user account. Cannot delete Admin accounts. Soft or hard delete is implementation choice.

**URL Parameters:**
- `id` (string, required): User ID to delete

**Request Body:** Empty or null

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Xóa người dùng thành công",
  "data": null,
  "success": true
}
```

**Error Response (404 - User Not Found):**
```json
{
  "statusCode": 404,
  "message": "Không tìm thấy người dùng",
  "success": false
}
```

**Error Response (403 - Cannot Delete Admin):**
```json
{
  "statusCode": 403,
  "message": "Không thể xóa tài khoản quản trị viên",
  "success": false
}
```

**Error Response (403 - Insufficient Permissions):**
```json
{
  "statusCode": 403,
  "message": "Chỉ Admin mới có thể xóa tài khoản",
  "success": false
}
```

**Frontend Implementation:**
```typescript
// src/services/api/userService.ts
await axiosClient.delete(`/users/${id}`);
```

---

## Response Format

### Standard API Response Structure

All API responses follow this format:

```typescript
interface ApiResponse<T> {
  statusCode: number;      // HTTP status code (200, 201, 400, 401, 403, 404, 423, 500)
  message: string;         // Human-readable message (Vietnamese)
  data: T;                 // Response payload (null for error responses)
  success: boolean;        // true for success, false for error
}
```

### Example Success Response
```json
{
  "statusCode": 200,
  "message": "Lấy danh sách thành công",
  "data": [...],
  "success": true
}
```

### Example Error Response
```json
{
  "statusCode": 401,
  "message": "Sai tên đăng nhập hoặc mật khẩu",
  "data": null,
  "success": false
}
```

---

## Error Handling

### HTTP Status Codes

| Status Code | Meaning | When Used |
|------------|---------|-----------|
| 200 | OK | Successful GET, PATCH, or DELETE request |
| 201 | Created | Successful POST request (resource created) |
| 400 | Bad Request | Invalid request body, missing required fields |
| 401 | Unauthorized | Missing/invalid token, login failed, token expired |
| 403 | Forbidden | Insufficient permissions for the operation |
| 404 | Not Found | Resource (user) not found |
| 423 | Locked | User account locked or inactive |
| 500 | Internal Server Error | Unexpected server error |

### Frontend Error Handling

The Frontend implements automatic error handling in `axiosClient.ts`:

1. **401 Errors (Token Expired):**
   - Automatically attempts to refresh token using refreshToken
   - Retries original request with new token
   - If refresh fails or no refreshToken available: redirects to login

2. **423 Errors (Account Locked):**
   - Displays error message to user
   - Prevents retry of login

3. **Other Errors:**
   - Displays error message from API response
   - User can retry manually

---

## User Data Types

### User Entity

```typescript
interface User {
  id: string;                    // Unique user identifier (UUID or numeric)
  username: string;              // Unique login name (auto-generated from fullName)
  fullName: string;              // User's full name
  email: string;                 // Email address (auto-generated from username)
  role: UserRole;                // One of: "Admin", "Manager", "Technician", "Viewer"
  status: UserStatus;            // One of: "Active", "Inactive", "Locked"
  mustChangePassword?: boolean;  // true if user must change password on next login
  createdAt: string;             // ISO 8601 timestamp
  updatedAt: string;             // ISO 8601 timestamp
}

type UserRole = "Admin" | "Manager" | "Technician" | "Viewer";
type UserStatus = "Active" | "Inactive" | "Locked";
```

### Authentication Tokens

```typescript
interface AuthTokens {
  accessToken: string;      // JWT or similar, used in Authorization header
  refreshToken: string;     // Long-lived token for obtaining new access token
}
```

---

## Testing Credentials

### Test Accounts (for Manual Testing)

```
┌───────────┬──────────────┬────────────┬──────────┐
│ Username  │ Password     │ Role       │ Status   │
├───────────┼──────────────┼────────────┼──────────┤
│ admin     │ admin@123    │ Admin      │ Active   │
│ manager   │ manager@123  │ Manager    │ Active   │
│ technician│ tech@123     │ Technician │ Active   │
│ locked    │ locked@123   │ Viewer     │ Locked   │
└───────────┴──────────────┴────────────┴──────────┘
```

### Testing Workflows

**1. Login → Change Password → Logout**
```
POST /auth/login                    (admin / admin@123)
POST /auth/change-password          (after login)
POST /auth/logout
```

**2. User Management (Admin)**
```
GET /users                          (list all users)
POST /users                         (create new user)
PATCH /users/{id}                   (update user role/status)
POST /users/{id}/reset-password     (reset password)
DELETE /users/{id}                  (delete user)
```

**3. Token Refresh**
```
POST /auth/login                    (get tokens)
[Wait for token to expire]
POST /auth/refresh                  (automatic - tested by making request after expiry)
```

---

## Implementation Notes for Backend Team

### Authentication Flow

1. User logs in with username/password
2. Backend validates credentials and returns `accessToken` + `refreshToken`
3. Frontend stores tokens in localStorage/sessionStorage
4. Frontend includes `Authorization: Bearer {accessToken}` in all requests
5. When accessToken expires, Frontend automatically calls `/auth/refresh`
6. Backend returns new tokens without user action (silent refresh)
7. If refreshToken is also expired/invalid, user is redirected to login

### Password Generation Requirements

When creating users or resetting passwords, temporary passwords should:
- Be random and secure (minimum 12 characters recommended)
- Include uppercase, lowercase, numbers, and special characters
- Example: `TempPass@123456` or similar pattern
- Frontend displays password in modal for admin to copy and share

### Username Generation

When creating users, auto-generated username should:
- Be based on fullName
- Be lowercase
- Replace spaces with underscores
- Be unique (check for collisions)
- Example: "Phạm Văn Test" → "pham_van_test"

### Email Assignment

Auto-generated email should:
- Use pattern: `{username}@evn.com`
- Be unique in the system
- Example: "pham_van_test" → "pham_van_test@evn.com"

### Role Permissions

| Role | Can View Users | Can Create Users | Can Update Users | Can Delete Users | Can Reset Password |
|------|---|---|---|---|---|
| Admin | Yes | Yes | Yes | Yes | Yes |
| Manager | Yes | Yes | Yes | No | Yes |
| Technician | No | No | No | No | No |
| Viewer | No | No | No | No | No |

### Required Validations

**Username & Email:**
- Must be unique
- Username: alphanumeric + underscore only
- Email: valid email format

**Password:**
- Current password validation (when changing password)
- Cannot be same as previous password (optional but recommended)

**User Status:**
- Cannot change status of only remaining Admin

**Delete Operation:**
- Cannot delete Admin accounts
- Only Admin role can delete users

### Database Considerations

**Indexes:**
- `username` (unique)
- `email` (unique)
- `role` (for filtering)
- `status` (for filtering)

**Timestamps:**
- Store in UTC ISO 8601 format: `2026-06-15T10:45:00Z`
- Update `updatedAt` on every modification

**Soft Deletes (Recommended):**
- Add `deletedAt` field (nullable)
- Exclude soft-deleted users from list queries unless explicitly requested

---

## Rate Limiting & Security

### Recommended Security Measures

1. **Rate Limiting:**
   - `/auth/login`: max 5 attempts per minute per IP
   - Other endpoints: reasonable limits to prevent abuse

2. **CORS Configuration:**
   ```
   Allow-Origin: http://localhost:5173 (dev), https://yourdomain.com (prod)
   Allow-Methods: GET, POST, PATCH, DELETE
   Allow-Headers: Content-Type, Authorization
   ```

3. **Token Expiration:**
   - accessToken: 15-30 minutes
   - refreshToken: 7-30 days

4. **Password Requirements:**
   - Minimum 6 characters (Frontend validation)
   - Backend should enforce stricter requirements (8+ with complexity)

---

## Additional Notes

- All timestamps should be in **ISO 8601 format** (UTC)
- All responses must include `success` field for easy client-side checking
- Messages should be in **Vietnamese** to match Frontend UI
- Frontend uses `@utils/storage` for token management (localStorage wrapper)
- Frontend has built-in automatic token refresh with retry logic
- All requests are authenticated except `/auth/login`

---

## Contact & Support

For questions regarding this specification:
- Frontend Location: `d:\UavPms_Org\Frontend`
- Backend Location: `d:\UavPms_Org\Backend`
- Frontend API Services: `src/services/api/`
- Mock Data: `src/mocks/mockData.ts`

