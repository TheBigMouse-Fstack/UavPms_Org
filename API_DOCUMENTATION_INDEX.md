# UAV PMS - Backend API Documentation Index

**Project Name:** UAV PMS (Unmanned Aerial Vehicle Persistent Monitoring System)  
**Version:** 1.0  
**Last Updated:** 2026-06-15  
**Frontend Build Status:** ✅ Build Successful

---

## 📚 Documentation Files

### 1. **FRONTEND_API_SPECIFICATION.md**
   - **Purpose:** Complete API specification based on Frontend requirements
   - **Audience:** Backend developers, Frontend developers
   - **Contents:**
     - Base configuration and environment setup
     - All 9 API endpoints with detailed specifications
     - Request/response examples for each endpoint
     - Error handling and status codes
     - User data types and structures
     - Testing credentials and workflows
     - Rate limiting and security recommendations
   - **Key Sections:**
     - ✅ Authentication Endpoints (Login, Logout, Refresh, Change Password)
     - ✅ User Management Endpoints (CRUD operations)
     - ✅ Response Format Standard
     - ✅ Error Handling Guide
   - **When to Use:** Reference this first for understanding what API needs to be built

---

### 2. **BACKEND_IMPLEMENTATION_GUIDE.md**
   - **Purpose:** Step-by-step guide for implementing the API on Backend
   - **Audience:** Backend developers using .NET/C#
   - **Contents:**
     - Project structure recommendations
     - Core implementation guidelines
     - Database schema design
     - DTOs and entity definitions
     - Complete controller code templates
     - Service implementation patterns
     - Authentication & authorization setup
     - Exception handling middleware
     - Unit and integration test examples
   - **Key Sections:**
     - ✅ Response Wrapper Implementation
     - ✅ User Entity & Database Schema
     - ✅ Endpoint Implementation Checklist
     - ✅ JWT Token Generation
     - ✅ Program.cs Configuration
     - ✅ Testing Requirements
   - **When to Use:** Follow this guide while writing backend code

---

### 3. **BACKEND_API_TEST_CASES.md**
   - **Purpose:** Comprehensive test cases for API validation
   - **Audience:** QA team, Backend developers
   - **Contents:**
     - 24+ detailed test cases with step-by-step instructions
     - Expected results for each test
     - Postman test scripts
     - Authorization matrix (role-based access control)
     - Data integrity tests
     - Error handling tests
     - Security tests (SQL injection, CORS, password validation)
     - Performance benchmarks
   - **Key Test Suites:**
     - ✅ Authentication Tests (13 tests)
     - ✅ User Management Tests (19 tests)
     - ✅ Authorization Tests (1 comprehensive matrix)
     - ✅ Data Integrity Tests
     - ✅ Security Tests (5 tests)
   - **When to Use:** Execute these tests to validate API implementation

---

## 🚀 Quick Start Guide

### For Backend Developers

**Step 1: Read the Specification** (30 min)
- Open: `FRONTEND_API_SPECIFICATION.md`
- Read sections: Overview, Base Configuration, All Endpoints

**Step 2: Plan Implementation** (1 hour)
- Review: `BACKEND_IMPLEMENTATION_GUIDE.md`
- Create project structure
- Plan database schema
- Set up DTOs

**Step 3: Implement Each Endpoint** (6-8 hours)
- Implement Authentication endpoints first
- Then implement User Management endpoints
- Follow the DTO and service templates provided

**Step 4: Test Implementation** (2-3 hours)
- Use: `BACKEND_API_TEST_CASES.md`
- Run through all test cases
- Fix any failures
- Verify error handling

**Step 5: Integration** (1 hour)
- Test with Frontend application
- Verify CORS configuration
- Test token refresh flow

---

## 📋 API Endpoints Summary

### Authentication Endpoints
| Method | Endpoint | Status | Purpose |
|--------|----------|--------|---------|
| POST | `/auth/login` | ✅ | User login with credentials |
| POST | `/auth/logout` | ✅ | User logout and session clear |
| POST | `/auth/refresh` | ✅ | Refresh access token |
| POST | `/auth/change-password` | ✅ | Change password (authenticated) |

