# API Test Cases - Frontend Requirements

**Version:** 1.0  
**Last Updated:** 2026-06-15  
**Purpose:** Comprehensive test cases for Backend API development and verification

## Test Environment Setup

### Base Configuration
```
Base URL: http://localhost:3000/api
Content-Type: application/json
Test Framework: xUnit or MSTest (for Backend team)
HTTP Client: Postman, Insomnia, or REST Client extension
```

### Test Data
Use the following test accounts for all test cases:

| Username  | Password   | Role       | Status  | Expected Behavior |
|-----------|-----------|-----------|---------|------------------|
| admin     | admin@123 | Admin      | Active  | Full access |
| manager   | manager@123 | Manager    | Active  | Can manage users (except delete) |
| technician| tech@123  | Technician | Active  | Limited access |
| locked    | locked@123| Viewer     | Locked  | Login should fail (423) |

---

## Test Cases

### 1. Authentication Test Suite

#### 1.1 POST /auth/login - Valid Credentials

**Test ID:** AUTH-001  
**Priority:** Critical  
**Precondition:** Database contains test users

**Test Steps:**
1. Send POST request to `/auth/login`
2. Provide valid admin credentials: `{"username": "admin", "password": "admin@123"}`

**Expected Result:**
- Status Code: **200 OK**
- Response body:
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
        "accessToken": "<valid JWT token>",
        "refreshToken": "<valid refresh token>"
      }
    },
    "success": true
  }
  ```
- Response time: < 500ms

**Postman Test:**
```javascript
pm.test("Status code is 200", () => {
  pm.response.to.have.status(200);
});

pm.test("Response has valid structure", () => {
  const json = pm.response.json();
  pm.expect(json.statusCode).to.equal(200);
  pm.expect(json.success).to.be.true;
  pm.expect(json.data.tokens.accessToken).to.be.a('string');
  pm.expect(json.data.tokens.refreshToken).to.be.a('string');
});

