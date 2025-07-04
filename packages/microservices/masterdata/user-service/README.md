# User Service

The User Service is a comprehensive authentication and user management service for the QaliTrack system. It provides JWT-based authentication, role-based access control, and user management capabilities.

## Features

- **Authentication & Authorization**
  - JWT token-based authentication
  - Refresh token mechanism
  - Role-based access control (RBAC)
  - Permission-based authorization
  - Multi-factor authentication support

- **User Management**
  - User registration and profile management
  - Email confirmation and password reset
  - Account lockout and security features
  - User sessions management

- **Organization Integration**
  - Multi-tenant user management
  - Organization-scoped roles and permissions
  - User invitation system
  - Organization membership management

- **Security Features**
  - Password hashing with BCrypt
  - Account lockout after failed login attempts
  - Secure password requirements
  - Session management and revocation

## Technology Stack

- **.NET 8** - Framework
- **Entity Framework Core** - ORM
- **SQLite** - Development database
- **JWT** - Authentication tokens
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Logging
- **Swagger/OpenAPI** - API documentation

## Project Structure

```
src/
├── UserService.Api/          # Web API controllers and configuration
├── UserService.Core/         # Domain entities, DTOs, interfaces, and business logic
└── UserService.Infrastructure/ # Data access, repositories, and external services

tests/
└── UserService.Tests/        # Unit and integration tests
```

## Core Entities

- **User** - Core user entity with authentication details
- **Role** - System and organization roles
- **Permission** - Granular permissions for resources and actions
- **UserRole** - User-role assignments with organization scope
- **UserSession** - Authentication session management
- **UserProfile** - Extended user profile information
- **OrganizationUser** - Organization membership details
- **UserInvitation** - User invitation system

## Key API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/logout` - User logout
- `POST /api/auth/change-password` - Change password
- `POST /api/auth/reset-password` - Password reset request
- `POST /api/auth/confirm-email` - Email confirmation

### User Management
- `GET /api/users/profile` - Get user profile
- `PUT /api/users/profile` - Update user profile
- `GET /api/users/roles` - Get user roles
- `GET /api/users/sessions` - Get user sessions
- `DELETE /api/users/sessions/{id}` - Revoke session

### Utilities
- `GET /api/users/check-username/{username}` - Check username availability
- `GET /api/users/check-email/{email}` - Check email availability
- `GET /health` - Health check endpoint

## Configuration

### JWT Settings
```json
{
  "Jwt": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "UserService",
    "Audience": "UserService",
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "7"
  }
}
```

### Security Settings
```json
{
  "Security": {
    "MaxFailedAttempts": "5",
    "LockoutMinutes": "30"
  }
}
```

## Running the Service

1. **Development Environment:**
   ```bash
   dotnet run --project src/UserService.Api
   ```

2. **Access Swagger UI:**
   Navigate to `https://localhost:7001` for API documentation

3. **Health Check:**
   Navigate to `https://localhost:7001/health`

## Database

The service uses SQLite for development and can be configured for PostgreSQL in production. The database is automatically created and seeded with default roles and permissions on startup.

## Default Roles

- **System Administrator** - Full system access
- **User** - Basic user access (default)
- **Organization Administrator** - Organization management
- **Organization User** - Standard organization access (default)

## Integration

This service integrates with:
- **Organization Service** - For organization management
- **All Data Services** - For authentication context
- **Audit Systems** - For user action tracking

## Security Considerations

- JWT tokens have configurable expiration times
- Refresh tokens provide secure token renewal
- Password requirements enforce strong passwords
- Account lockout prevents brute force attacks
- Session management allows centralized logout
- All sensitive operations are logged

## Development

To extend the service:

1. Add new entities to `UserService.Core/Entities/`
2. Create corresponding DTOs in `UserService.Core/DTOs/`
3. Implement repositories in `UserService.Infrastructure/Repositories/`
4. Add business logic to `UserService.Core/Services/`
5. Create API endpoints in `UserService.Api/Controllers/`
6. Update AutoMapper profiles for new mappings

## Testing

Run tests with:
```bash
dotnet test
```

Use the provided `UserService.Api.http` file for manual API testing.