### User Management Endpoints
| Method | Endpoint | Status | Purpose |
|--------|----------|--------|---------|
| GET | `/users` | ✅ | Get all users (Admin/Manager only) |
| POST | `/users` | ✅ | Create new user (Admin/Manager only) |
| PATCH | `/users/{id}` | ✅ | Update user role/status (Admin/Manager only) |
| POST | `/users/{id}/reset-password` | ✅ | Reset user password (Admin/Manager only) |
| DELETE | `/users/{id}` | ✅ | Delete user (Admin only) |

**Total Endpoints:** 9  
**Status:** ✅ All designed and documented

---

## 🔐 Security Checklist

- [ ] Password hashing using BCrypt or PBKDF2
- [ ] JWT token validation on every request
- [ ] CORS configured for Frontend origin only
- [ ] SQL injection prevention (parameterized queries)
- [ ] Rate limiting on login endpoint
- [ ] Refresh token blacklist on logout
- [ ] No sensitive data in error messages
- [ ] HTTPS enforced in production
- [ ] Tokens expire appropriately (access: 30min, refresh: 30days)
- [ ] User passwords never returned in API responses

---

## 🗄️ Database Schema

### AppUsers Table
```
Columns:
- Id (PK, string)
- Username (unique, string)
- FullName (string)
- Email (unique, string)
- PasswordHash (string)
- Role (string: Admin|Manager|Technician|Viewer)
- Status (string: Active|Inactive|Locked)
- MustChangePassword (boolean)
- CreatedAt (datetime UTC)
- UpdatedAt (datetime UTC)
- DeletedAt (datetime nullable - for soft delete)

Indexes:
- IX_Username (unique)
- IX_Email (unique)
- IX_Role
- IX_Status
```

---

## 👥 Role Permissions Matrix

| Permission | Admin | Manager | Technician | Viewer |
|------------|-------|---------|-----------|--------|
| View Users | ✅ | ✅ | ❌ | ❌ |
| Create User | ✅ | ✅ | ❌ | ❌ |
| Update User | ✅ | ✅ | ❌ | ❌ |
| Reset Password | ✅ | ✅ | ❌ | ❌ |
| Delete User | ✅ | ❌ | ❌ | ❌ |
| Change Own Password | ✅ | ✅ | ✅ | ✅ |

---

## 📊 Test Credentials

For manual testing and integration tests:

```
Admin Account
  Username: admin
  Password: admin@123
  Role: Admin
  Status: Active

Manager Account
  Username: manager
  Password: manager@123
  Role: Manager
  Status: Active

Technician Account
  Username: technician
  Password: tech@123
  Role: Technician
  Status: Active

Locked Account (for error testing)
  Username: locked
  Password: locked@123
  Role: Viewer
  Status: Locked
```

---

## 🔄 API Flow Examples

### Login → Use API → Logout Flow
```
1. POST /auth/login
   ↓ Returns: accessToken + refreshToken
2. GET /api/users (with Authorization header)
   ↓ Use accessToken
3. [After 30 min] GET /api/users
   ↓ 401 Unauthorized - token expired
4. POST /auth/refresh (with refreshToken)
   ↓ Get new accessToken
5. GET /api/users (with new accessToken)
   ↓ Success
6. POST /auth/logout
   ↓ Session cleared
```

### User Creation Flow
```
1. POST /users (Admin/Manager creates new user)
   ← Receive: temporaryPassword
2. Admin shares username + temporaryPassword with user
3. User logs in with temporary password
4. Frontend detects: mustChangePassword = true
5. Frontend redirects to: /change-password page
6. POST /auth/change-password
   ← User now has permanent password
```

---

## 📝 Response Format

### Success Response (200/201)
```json
{
  "statusCode": 200,
  "message": "Lấy danh sách thành công",
  "data": [ ... ],
  "success": true
}
```

