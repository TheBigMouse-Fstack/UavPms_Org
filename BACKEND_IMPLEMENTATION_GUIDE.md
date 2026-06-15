# Backend Implementation Guide - API Development from Frontend Specification

**Version:** 1.0  
**Last Updated:** 2026-06-15  
**Purpose:** Detailed backend implementation guide to satisfy Frontend requirements

## Table of Contents

1. [Project Structure](#project-structure)
2. [Core Implementation Guidelines](#core-implementation-guidelines)
3. [Endpoint Implementation Checklist](#endpoint-implementation-checklist)
4. [Database Design](#database-design)
5. [Authentication & Authorization](#authentication--authorization)
6. [Error Response Mapping](#error-response-mapping)
7. [Testing Requirements](#testing-requirements)

---

## Project Structure

Current Backend Structure (from `UavPms.sln`):

```
Backend/
├── UavPms.Core/                  # Domain layer
│   ├── Common/
│   │   └── BaseEntity.cs         # Base class for all entities
│   ├── Entities/
│   │   ├── AppUser.cs            # User entity (Primary)
│   │   ├── AppRole.cs            # Role entity
│   │   ├── UserRole.cs           # User-Role mapping
│   │   ├── Notification.cs       # For future use
│   │   └── ...
│   ├── Contracts/                # DTOs
│   │   └── (Create Login/User DTOs here)
│   ├── Interfaces/
│   │   ├── Repositories/         # Repository contracts
│   │   └── Services/             # Service contracts
│   └── Enums/
│       └── (User status enums)
│
├── UavPms.Infrastructure/        # Infrastructure/Persistence layer
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs  # EF Core DbContext
│   │   └── Configurations/         # Entity configurations
│   ├── Repositories/
│   │   ├── GenericRepository.cs    # Base CRUD operations
│   │   └── (Create UserRepository here)
│   ├── Migrations/               # Database migrations
│   └── DependencyInjection.cs    # Service registration
│
├── UavPms.Application/           # Application layer
│   ├── (Create Services here)
│   └── Features/                 # Feature-based organization
│
└── UavPms.WebApi/               # Presentation/API layer
    ├── Controllers/
    │   ├── AuthController.cs     # Authentication endpoints
    │   └── UsersController.cs    # User management endpoints
    ├── Middleware/               # Custom middleware
    └── Program.cs                # Application configuration
```

---

## Core Implementation Guidelines

### 1. Response Format Implementation

**Create a Response Wrapper:**

```csharp
// UavPms.Core/Contracts/ApiResponse.cs
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public bool Success { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Success = true
        };
    }

    public static ApiResponse<T> ErrorResponse(int statusCode, string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Message = message,
            Data = default!,
            Success = false
        };
    }
}

// Non-generic version for void responses
public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }

    public static ApiResponse SuccessResponse(string message = "Success", int statusCode = 200)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Message = message,
            Success = true
        };
    }

    public static ApiResponse ErrorResponse(int statusCode, string message)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Message = message,
            Success = false
        };
    }
}
```

### 2. User Entity & Database Schema

**User Entity:**

```csharp
// UavPms.Core/Entities/AppUser.cs
public class AppUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  // BCrypt or similar
    public string Role { get; set; } = "Viewer";  // Admin, Manager, Technician, Viewer
    public string Status { get; set; } = "Active";  // Active, Inactive, Locked
    public bool MustChangePassword { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }  // For soft delete

    // Navigation properties (if using role management)
    public ICollection<AppRole> Roles { get; set; } = new List<AppRole>();
}
```

**Database Schema:**

```sql
CREATE TABLE [dbo].[AppUsers] (
    [Id] NVARCHAR(36) NOT NULL PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [FullName] NVARCHAR(200) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL DEFAULT 'Viewer',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [MustChangePassword] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DeletedAt] DATETIME2 NULL
);

-- Indexes
CREATE UNIQUE INDEX [IX_AppUsers_Username] ON [dbo].[AppUsers]([Username]);
CREATE UNIQUE INDEX [IX_AppUsers_Email] ON [dbo].[AppUsers]([Email]);
CREATE INDEX [IX_AppUsers_Role] ON [dbo].[AppUsers]([Role]);
CREATE INDEX [IX_AppUsers_Status] ON [dbo].[AppUsers]([Status]);
```

### 3. DTOs (Data Transfer Objects)

```csharp
// UavPms.Core/Contracts/Auth/LoginRequest.cs
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

// UavPms.Core/Contracts/Auth/AuthTokens.cs
public class AuthTokens
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

// UavPms.Core/Contracts/Auth/LoginResponse.cs
public class LoginResponse
{
    public AppUserDto User { get; set; }
    public AuthTokens Tokens { get; set; }
}

// UavPms.Core/Contracts/User/AppUserDto.cs
public class AppUserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Status { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// UavPms.Core/Contracts/User/CreateUserRequest.cs
public class CreateUserRequest
{
    [Required]
    public string FullName { get; set; }
    
    [Required]
    public string Role { get; set; }  // Admin, Manager, Technician, Viewer
}

// UavPms.Core/Contracts/User/CreateUserResponse.cs
public class CreateUserResponse
{
    public AppUserDto User { get; set; }
    public string Username { get; set; }
    public string TemporaryPassword { get; set; }
}

// UavPms.Core/Contracts/User/UpdateUserRequest.cs
public class UpdateUserRequest
{
    public string Role { get; set; }
    public string Status { get; set; }
}

// UavPms.Core/Contracts/User/ChangePasswordRequest.cs
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}

// UavPms.Core/Contracts/User/ResetPasswordResponse.cs
public class ResetPasswordResponse
{
    public string Username { get; set; }
    public string TemporaryPassword { get; set; }
}
```

---

## Endpoint Implementation Checklist

### Authentication Controller

**File:** `UavPms.WebApi/Controllers/AuthController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/login
    /// Authenticate user with username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // TODO: Validate request
            // TODO: Find user by username
            // TODO: Verify password (BCrypt.VerifyHashedPassword)
            // TODO: Check user status (not Locked or Inactive)
            // TODO: Generate JWT tokens (AccessToken + RefreshToken)
            // TODO: Return response with status 200

            // Error Cases:
            // - 401: Invalid credentials or user not found
            // - 423: Account locked or inactive
            
            var response = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(
                response, 
                "Đăng nhập thành công"
            ));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(
                401, 
                "Sai tên đăng nhập hoặc mật khẩu"
            ));
        }
        catch (InvalidOperationException)
        {
            return StatusCode(423, ApiResponse<LoginResponse>.ErrorResponse(
                423, 
                "Tài khoản đã bị vô hiệu hóa hoặc khóa"
            ));
        }
    }

    /// <summary>
    /// POST /api/auth/logout
    /// Clear user session
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        // TODO: Invalidate refresh token (optional - store in blacklist)
        // TODO: Clear session if needed
        
        return Ok(ApiResponse.SuccessResponse("Đăng xuất thành công"));
    }

    /// <summary>
    /// POST /api/auth/refresh
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthTokens>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // TODO: Validate refresh token
            // TODO: Generate new access token
            // TODO: Optionally generate new refresh token
            
            var tokens = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<AuthTokens>.SuccessResponse(
                tokens, 
                "Token đã được làm mới"
            ));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<AuthTokens>.ErrorResponse(
                401, 
                "Refresh token không hợp lệ hoặc đã hết hạn"
            ));
        }
    }

    /// <summary>
    /// POST /api/auth/change-password
    /// Change authenticated user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AppUserDto>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // TODO: Get current user from token (User.FindFirst(ClaimTypes.NameIdentifier))
            // TODO: Verify current password
            // TODO: Hash and save new password
            // TODO: Set MustChangePassword = false
            
            var user = await _authService.ChangePasswordAsync(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                request.CurrentPassword,
                request.NewPassword
            );
            
            return Ok(ApiResponse<AppUserDto>.SuccessResponse(
                user, 
                "Đổi mật khẩu thành công"
            ));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<AppUserDto>.ErrorResponse(
                401, 
                "Chưa đăng nhập"
            ));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse<AppUserDto>.ErrorResponse(
                400, 
                "Không thể đổi mật khẩu"
            ));
        }
    }
}
```

### Users Controller

**File:** `UavPms.WebApi/Controllers/UsersController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// GET /api/users
    /// Get all users (requires Admin or Manager role)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppUserDto>>>> GetAll()
    {
        try
        {
            // TODO: Query all users (exclude soft-deleted)
            // TODO: Map to DTO
            
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<AppUserDto>>.SuccessResponse(
                users, 
                "Lấy danh sách thành công"
            ));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// POST /api/users
    /// Create new user (requires Admin or Manager role)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            // TODO: Validate fullName and role are provided
            // TODO: Generate unique username from fullName
            // TODO: Generate secure temporary password
            // TODO: Generate email from username
            // TODO: Create user with MustChangePassword = true
            // TODO: Return 201 Created
            
            var response = await _userService.CreateUserAsync(request);
            return Created(
                $"/api/users/{response.User.Id}", 
                ApiResponse<CreateUserResponse>.SuccessResponse(
                    response, 
                    "Tạo tài khoản thành công", 
                    201
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CreateUserResponse>.ErrorResponse(
                400, 
                ex.Message  // "Họ tên và vai trò là bắt buộc"
            ));
        }
    }

    /// <summary>
    /// PATCH /api/users/{id}
    /// Update user role or status (requires Admin or Manager role)
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<AppUserDto>>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            // TODO: Find user by id
            // TODO: Validate: cannot change only Admin's role/status
            // TODO: Update role and/or status
            // TODO: Update UpdatedAt timestamp
            
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(ApiResponse<AppUserDto>.SuccessResponse(
                user, 
                "Cập nhật thành công"
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AppUserDto>.ErrorResponse(
                404, 
                "Không tìm thấy người dùng"
            ));
        }
        catch (InvalidOperationException ex)
        {
            return Forbid();  // Cannot modify admin
        }
    }

    /// <summary>
    /// POST /api/users/{id}/reset-password
    /// Reset user password to temporary password (requires Admin or Manager role)
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ResetPasswordResponse>>> ResetPassword(string id)
    {
        try
        {
            // TODO: Find user by id
            // TODO: Generate new temporary password
            // TODO: Update PasswordHash
            // TODO: Set MustChangePassword = true
            
            var response = await _userService.ResetPasswordAsync(id);
            return Ok(ApiResponse<ResetPasswordResponse>.SuccessResponse(
                response, 
                "Đặt lại mật khẩu thành công"
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ResetPasswordResponse>.ErrorResponse(
                404, 
                "Không tìm thấy người dùng"
            ));
        }
        catch (InvalidOperationException)
        {
            return Forbid();  // Cannot reset admin password
        }
    }

    /// <summary>
    /// DELETE /api/users/{id}
    /// Delete user (requires Admin role only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(string id)
    {
        try
        {
            // TODO: Find user by id
            // TODO: Validate: cannot delete Admin accounts
            // TODO: Soft delete (set DeletedAt) or hard delete
            
            await _userService.DeleteUserAsync(id);
            return Ok(ApiResponse.SuccessResponse(
                "Xóa người dùng thành công"
            ));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.ErrorResponse(
                404, 
                "Không tìm thấy người dùng"
            ));
        }
        catch (InvalidOperationException)
        {
            return StatusCode(403, ApiResponse.ErrorResponse(
                403, 
                "Không thể xóa tài khoản quản trị viên"
            ));
        }
    }
}
```

---

## Database Design

### Initial Migration

```csharp
// UavPms.Infrastructure/Migrations/[timestamp]_CreateUsersTable.cs
public partial class CreateUsersTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppUsers",
            columns: table => new
            {
                Id = table.Column<string>(maxLength: 36, nullable: false),
                Username = table.Column<string>(maxLength: 100, nullable: false),
                FullName = table.Column<string>(maxLength: 200, nullable: false),
                Email = table.Column<string>(maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(nullable: false),
                Role = table.Column<string>(maxLength: 50, nullable: false, defaultValue: "Viewer"),
                Status = table.Column<string>(maxLength: 50, nullable: false, defaultValue: "Active"),
                MustChangePassword = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUsers", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_Username",
            table: "AppUsers",
            column: "Username",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_Email",
            table: "AppUsers",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_Role",
            table: "AppUsers",
            column: "Role");

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_Status",
            table: "AppUsers",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppUsers");
    }
}
```

### Seed Initial Data

```csharp
// UavPms.Infrastructure/Persistence/ApplicationDbContextModelSnapshot.cs or seed data method
private void SeedUsers(ModelBuilder modelBuilder)
{
    var users = new[]
    {
        new AppUser
        {
            Id = "1",
            Username = "admin",
            FullName = "Nguyễn Văn Admin",
            Email = "admin@evn.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@123"),
            Role = "Admin",
            Status = "Active",
            MustChangePassword = false,
            CreatedAt = DateTime.Parse("2026-01-01T00:00:00Z"),
            UpdatedAt = DateTime.Parse("2026-01-01T00:00:00Z")
        },
        new AppUser
        {
            Id = "2",
            Username = "manager",
            FullName = "Trần Thị Manager",
            Email = "manager@evn.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager@123"),
            Role = "Manager",
            Status = "Active",
            MustChangePassword = false,
            CreatedAt = DateTime.Parse("2026-01-02T00:00:00Z"),
            UpdatedAt = DateTime.Parse("2026-01-02T00:00:00Z")
        },
        // ... more users
    };

    modelBuilder.Entity<AppUser>().HasData(users);
}
```

---

## Authentication & Authorization

### JWT Token Generation

```csharp
// UavPms.Infrastructure/Services/TokenService.cs
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokens GenerateTokens(AppUser user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        return new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private string GenerateAccessToken(AppUser user)
    {
        // TODO: Create JWT with:
        // - sub (subject): user.Id
        // - username: user.Username
        // - role: user.Role
        // - exp: expires in 30 minutes
        // - iat: issued at
        // - jti: JWT ID (for refresh token invalidation)
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(AppUser user)
    {
        // Similar to AccessToken but longer expiration (7-30 days)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshSecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("type", "refresh")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Program.cs Configuration

```csharp
// UavPms.WebApi/Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication (JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",  // Vite dev server
            "https://yourdomain.com"   // Production
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

## Error Response Mapping

### Exception Handling Middleware

```csharp
// UavPms.WebApi/Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ArgumentException => new { statusCode = 400, message = exception.Message, success = false },
            KeyNotFoundException => new { statusCode = 404, message = "Không tìm thấy người dùng", success = false },
            UnauthorizedAccessException => new { statusCode = 401, message = "Chưa đăng nhập", success = false },
            InvalidOperationException => new { statusCode = 423, message = exception.Message, success = false },
            _ => new { statusCode = 500, message = "Lỗi máy chủ nội bộ", success = false }
        };

        context.Response.StatusCode = response.statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}

// Register in Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## Testing Requirements

### Unit Tests for UserService

```csharp
// UavPms.Application.Tests/Services/UserServiceTests.cs
[TestClass]
public class UserServiceTests
{
    private UserService _userService;
    private Mock<IUserRepository> _mockUserRepository;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _userService = new UserService(_mockUserRepository.Object);
    }

    [TestMethod]
    public async Task CreateUser_WithValidData_ShouldReturnUserWithTemporaryPassword()
    {
        // Arrange
        var request = new CreateUserRequest { FullName = "Test User", Role = "Technician" };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.TemporaryPassword);
        Assert.IsTrue(result.User.MustChangePassword);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<AppUser>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateUser_WithMissingFullName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateUserRequest { FullName = "", Role = "Technician" };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _userService.CreateUserAsync(request));
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ShouldReturnUserAndTokens()
    {
        // Arrange
        var user = new AppUser
        {
            Id = "1",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@123"),
            Status = "Active"
        };
        var request = new LoginRequest { Username = "admin", Password = "admin@123" };

        _mockUserRepository.Setup(x => x.FindByUsernameAsync("admin"))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.LoginAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Tokens.AccessToken);
        Assert.IsNotNull(result.Tokens.RefreshToken);
    }

    [TestMethod]
    public async Task Login_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = new AppUser
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@123")
        };
        var request = new LoginRequest { Username = "admin", Password = "wrongpassword" };

        _mockUserRepository.Setup(x => x.FindByUsernameAsync("admin"))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _userService.LoginAsync(request));
    }
}
```

### Integration Tests

```csharp
// UavPms.WebApi.Tests/Controllers/AuthControllerTests.cs
[TestClass]
public class AuthControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithTokens()
    {
        // Arrange
        var request = new { username = "admin", password = "admin@123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsAsync<ApiResponse<LoginResponse>>();
        Assert.IsTrue(content.Success);
        Assert.IsNotNull(content.Data.Tokens.AccessToken);
    }

    [TestMethod]
    public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetUsers_WithValidToken_ShouldReturnOkWithUsers()
    {
        // Arrange - first login to get token
        var loginRequest = new { username = "admin", password = "admin@123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsAsync<ApiResponse<LoginResponse>>();
        var token = loginContent.Data.Tokens.AccessToken;

        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsAsync<ApiResponse<IEnumerable<AppUserDto>>>();
        Assert.IsTrue(content.Success);
    }
}
```

---

## Implementation Checklist

- [ ] Create AppUser entity with all required fields
- [ ] Create DTOs for all request/response types
- [ ] Create ApiResponse wrapper class
- [ ] Implement user repository with CRUD operations
- [ ] Implement user service with business logic
- [ ] Implement authentication service with JWT token generation
- [ ] Create AuthController with all endpoints
- [ ] Create UsersController with all endpoints
- [ ] Configure JWT authentication in Program.cs
- [ ] Add CORS configuration for Frontend (http://localhost:5173)
- [ ] Create database migrations
- [ ] Seed initial test data
- [ ] Create exception handling middleware
- [ ] Add unit tests for services
- [ ] Add integration tests for controllers
- [ ] Test all error cases (400, 401, 403, 404, 423, 500)
- [ ] Verify response format matches specification
- [ ] Document API with Swagger/OpenAPI
- [ ] Perform load testing
- [ ] Security audit (password hashing, token validation, etc.)

---

## Key Considerations

### Password Security
- Use BCrypt or PBKDF2 for password hashing (not plain text or MD5)
- Never return password or passwordHash in API responses
- Temporary passwords should be strong and unique

### Token Security
- AccessToken: Short-lived (15-30 minutes)
- RefreshToken: Long-lived (7-30 days)
- Store refreshToken securely (consider database backup)
- Implement token blacklist for logout (optional but recommended)

### Data Validation
- Validate all inputs on backend (not just frontend)
- Check role permissions on every protected endpoint
- Validate enum values (role, status)

### Performance
- Add database indexes on frequently queried fields
- Consider pagination for GetAll users endpoint
- Cache user data if appropriate

### Monitoring
- Log all authentication attempts
- Log all user modifications
- Monitor for suspicious activities (multiple failed logins)

