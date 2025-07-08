# QaliTrack Makefile Testing Guide

This guide explains how to use the updated Makefile to test both mock and real services.

## Quick Start

```bash
# See all available options
make help

# Interactive test selector
make test-interactive

# Quick environment status check
make test-env-status
```

## Mock Service Testing

### Start Mock Environment
```bash
make start-mock
```

### Run Mock Tests
```bash
# Complete mock service test suite
make test-mock

# Individual mock tests
make test-mock-health      # Health checks
make test-mock-auth        # Authentication testing
make test-role-matrix      # Role-based authorization
```

### Mock Service Features
- **Fast startup** - No database initialization
- **Role switching** - Instant role changes for testing
- **Stateless** - No persistent data, perfect for testing
- **JWT Generation** - All roles (Guest, User, Operator, Admin, SuperAdmin)

## Real Service Testing

### Start Real Environment
```bash
make start-real
```

### Run Real Tests
```bash
# Complete real service test suite
make test-real

# Individual real tests
make test-real-health      # Health checks
make test-real-auth        # User registration and login
```

### Real Service Features
- **Database-backed** - Persistent user accounts
- **Full functionality** - Complete user management
- **Production-like** - Uses same services as production
- **Registration flow** - User creation, login, JWT generation

## Service Switching

### Switch Between Modes
```bash
# Switch to mock services
make switch-to-mock

# Switch to real services
make switch-to-real

# Test switching capability
make test-auth-switch
```

### Environment Management
```bash
# Stop testing environment
make stop-testing

# Check current status
make test-env-status
```

## Role-Based Authorization Testing

### Test Authorization Matrix
```bash
make test-role-matrix
```

This tests all roles against all endpoints:
- **Guest** → Products: DENIED, Customers: DENIED
- **User** → Products: PASS, Customers: DENIED
- **Operator** → Products: PASS, Customers: PASS
- **Admin** → Products: PASS, Customers: PASS
- **SuperAdmin** → Products: PASS, Customers: PASS

### Test Role Generation
```bash
make test-mock-auth
```

Tests JWT token generation for all roles:
- Guest, User, Operator, Admin, SuperAdmin

## Environment Status Detection

### Check Current Mode
```bash
make test-env-status
```

Automatically detects:
- **🧪 MOCK SERVICES** - Mock user service active
- **🔐 REAL SERVICES** - Real user service active
- **❓ UNKNOWN** - Services not responding

## Interactive Testing

### Use Interactive Selector
```bash
make test-interactive
```

Provides menu with options:
1. Core Tests (1-6)
2. Mock/Real Service Tests (7-11)
3. Environment Management (12-16)
4. Legacy Options (17-21)

## Integration with Existing Tests

### Legacy Test Commands Still Work
```bash
make test-health          # Health checks
make test-api             # API endpoint tests
make test-gateway         # Gateway security tests
make test-users           # User service tests
make test-product         # Product service tests
make test-customer        # Customer service tests
```

### New Mock/Real Capabilities
All existing tests now work with both mock and real services, depending on which environment is running.

## Common Workflows

### 1. Development Testing (Fast)
```bash
make start-mock           # Start mock environment
make test-role-matrix     # Test authorization
make test-mock-auth       # Test authentication
```

### 2. Integration Testing (Comprehensive)
```bash
make start-real           # Start real environment
make test-real-auth       # Test user registration/login
make test-role-matrix     # Test authorization with real users
```

### 3. Switching Testing
```bash
make test-auth-switch     # Test both modes automatically
```

### 4. Quick Status Check
```bash
make test-env-status      # See what's running
```

## Error Handling

### Service Not Responding
```bash
# Check environment status
make test-env-status

# Restart services
make stop-testing
make start-mock    # or start-real
```

### Authentication Issues
```bash
# Check current mode
make test-env-status

# Test authentication
make test-mock-auth      # for mock mode
make test-real-auth      # for real mode
```

### Role Authorization Problems
```bash
# Test role matrix
make test-role-matrix

# Expected results are shown in test output
```

## Environment Variables

The Makefile automatically handles all environment variables:
- `USE_MOCK_SERVICES=true/false`
- `JWT_ISSUER=MockUserService/UserService`
- `JWT_AUDIENCE=MockUserService/UserService`
- `JWT_SECRET_KEY` (shared between all services)

## Files Created/Modified

### New Files
- `apps/testing/docker-compose.mock.yml` - Mock services overlay
- `apps/testing/docker-compose.real.yml` - Real services overlay
- `apps/testing/scripts/start-mock.sh` - Mock startup script
- `apps/testing/scripts/start-real.sh` - Real startup script

### Modified Files
- `Makefile` - Added mock/real testing targets
- `apps/testing/docker-compose.testing.yml` - Base configuration
- `packages/qalitrack-gateway/src/Program.cs` - Environment detection

## Benefits

1. **No Code Changes** - Switch between mock and real without modifying code
2. **Consistent Interface** - Same API endpoints in both modes
3. **Easy Testing** - Simple commands for all scenarios
4. **Production Ready** - Real mode uses production-like configuration
5. **Fast Development** - Mock mode for rapid iteration
6. **Comprehensive Coverage** - Test all roles and permissions

## Troubleshooting

### Common Issues
1. **Services not starting** - Check Docker daemon and port conflicts
2. **Authentication failing** - Verify JWT configuration matches between services
3. **Role authorization not working** - Check gateway logs and route configuration
4. **Environment detection failing** - Ensure services are fully started before testing

### Debug Commands
```bash
# Check service logs
make logs

# Check container status
docker ps --filter "name=testing"

# Check service health
make test-env-status

# Test specific authentication
make test-mock-auth    # or test-real-auth
```