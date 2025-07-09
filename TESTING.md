# QaliTrack Testing Guide

*Comprehensive testing documentation for the QaliTrack microservices ecosystem*

## 📖 Overview

QaliTrack implements a comprehensive testing strategy across all microservices, ensuring reliability, security, and business value validation. This document provides a complete guide to our testing infrastructure, methodologies, and tools.

## 🏗️ Testing Architecture

```
QaliTrack Testing Ecosystem
├── Gateway Tests (79+ tests)          # Security, routing, authentication
├── User Service Tests (85+ tests)     # Authentication, authorization, user management
├── Service-Specific Tests (80+ each)  # Business logic, API endpoints, integration
├── Authorization Tests                 # Role-based access control validation
└── Integration Tests                  # Cross-service communication
```

## 📁 Directory Structure

### Test Locations

```
qalitrackservices/
├── packages/qalitrack-gateway/tests/
│   └── QaliTrack.Gateway.Tests/       # Gateway security and routing tests
├── packages/microservices/masterdata/user-service/tests/
│   └── UserService.Tests/             # User management and authentication tests
├── packages/microservices/datamanager/*/tests/
│   └── *Service.Tests/                # Data management service tests
├── tests/                             # Root-level testing utilities
│   ├── auth_config_generator_test.py  # Authorization configuration tests
│   └── shared/                        # Shared test utilities and helpers
└── .claude/commands/                  # Test generation and execution commands
    ├── create-service-test.md         # Generate comprehensive test suites
    ├── test-all-services.md          # Execute full ecosystem testing
    └── generate-docs.md               # Generate test documentation
```

### Test Categories

Each service includes the following test categories:

#### 1. **Unit Tests**
```
tests/{ServiceName}.Tests/Unit/
├── Controllers/                       # API controller testing
├── Services/                         # Business logic testing
├── Repositories/                     # Data access testing
└── Validators/                       # Input validation testing
```

#### 2. **Integration Tests**
```
tests/{ServiceName}.Tests/Integration/
├── Api/                              # End-to-end API testing
├── Database/                         # Database integration testing
└── Authentication/                   # Auth flow testing
```

#### 3. **Security Tests**
```
tests/{ServiceName}.Tests/Security/
├── Authorization/                    # Role-based access control
├── Authentication/                   # JWT and login security
└── Validation/                       # Input sanitization and validation
```

#### 4. **Test Helpers**
```
tests/{ServiceName}.Tests/Helpers/
├── TestDataFactory.cs               # Realistic test data generation
├── TestWebApplicationFactory.cs     # Integration test setup
├── MockFactories.cs                 # Mock object creation
└── DatabaseTestBase.cs              # Database test utilities
```

## 🧪 Testing Standards

### Framework and Tools

- **Testing Framework**: xUnit (.NET 8)
- **Assertion Library**: FluentAssertions
- **Mocking Framework**: Moq
- **Integration Testing**: WebApplicationFactory
- **Test Data Generation**: AutoFixture
- **Coverage Tool**: Coverlet
- **Performance Testing**: NBomber (where applicable)

### Naming Conventions

```csharp
// Test class naming
public class VehicleControllerTests
public class VehicleServiceTests  
public class VehicleRepositoryTests

// Test method naming
[Fact]
public void CreateVehicle_WithValidData_ShouldReturnCreatedVehicle()

[Fact] 
public void CreateVehicle_WithDuplicateLicensePlate_ShouldThrowConflictException()
```

### Test Categories and Attributes

```csharp
[Fact]
[Trait("Category", "Unit")]
public void UnitTest_Description() { }

[Fact]
[Trait("Category", "Integration")]
public void IntegrationTest_Description() { }

[Fact]
[Trait("Category", "Security")]
public void SecurityTest_Description() { }

[Fact]
[Trait("Category", "Performance")]
public void PerformanceTest_Description() { }
```

## 🚀 Test Execution

### Make Commands

QaliTrack provides business-friendly make commands for testing:

#### Quick Tests
```bash
make quick-test         # Smoke test
make test-health        # Health checks  
make test-api          # API endpoints
```

#### Core Service Tests
```bash
make test-gateway      # Gateway security and routing (79+ tests)
make test-users        # User service authentication (85+ tests)
```

#### Deployment Management
```bash
make start-testing     # Start testing environment
make status           # Show service status
make logs            # View logs
make stop-all        # Stop all services
```

#### Comprehensive Testing
```bash
make test-all         # Run all tests
make full-test        # Comprehensive test with report
make test-interactive # Interactive test menu
```

#### Gateway Security Tests
```bash
make test-gateway
```
**Output:**
```
🔐 QaliTrack Gateway Security Tests
===================================

Testing the following security use cases:
  ✓ JWT Token Generation and Validation
  ✓ Public Endpoints (Health, Swagger) Allow Anonymous Access
  ✓ Protected Endpoints Require Authentication
  ✓ Role-Based Access Control (Operator, Manager, Admin)
  ✓ Token Expiration Handling
  ✓ Invalid Token Rejection
  ✓ Tampered Token Detection
  ✓ Wrong Issuer/Audience Rejection
  ✓ User Context Header Forwarding
  ✓ Role Hierarchy Enforcement

Running 79 comprehensive security tests...

✅ Gateway Security Tests Complete!

🛡️  What was tested:
   • Authentication: Ensures only valid JWT tokens are accepted
   • Authorization: Verifies users can only access permitted services
   • Role Enforcement: Confirms role hierarchy (User < Operator < Manager < Admin)
   • Security Headers: Validates user context is forwarded to services
   • Token Security: Prevents tampering, replay, and expiration attacks
```

