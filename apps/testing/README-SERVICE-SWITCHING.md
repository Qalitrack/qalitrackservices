# Service Switching Guide

This guide explains how to switch between mock and real services in the testing environment.

## Overview

The testing environment supports two modes:
- **Mock Mode**: Uses mock services for rapid development and testing
- **Real Mode**: Uses production-like services with full functionality

## Quick Start

### Using Mock Services (Recommended for Testing)

```bash
# Start with mock services
cd apps/testing
./scripts/start-mock.sh

# Or manually:
docker-compose -f docker-compose.testing.yml -f docker-compose.mock.yml up -d
```

### Using Real Services (For Integration Testing)

```bash
# Start with real services
cd apps/testing
./scripts/start-real.sh

# Or manually:
docker-compose -f docker-compose.testing.yml -f docker-compose.real.yml up -d
```

## Service Configurations

### Mock Mode Features
- **Mock User Service**: Instant JWT token generation without database
- **Role-based Testing**: Easy role switching (Guest, User, Operator, Admin, SuperAdmin)
- **No Database**: Stateless authentication for testing
- **Fast Startup**: Minimal dependencies

**Mock Authentication Example:**
```bash
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "role": "User"}'
```

### Real Mode Features
- **Full User Service**: Database-backed authentication with user management
- **Persistent Data**: User accounts, roles, and permissions stored in database
- **Production-like**: Uses same services as production environment
- **Full Features**: Complete user registration, login, profile management

**Real Authentication Example:**
```bash
curl -X POST http://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'
```

## Environment Variables

| Variable | Mock Mode | Real Mode | Description |
|----------|-----------|-----------|-------------|
| `USE_MOCK_SERVICES` | `true` | `false` | Service selection flag |
| `Jwt__Issuer` | `MockUserService` | `UserService` | JWT token issuer |
| `Jwt__Audience` | `MockUserService` | `UserService` | JWT token audience |
| `Jwt__SecretKey` | Shared secret key | Same key | JWT signing key |

## Docker Compose Architecture

```
docker-compose.testing.yml     # Base configuration (defaults to real services)
├── docker-compose.mock.yml    # Mock services overlay
└── docker-compose.real.yml    # Real services overlay
```

### Base Configuration
- Contains common services (gateway, product-service, customer-service)
- Defaults to real user service
- Shared networking and volume configuration

### Mock Overlay
- Overrides user-service with mock implementation
- Sets mock-specific environment variables
- Removes database volumes and dependencies

### Real Overlay
- Confirms real user service configuration
- Sets production-like environment variables
- Includes database volumes and health checks

## Gateway Configuration

The gateway automatically detects which mode it's running in:

```
🧪 Using MOCK services for authentication
   JWT Issuer: MockUserService
   JWT Audience: MockUserService
```

or

```
🔐 Using REAL services for authentication
   JWT Issuer: UserService
   JWT Audience: UserService
```

## Testing Workflows

### 1. Role-Based Authorization Testing (Mock Mode)
```bash
# Start mock environment
./scripts/start-mock.sh

# Test different roles
TOKEN=$(curl -s -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "role": "User"}' | jq -r '.token')

curl -H "Authorization: Bearer $TOKEN" http://localhost:7000/api/products
```

### 2. Integration Testing (Real Mode)
```bash
# Start real environment
./scripts/start-real.sh

# Test with actual user accounts
curl -X POST http://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "password", "email": "test@example.com"}'
```

### 3. Switching Between Modes
```bash
# Stop current environment
docker-compose -f docker-compose.testing.yml down

# Start in different mode
./scripts/start-mock.sh    # or start-real.sh
```

## Maintenance Benefits

### 1. **No Code Changes Required**
- Switch between mock and real services without modifying application code
- Environment variables handle all configuration differences

### 2. **Consistent Interface**
- Both mock and real services expose the same API endpoints
- Gateway configuration remains the same

### 3. **Easy Development**
- Use mock services for rapid development and testing
- Switch to real services for integration testing
- No JWT configuration changes needed

### 4. **Production Readiness**
- Real mode uses production-like configuration
- Easy transition from testing to production

## Troubleshooting

### JWT Token Issues
- Ensure `Jwt__Issuer` and `Jwt__Audience` match between gateway and user service
- Check that `Jwt__SecretKey` is identical across all services

### Service Discovery
- Mock and real services both register with the same service names
- Gateway routes work identically in both modes

### Database Issues (Real Mode)
- Check that database volumes are properly mounted
- Verify connection strings in environment variables

## Files Modified

- `docker-compose.testing.yml` - Base configuration
- `docker-compose.mock.yml` - Mock services overlay
- `docker-compose.real.yml` - Real services overlay
- `scripts/start-mock.sh` - Mock mode startup script
- `scripts/start-real.sh` - Real mode startup script
- `packages/qalitrack-gateway/src/Program.cs` - Environment detection