// Store token for subsequent requests
pm.environment.set("accessToken", pm.response.json().data.tokens.accessToken);
pm.environment.set("refreshToken", pm.response.json().data.tokens.refreshToken);
```

---

#### 1.2 POST /auth/login - Invalid Username

**Test ID:** AUTH-002  
**Priority:** High  
**Precondition:** None

**Test Steps:**
1. Send POST request to `/auth/login`
2. Provide non-existent username: `{"username": "nonexistent", "password": "password@123"}`

**Expected Result:**
- Status Code: **401 Unauthorized**
- Response:
  ```json
  {
    "statusCode": 401,
    "message": "Sai tên đăng nhập hoặc mật khẩu",
    "data": null,
    "success": false
  }
  ```

---

#### 1.3 POST /auth/login - Invalid Password

**Test ID:** AUTH-003  
**Priority:** High  
**Precondition:** Database contains admin user

**Test Steps:**
1. Send POST request to `/auth/login`
2. Provide correct username but wrong password: `{"username": "admin", "password": "wrongpassword"}`

**Expected Result:**
- Status Code: **401 Unauthorized**
- Same message: "Sai tên đăng nhập hoặc mật khẩu"

---

#### 1.4 POST /auth/login - Locked Account

**Test ID:** AUTH-004  
**Priority:** High  
**Precondition:** Locked test account exists

**Test Steps:**
1. Send POST request to `/auth/login`
2. Provide locked account credentials: `{"username": "locked", "password": "locked@123"}`

**Expected Result:**
- Status Code: **423 Locked**
- Response:
  ```json
  {
    "statusCode": 423,
    "message": "Tài khoản đã bị vô hiệu hóa hoặc khóa",
    "data": null,
    "success": false
  }
  ```

---

#### 1.5 POST /auth/login - Missing Username

**Test ID:** AUTH-005  
**Priority:** Medium  
**Precondition:** None

**Test Steps:**
1. Send POST request to `/auth/login`
2. Omit username: `{"password": "admin@123"}`

**Expected Result:**
- Status Code: **400 Bad Request** or **401 Unauthorized**
- Error message indicates missing required field

---

#### 1.6 POST /auth/login - Missing Password

**Test ID:** AUTH-006  
**Priority:** Medium  
**Precondition:** None

**Test Steps:**
1. Send POST request to `/auth/login`
2. Omit password: `{"username": "admin"}`

**Expected Result:**
- Status Code: **400 Bad Request** or **401 Unauthorized**
- Error message indicates missing required field

---

#### 1.7 POST /auth/logout - Valid Token

**Test ID:** AUTH-007  
**Priority:** High  
**Precondition:** User is logged in with valid accessToken

**Test Steps:**
1. Login first (AUTH-001) to get accessToken
2. Send POST request to `/auth/logout`
3. Include header: `Authorization: Bearer {accessToken}`

**Expected Result:**
- Status Code: **200 OK**
- Response:
  ```json
  {
    "statusCode": 200,
    "message": "Đăng xuất thành công",
    "data": null,
    "success": true
  }
  ```

---

#### 1.8 POST /auth/logout - Without Authentication

**Test ID:** AUTH-008  
**Priority:** High  
**Precondition:** None

**Test Steps:**
1. Send POST request to `/auth/logout` without Authorization header

**Expected Result:**
- Status Code: **401 Unauthorized**
- Error message: "Chưa đăng nhập" or "Invalid token"

---

#### 1.9 POST /auth/refresh - Valid Refresh Token

**Test ID:** AUTH-009  
**Priority:** Critical  
**Precondition:** User has valid refreshToken

**Test Steps:**
1. Login to get tokens (AUTH-001)
2. Send POST request to `/auth/refresh`
3. Body: `{"refreshToken": "<valid refresh token>"}`

**Expected Result:**
- Status Code: **200 OK**
- Response:
  ```json
  {
    "statusCode": 200,
    "message": "Token đã được làm mới",
    "data": {
      "accessToken": "<new valid JWT token>",
      "refreshToken": "<new or same refresh token>"
    },
    "success": true
  }
  ```
- New accessToken is different from old one

---

#### 1.10 POST /auth/refresh - Invalid Refresh Token

**Test ID:** AUTH-010  
**Priority:** High  
**Precondition:** None

**Test Steps:**
1. Send POST request to `/auth/refresh`
2. Body: `{"refreshToken": "invalid_token_xyz"}`

**Expected Result:**
- Status Code: **401 Unauthorized**
- Response message: "Refresh token không hợp lệ hoặc đã hết hạn"

---

#### 1.11 POST /auth/change-password - Valid Change

**Test ID:** AUTH-011  
**Priority:** High  
**Precondition:** User is logged in with accessToken

**Test Steps:**
1. Login first (AUTH-001)
2. Send POST request to `/auth/change-password`
3. Header: `Authorization: Bearer {accessToken}`
4. Body:
   ```json
   {
     "currentPassword": "admin@123",
     "newPassword": "NewPassword@456"
   }
   ```

**Expected Result:**
- Status Code: **200 OK**
- Response:
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
- Verify: New password works for login, old password doesn't

---

#### 1.12 POST /auth/change-password - Wrong Current Password

**Test ID:** AUTH-012  
**Priority:** High  
**Precondition:** User is logged in

**Test Steps:**
1. Login to get accessToken
2. Send POST to `/auth/change-password`
3. Provide wrong current password:
   ```json
   {
     "currentPassword": "wrongpassword",
     "newPassword": "NewPassword@456"
   }
   ```

**Expected Result:**
- Status Code: **400 Bad Request**
- Response: "Không thể đổi mật khẩu"

---

#### 1.13 POST /auth/change-password - Without Authentication

**Test ID:** AUTH-013  
**Priority:** High  
**Precondition:** None

**Test Steps:**
1. Send POST to `/auth/change-password` without Authorization header

**Expected Result:**
- Status Code: **401 Unauthorized**

---

### 2. User Management Test Suite

#### 2.1 GET /users - Admin Access

**Test ID:** USER-001  
**Priority:** Critical  
**Precondition:** Admin user is logged in

**Test Steps:**
1. Login as admin (AUTH-001) to get accessToken
2. Send GET request to `/users`
3. Header: `Authorization: Bearer {accessToken}`

**Expected Result:**
- Status Code: **200 OK**
- Response contains array of all users:
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
      // ... more users
    ],
    "success": true
  }
  ```