#### User Service Tests
```bash
make test-users
```
**Output:**
```
👤 QaliTrack User Service Tests
===============================

Testing the following user management use cases:
  ✓ User Registration and Validation
  ✓ Password Security and Hashing (BCrypt)
  ✓ User Authentication and Login
  ✓ JWT Token Generation and Validation
  ✓ Role Assignment and Management
  ✓ Permission-Based Authorization
  ✓ User Profile Management
  ✓ Password Reset and Recovery
  ✓ Session Management and Token Refresh
  ✓ API Security and Input Validation

Running 85+ comprehensive user management tests...

✅ User Service Tests Complete!

👥 What was tested:
   • Registration: Validates user creation with proper constraints
   • Authentication: Ensures secure login with password verification
   • Authorization: Confirms role-based access control works correctly
   • Token Management: Validates JWT generation, expiration, and refresh
   • Security: Tests password hashing, input validation, and rate limiting
   • API Endpoints: Verifies all user management endpoints function properly
```

### Direct dotnet Commands

```bash
# Run all tests for a service
dotnet test packages/microservices/masterdata/user-service/tests/UserService.Tests/

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Security

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 🔧 Test Generation

### Claude Commands for Test Generation

QaliTrack provides AI-powered test generation through Claude commands:

#### Generate Service Tests
```bash
/create-service-test vehicle-service
```
**What it generates:**
- Complete test suite (85+ tests)
- Unit, integration, and security tests
- Realistic test data factories
- Business-friendly make targets
- Test documentation

#### Test All Services
```bash
/test-all-services
```
**What it does:**
- Discovers all services
- Executes comprehensive test suites
- Provides business value reporting
- Generates detailed metrics

#### Generate Documentation
```bash
/generate-docs vehicle-service
```
**What it includes:**
- Testing strategy documentation
- Test coverage reports
- Business test descriptions

### Example Generated Make Target

When `/create-service-test vehicle-service` is run, it generates:

```makefile
test-vehicles:
	@echo "🚗 QaliTrack Vehicle Service Tests"
	@echo "=================================="
	@echo ""
	@echo "Testing the following vehicle management use cases:"
	@echo "  ✓ Vehicle Registration and Validation"
	@echo "  ✓ Fleet Management Operations"
	@echo "  ✓ Vehicle Assignment and Tracking"
	@echo "  ✓ License Plate Validation and Uniqueness"
	@echo "  ✓ Vehicle Type and Capacity Management"
	@echo "  ✓ Ownership and Assignment Validation"
	@echo "  ✓ Vehicle Status Management (Active/Inactive)"
	@echo "  ✓ API Security and Authentication"
	@echo "  ✓ Role-Based Access Control (Operator+ required)"
	@echo "  ✓ Database Integration and Data Validation"
	@echo ""
	@echo "Running 85+ comprehensive vehicle management tests..."
	@echo ""
	@cd packages/microservices/masterdata/vehicle-service/tests/VehicleService.Tests && \
	dotnet test --logger "console;verbosity=normal" --configuration Release
	@echo ""
	@echo "✅ Vehicle Service Tests Complete!"
	@echo ""
	@echo "🚗 What was tested:"
	@echo "   • Registration: Validates vehicle creation with proper constraints"
	@echo "   • Fleet Management: Ensures comprehensive vehicle lifecycle operations"
	@echo "   • Security: Tests authentication and role-based access control"
	@echo "   • Data Integrity: Verifies database operations and business rules"
	@echo "   • API Endpoints: Confirms all vehicle management endpoints work correctly"
```

## 🔐 Authorization Testing

### Authorization Config Generator Tests

Location: `testing-unified/scripts/auth_config_generator_test.py`

**What it tests:**
- YAML authorization rule loading
- Ocelot route generation
- Environment-specific overrides
- Gateway configuration validation
- Role hierarchy enforcement

**Test Categories:**
- Configuration loading and validation
- Route generation from YAML rules
- Environment override application
- Gateway configuration backup and restore
- Integration testing with real configurations

**Make Commands:**
```bash
# Test authorization config generator
make auth-config-test