### Error Response (4xx/5xx)
```json
{
  "statusCode": 400,
  "message": "Không thể đổi mật khẩu",
  "data": null,
  "success": false
}
```

---

## 🚨 Common HTTP Status Codes

| Code | Meaning | Use Case |
|------|---------|----------|
| 200 | OK | Successful GET, PATCH, DELETE |
| 201 | Created | Successful POST (resource created) |
| 400 | Bad Request | Invalid input, missing fields |
| 401 | Unauthorized | No token, invalid token, wrong credentials |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 423 | Locked | Account locked or inactive |
| 500 | Server Error | Unexpected error |

---

## ⚡ Performance Targets

- **Login:** < 500ms
- **Get Users List:** < 1000ms
- **Create User:** < 1000ms
- **Update User:** < 500ms
- **Delete User:** < 500ms
- **Token Refresh:** < 300ms

---

## 📞 Integration Points

### Frontend Application
- **Location:** `d:\UavPms_Org\Frontend`
- **API Services:** `src/services/api/userService.ts`
- **Mock Setup:** `src/mocks/setupMockInterceptors.ts`
- **Environment:** `VITE_API_URL`, `VITE_USE_MOCK`

### Backend Application
- **Location:** `d:\UavPms_Org\Backend`
- **Solution:** `UavPms.sln`
- **Main Projects:** UavPms.Core, UavPms.Infrastructure, UavPms.WebApi

---

## 🔗 File References

**Frontend Implementation Files:**
- `src/services/api/userService.ts` - API service implementation
- `src/services/api/axiosClient.ts` - Axios configuration with interceptors
- `src/mocks/mockData.ts` - Mock test data
- `src/mocks/setupMockInterceptors.ts` - Mock API implementation

**Backend Project Files to Create/Modify:**
- `UavPms.Core/Contracts/` - DTOs and request/response types
- `UavPms.Core/Entities/AppUser.cs` - User entity
- `UavPms.Infrastructure/Persistence/ApplicationDbContext.cs` - Database context
- `UavPms.Infrastructure/Repositories/` - Data access layer
- `UavPms.Application/Services/` - Business logic layer
- `UavPms.WebApi/Controllers/` - API controllers

---

## ✅ Implementation Checklist

- [ ] Read all documentation files
- [ ] Create database schema and migrations
- [ ] Implement response wrapper classes
- [ ] Implement user entity and DTOs
- [ ] Implement repositories (CRUD)
- [ ] Implement services (business logic)
- [ ] Implement authentication controller
- [ ] Implement users controller
- [ ] Configure JWT authentication
- [ ] Set up CORS for Frontend
- [ ] Add exception handling middleware
- [ ] Create seed data with test accounts
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Execute all test cases from TEST_CASES.md
- [ ] Verify error responses match specification
- [ ] Test with actual Frontend application
- [ ] Security audit
- [ ] Performance testing
- [ ] Documentation for operations team

---

## 📞 Support & Questions

For questions about:
- **API Specification:** See `FRONTEND_API_SPECIFICATION.md`
- **Implementation Details:** See `BACKEND_IMPLEMENTATION_GUIDE.md`
- **Test Validation:** See `BACKEND_API_TEST_CASES.md`
- **Frontend Code:** Check `d:\UavPms_Org\Frontend\src`
- **Database Design:** See `BACKEND_IMPLEMENTATION_GUIDE.md` - Database Design section

---

## 🎯 Next Steps

1. **Backend Team:**
   - Clone/setup Backend project
   - Review this documentation
   - Follow implementation guide
   - Execute test cases

2. **Frontend Team:**
   - Ensure Frontend is properly configured to point to Backend API
   - Test integration with running Backend
   - Report any API contract issues

3. **DevOps/Infrastructure:**
   - Set up database for Backend
   - Configure environment variables
   - Set up CI/CD pipeline
   - Configure CORS for production

---

**Document Version:** 1.0  
**Status:** ✅ Complete and Ready for Backend Implementation  
**Last Review:** 2026-06-15