- Array contains at least 3+ users
- No password field in response

---

#### 2.2 GET /users - Manager Access

**Test ID:** USER-002  
**Priority:** High  
**Precondition:** Manager user is logged in

**Test Steps:**
1. Login as manager to get accessToken
2. Send GET request to `/users`
3. Header: `Authorization: Bearer {accessToken}`

**Expected Result:**
- Status Code: **200 OK**
- Response contains full user list (same as admin)

---

#### 2.3 GET /users - Technician Access

**Test ID:** USER-003  
**Priority:** High  
**Precondition:** Technician user is logged in

**Test Steps:**
1. Login as technician to get accessToken
2. Send GET request to `/users`

**Expected Result:**
- Status Code: **403 Forbidden**
- Response: "Không có quyền truy cập"

---

#### 2.4 GET /users - Without Authentication

**Test ID:** USER-004  
**Priority:** High  
**Precondition:** None

**Test Steps:**
1. Send GET request to `/users` without Authorization header

**Expected Result:**
- Status Code: **401 Unauthorized**

---

#### 2.5 POST /users - Admin Create User

**Test ID:** USER-005  
**Priority:** Critical  
**Precondition:** Admin is logged in

**Test Steps:**
1. Login as admin
2. Send POST request to `/users`
3. Body:
   ```json
   {
     "fullName": "Phạm Văn Test",
     "role": "Technician"
   }
   ```