# Generate and validate auth configs
make auth-config-validate
```

## 📊 Test Coverage Standards

### Coverage Requirements

| Test Type | Minimum Coverage | Target Coverage |
|-----------|-----------------|-----------------|
| Unit Tests | 80% | 95% |
| Integration Tests | 70% | 85% |
| Security Tests | 90% | 100% |
| API Endpoints | 100% | 100% |

### Coverage Reporting

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML coverage report
reportgenerator -reports:"**/*.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Quality Gates

Tests must pass the following quality gates:
- All tests pass (100% pass rate)
- Minimum coverage thresholds met
- No security vulnerabilities detected
- Performance benchmarks achieved
- Code quality standards met

## 🛡️ Security Testing

### Security Test Categories

#### Authentication Testing
- JWT token validation
- Token expiration handling
- Invalid token rejection
- Multi-factor authentication flows

#### Authorization Testing  
- Role-based access control
- Permission inheritance
- Resource-level authorization
- Cross-tenant data isolation

#### Input Validation Testing
- SQL injection prevention
- XSS protection
- Input sanitization
- Boundary value testing

#### API Security Testing
- HTTPS enforcement
- CORS configuration
- Rate limiting
- Request/response validation

## 🔄 CI/CD Integration

### GitHub Actions

Tests are automatically executed in GitHub Actions:

```yaml
- name: Run Tests
  run: |
    make test-all
    
- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    files: '**/coverage.cobertura.xml'
```

### Test Automation

- **Pre-commit hooks**: Run unit tests before commits
- **Pull request validation**: Full test suite execution
- **Deployment gates**: Tests must pass before deployment
- **Scheduled testing**: Nightly comprehensive test runs

## 📈 Performance Testing

### Performance Test Categories

#### Load Testing
- Normal operational load simulation
- Peak traffic handling
- Resource utilization monitoring

#### Stress Testing  
- System breaking point identification
- Recovery behavior validation
- Error rate monitoring under stress

#### Endurance Testing
- Long-running operation validation
- Memory leak detection
- Performance degradation monitoring

### Performance Benchmarks

| Service Type | Response Time | Throughput | Error Rate |
|-------------|---------------|------------|------------|
| Gateway | < 50ms | 1000 req/s | < 0.1% |
| Auth Service | < 100ms | 500 req/s | < 0.1% |
| Data Service | < 200ms | 200 req/s | < 0.5% |
| Analytics | < 500ms | 50 req/s | < 1% |

## 🔍 Troubleshooting

### Common Test Issues

#### Test Failures
```bash
# Run specific failing test
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

#### Database Issues
```bash
# Reset test database
dotnet ef database drop --force --context TestDbContext
dotnet ef database update --context TestDbContext
```

#### Authentication Issues
- Verify JWT configuration matches across services
- Check role mappings and hierarchy
- Validate token generation and validation

#### Coverage Issues
- Ensure test projects reference all source projects
- Verify test discovery is working correctly
- Check for excluded files in coverage configuration

### Debug Commands

```bash
# Debug specific service tests
make test-[service] --debug

# Run tests with debugging
dotnet test --logger console --verbosity detailed

# Validate test discovery
dotnet test --list-tests
```

## 📚 Testing Best Practices

### Test Design Principles

1. **Arrange-Act-Assert (AAA)**: Structure tests clearly
2. **Single Responsibility**: One test, one concern
3. **Descriptive Names**: Test names explain the scenario
4. **Independent Tests**: No test dependencies
5. **Realistic Data**: Use business-meaningful test data

### Test Data Management

```csharp
// Use TestDataFactory for consistent test data
var vehicle = TestDataFactory.CreateVehicle(
    licensePlate: "TEST-123",
    vehicleType: VehicleType.Truck
);

// Use AutoFixture for random but valid data
var customer = _fixture.Create<Customer>();
```

### Mock Usage

```csharp
// Mock external dependencies
var mockUserService = new Mock<IUserService>();
mockUserService.Setup(x => x.GetCurrentUserAsync())
    .ReturnsAsync(TestDataFactory.CreateUser());
```

### Performance Considerations

- Use `TestHost` for integration tests
- Implement proper test cleanup
- Minimize database operations in unit tests
- Use in-memory databases for fast testing

## 🎯 Test Strategy by Service Type

### Master Data Services
**Focus Areas:**
- CRUD operations validation
- Data integrity constraints
- Business rule enforcement
- API endpoint coverage

### Operational Services  
**Focus Areas:**
- Real-time processing validation
- Performance under load
- Data accuracy and consistency
- Integration with hardware/external systems

### Analytics Services
**Focus Areas:**
- Data aggregation accuracy
- Report generation validation
- Performance with large datasets
- Caching effectiveness

### Infrastructure Services
**Focus Areas:**
- High availability scenarios
- Security enforcement
- Configuration management
- Monitoring and alerting

## 📞 Support and Documentation

### Getting Help

1. **Review this testing guide**
2. **Check service-specific test documentation**
3. **Use Claude commands for test generation**
4. **Consult team lead for complex scenarios**

### Additional Resources

- [User Service Testing Guide](docs/packages/user/README.md)
- [Gateway Testing Documentation](docs/packages/gateway/README.md)
- [Authorization Testing Guide](docs/packages/gateway/technical-architecture.md)
- [API Testing Standards](docs/packages/gateway/api-reference.md)

---

*This testing guide ensures comprehensive validation of the QaliTrack ecosystem, maintaining high quality and reliability across all microservices while providing clear guidance for developers and stakeholders.*