**Expected Result:**
- Status Code: **201 Created**
- Response:
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
      "temporaryPassword": "<strong random password>"
    },
    "success": true
  }
  ```
- Generated username is unique
- Generated email is unique
- temporaryPassword is provided
- mustChangePassword is true

---

#### 2.6 POST /users - Manager Create User

**Test ID:** USER-006  
**Priority:** High  
**Precondition:** Manager is logged in

**Test Steps:**
1. Login as manager
2. Send POST request to `/users` with same body as USER-005

**Expected Result:**
- Status Code: **201 Created**
- Same success response as USER-005

---

#### 2.7 POST /users - Technician Cannot Create User

**Test ID:** USER-007  
**Priority:** High  
**Precondition:** Technician is logged in

**Test Steps:**
1. Login as technician
2. Send POST request to `/users`

**Expected Result:**
- Status Code: **403 Forbidden**

---

#### 2.8 POST /users - Missing fullName

**Test ID:** USER-008  
**Priority:** Medium  
**Precondition:** Admin is logged in

**Test Steps:**
1. Login as admin
2. Send POST to `/users` with body:
   ```json
   {
     "role": "Technician"
   }
   ```

**Expected Result:**
- Status Code: **400 Bad Request**
- Message: "Họ tên và vai trò là bắt buộc"

---

#### 2.9 POST /users - Missing Role

**Test ID:** USER-009  
**Priority:** Medium  
**Precondition:** Admin is logged in

**Test Steps:**
1. Login as admin
2. Send POST to `/users` with body:
   ```json
   {
     "fullName": "Test User"
   }
   ```

**Expected Result:**
- Status Code: **400 Bad Request**
- Message: "Họ tên và vai trò là bắt buộc"

---

#### 2.10 POST /users - Empty fullName

**Test ID:** USER-010  
**Priority:** Medium  
**Precondition:** Admin is logged in

**Test Steps:**
1. Login as admin
2. Send POST with:
   ```json
   {
     "fullName": "",
     "role": "Technician"
   }
   ```

**Expected Result:**
- Status Code: **400 Bad Request**

---

#### 2.11 PATCH /users/{id} - Update User Role

**Test ID:** USER-011  
**Priority:** High  
**Precondition:** Admin is logged in, user with id "2" exists

**Test Steps:**
1. Login as admin
2. Send PATCH request to `/users/2`
3. Body:
   ```json
   {
     "role": "Manager"
   }
   ```

**Expected Result:**
- Status Code: **200 OK**
- Response:
  ```json
  {
    "statusCode": 200,
    "message": "Cập nhật thành công",
    "data": {
      "id": "2",
      "username": "manager",
      "fullName": "Trần Thị Manager",
      "email": "manager@evn.com",
      "role": "Manager",  // Updated
      "status": "Active",
      "mustChangePassword": false,
      "createdAt": "2026-01-02T00:00:00Z",
      "updatedAt": "2026-06-15T11:00:00Z"
    },
    "success": true
  }
  ```
- Role is updated
- UpdatedAt is newer

---

#### 2.12 PATCH /users/{id} - Update User Status

**Test ID:** USER-012  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Login as admin
2. Send PATCH to `/users/2` with:
   ```json
   {
     "status": "Inactive"
   }
   ```

**Expected Result:**
- Status Code: **200 OK**
- Status is updated to "Inactive"

---

#### 2.13 PATCH /users/{id} - Update Both Role and Status

**Test ID:** USER-013  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Send PATCH to `/users/3` with:
   ```json
   {
     "role": "Technician",
     "status": "Active"
   }
   ```

**Expected Result:**
- Status Code: **200 OK**
- Both fields are updated

---

#### 2.14 PATCH /users/{id} - Non-existent User

**Test ID:** USER-014  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Send PATCH to `/users/999999`
2. Body: `{"role": "Admin"}`

**Expected Result:**
- Status Code: **404 Not Found**
- Message: "Không tìm thấy người dùng"

---

#### 2.15 PATCH /users/{id} - Unauthorized Role Change

**Test ID:** USER-015  
**Priority:** Medium  
**Precondition:** Technician is logged in

**Test Steps:**
1. Login as technician
2. Send PATCH to `/users/2`

**Expected Result:**
- Status Code: **403 Forbidden**

---

#### 2.16 POST /users/{id}/reset-password - Valid Reset

**Test ID:** USER-016  
**Priority:** Critical  
**Precondition:** Admin is logged in, user with id "2" exists

**Test Steps:**
1. Login as admin
2. Send POST request to `/users/2/reset-password`
3. No body required

**Expected Result:**
- Status Code: **200 OK**
- Response:
  ```json
  {
    "statusCode": 200,
    "message": "Đặt lại mật khẩu thành công",
    "data": {
      "username": "manager",
      "temporaryPassword": "<new random password>"
    },
    "success": true
  }
  ```
- New temporary password is provided
- Verify: User can login with new temporary password
- Verify: User's mustChangePassword is set to true

---

#### 2.17 POST /users/{id}/reset-password - Non-existent User

**Test ID:** USER-017  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Send POST to `/users/999999/reset-password`

**Expected Result:**
- Status Code: **404 Not Found**
- Message: "Không tìm thấy người dùng"

---

#### 2.18 POST /users/{id}/reset-password - Cannot Reset Admin

**Test ID:** USER-018  
**Priority:** High  
**Precondition:** Admin is logged in, user "1" is Admin

**Test Steps:**
1. Send POST to `/users/1/reset-password`

**Expected Result:**
- Status Code: **403 Forbidden**
- Message: "Không thể đặt lại mật khẩu tài khoản Admin"

---

#### 2.19 POST /users/{id}/reset-password - Unauthorized

**Test ID:** USER-019  
**Priority:** High  
**Precondition:** Technician is logged in

**Test Steps:**
1. Login as technician
2. Send POST to `/users/2/reset-password`

**Expected Result:**
- Status Code: **403 Forbidden**

---

#### 2.20 DELETE /users/{id} - Admin Delete User

**Test ID:** USER-020  
**Priority:** Critical  
**Precondition:** Admin is logged in, user "3" exists and is not admin

**Test Steps:**
1. Login as admin
2. Send DELETE request to `/users/3`

**Expected Result:**
- Status Code: **200 OK**
- Response:
  ```json
  {
    "statusCode": 200,
    "message": "Xóa người dùng thành công",
    "data": null,
    "success": true
  }
  ```
- Verify: GET /users no longer contains user "3"

---

#### 2.21 DELETE /users/{id} - Non-existent User

**Test ID:** USER-021  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Send DELETE to `/users/999999`

**Expected Result:**
- Status Code: **404 Not Found**
- Message: "Không tìm thấy người dùng"

---

#### 2.22 DELETE /users/{id} - Cannot Delete Admin

**Test ID:** USER-022  
**Priority:** Critical  
**Precondition:** Admin is logged in, user "1" is Admin

**Test Steps:**
1. Send DELETE to `/users/1`

**Expected Result:**
- Status Code: **403 Forbidden**
- Message: "Không thể xóa tài khoản quản trị viên"

---

#### 2.23 DELETE /users/{id} - Manager Cannot Delete

**Test ID:** USER-023  
**Priority:** High  
**Precondition:** Manager is logged in

**Test Steps:**
1. Login as manager
2. Send DELETE to `/users/3`

**Expected Result:**
- Status Code: **403 Forbidden**
- Message: "Chỉ Admin mới có thể xóa tài khoản"

---

#### 2.24 DELETE /users/{id} - Technician Cannot Delete

**Test ID:** USER-024  
**Priority:** High  
**Precondition:** Technician is logged in

**Test Steps:**
1. Login as technician
2. Send DELETE to `/users/3`

**Expected Result:**
- Status Code: **403 Forbidden**

---

### 3. Authorization Test Suite

#### 3.1 Verify Role-Based Access Control

**Test ID:** AUTHZ-001  
**Priority:** Critical  
**Precondition:** All test accounts are available

**Test Steps:**
Create a matrix and test each endpoint with each role:

| Endpoint | Admin | Manager | Technician | Viewer |
|----------|-------|---------|-----------|--------|
| POST /auth/login | ✓ | ✓ | ✓ | ✓ |
| POST /auth/logout | ✓ | ✓ | ✓ | ✓ |
| POST /auth/change-password | ✓ | ✓ | ✓ | ✓ |
| GET /users | ✓ | ✓ | ✗ | ✗ |
| POST /users | ✓ | ✓ | ✗ | ✗ |
| PATCH /users/{id} | ✓ | ✓ | ✗ | ✗ |
| POST /users/{id}/reset-password | ✓ | ✓ | ✗ | ✗ |
| DELETE /users/{id} | ✓ | ✗ | ✗ | ✗ |

✓ = Allowed (200/201)  
✗ = Denied (403)

---

### 4. Data Integrity Test Suite

#### 4.1 Username Uniqueness

**Test ID:** DATA-001  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Create user "Nguyễn Văn Unique1" → username "nguyen_van_unique1"
2. Try creating "Nguyễn Văn Unique1" again
3. Or try creating "Nguyễn  Văn  Unique1" (different spacing)

**Expected Result:**
- Step 1: Successfully created
- Step 2/3: Should either reject or generate unique variant "nguyen_van_unique1_2"

---

#### 4.2 Email Uniqueness

**Test ID:** DATA-002  
**Priority:** High  
**Precondition:** Admin is logged in

**Test Steps:**
1. Create user "Test User1" → email "test_user1@evn.com"
2. Try creating another user with same generated pattern

**Expected Result:**
- Emails must be unique
- System should prevent duplicate email creation

---

#### 4.3 Timestamp Accuracy

**Test ID:** DATA-003  
**Priority:** Medium  
**Precondition:** Admin is logged in

**Test Steps:**
1. Create a user
2. Check createdAt and updatedAt timestamps
3. Update the user
4. Check updatedAt timestamp

**Expected Result:**
- Timestamps are in ISO 8601 UTC format
- createdAt never changes
- updatedAt is updated on modifications
- updatedAt >= createdAt
- Timestamps are recent (within 1 second of system time)

---

### 5. Error Handling Test Suite

#### 5.1 Verify Error Response Format

**Test ID:** ERROR-001  
**Priority:** High  
**Precondition:** None

**Test Steps:**
1. Make a request that will fail (e.g., invalid login)

**Expected Result:**
- Error response follows format:
  ```json
  {
    "statusCode": <number>,
    "message": "<vietnamese message>",
    "data": null,
    "success": false
  }
  ```
- statusCode matches HTTP status
- Message is descriptive and in Vietnamese
- success is always false for errors

---

#### 5.2 Timeout Handling

**Test ID:** ERROR-002  
**Priority:** Medium  
**Precondition:** Database is accessible

**Test Steps:**
1. Make multiple rapid requests to same endpoint
2. Monitor timeout behavior

**Expected Result:**
- Requests complete within reasonable time (< 5 seconds)
- No hanging requests
- Proper error if server is overloaded

---

#### 5.3 Invalid Request Body

**Test ID:** ERROR-003  
**Priority:** Medium  
**Precondition:** None

**Test Steps:**
1. Send JSON with missing required fields
2. Send invalid JSON format
3. Send wrong data type (e.g., string instead of number)

**Expected Result:**
- Status: **400 Bad Request**
- Clear error message indicating the problem

---

### 6. Security Test Suite

#### 6.1 Password Not Returned

**Test ID:** SEC-001  
**Priority:** Critical  
**Precondition:** Any API call that returns user data

**Test Steps:**
1. Make any API request that returns user(s)
2. Check response body

**Expected Result:**
- Response never contains "password" or "passwordHash" field
- Only temporary passwords are returned during creation/reset

---

#### 6.2 Token Expiration

**Test ID:** SEC-002  
**Priority:** High  
**Precondition:** Access token is obtained

**Test Steps:**
1. Login to get accessToken
2. Wait for token to expire (or mock expiration)
3. Try to use expired token

**Expected Result:**
- Request with expired token: **401 Unauthorized**
- Message indicates token expired
- Refresh token endpoint can get new token

---

#### 6.3 Refresh Token Rotation

**Test ID:** SEC-003  
**Priority:** High  
**Precondition:** User has refreshToken

**Test Steps:**
1. Use refreshToken to get new tokens
2. Try using old refreshToken again
3. (Optional) Try using new refreshToken multiple times

**Expected Result:**
- First use of refreshToken: Success (200)
- Subsequent uses of same refreshToken: Behavior depends on implementation
  - Option A: Still works (no rotation)
  - Option B: Fails (token rotated)

---

#### 6.4 SQL Injection Prevention

**Test ID:** SEC-004  
**Priority:** Critical  
**Precondition:** None

**Test Steps:**
1. Try login with username: `admin' OR '1'='1`
2. Try login with password: `anything' OR '1'='1`
3. Try creating user with special SQL characters

**Expected Result:**
- All queries fail or return correct results
- No error messages revealing SQL structure
- Status: **401** for failed login (not 500)

---

#### 6.5 CORS Validation

**Test ID:** SEC-005  
**Priority:** High  
**Precondition:** API is accessible

**Test Steps:**
1. Make request from different origin (e.g., http://localhost:3000)
2. Make request from allowed origin (http://localhost:5173)

**Expected Result:**
- Response includes proper CORS headers
- Disallowed origins are blocked
- Allowed origins get full access

---

## Test Execution Report Template

```
Test Execution Report
Date: [YYYY-MM-DD]
Tester: [Name]
Build Version: [Version]

| Test ID | Status | Notes | Time |
|---------|--------|-------|------|
| AUTH-001 | ✓ PASS | - | 0.2s |
| AUTH-002 | ✓ PASS | - | 0.15s |
| ... | | | |

Total Tests: XX
Passed: XX
Failed: 0
Skipped: 0
Success Rate: 100%

Critical Issues: None
High Priority Issues: None
```

---

## Performance Benchmarks (Optional)

- Login endpoint: < 500ms
- User list retrieval: < 1000ms for 1000+ users
- Create user: < 1000ms
- Update user: < 500ms
- Delete user: < 500ms

---

## Notes for Backend Team

- Execute tests in provided order for dependency management
- Use test data provided above
- Verify both positive and negative test cases
- Check error message text exactly matches specification
- Validate timestamps are in UTC ISO 8601 format
- Ensure no sensitive data in responses
- Test with various input sizes and special characters
- Monitor database state after each test

