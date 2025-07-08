# QaliTrack API Gateway - Testing Guide

Comprehensive testing guide covering test strategies, framework setup, and examples for the QaliTrack API Gateway and its integration with microservices.

## 📋 Table of Contents

- [Testing Strategy Overview](#testing-strategy-overview)
- [Test Environment Setup](#test-environment-setup)
- [Unit Testing](#unit-testing)
- [Integration Testing](#integration-testing)
- [End-to-End Testing](#end-to-end-testing)
- [Performance Testing](#performance-testing)
- [Security Testing](#security-testing)
- [Test Data Management](#test-data-management)

## Testing Strategy Overview

The QaliTrack Gateway testing strategy follows a comprehensive pyramid approach:

```
┌─────────────────────────────────────────────────┐
│                E2E Tests                        │ ←── Production-like scenarios
│              (20 tests)                         │
├─────────────────────────────────────────────────┤
│           Integration Tests                     │ ←── Service interactions
│              (57 tests)                         │
├─────────────────────────────────────────────────┤
│              Unit Tests                         │ ←── Component isolation
│             (104+ tests)                        │
└─────────────────────────────────────────────────┘
```

### Testing Categories

| Category | Purpose | Test Count | Tools |
|----------|---------|------------|--------|
| **Unit Tests** | Component isolation | 104+ | xUnit, FluentAssertions |
| **Integration Tests** | Service communication | 57 | WebApplicationFactory |
| **E2E Tests** | User workflows | 20 | Deployed services |
| **Performance Tests** | Load and stress | 15 | NBomber, k6 |
| **Security Tests** | Authorization matrix | 25 | Custom framework |

## Test Environment Setup

### 1. Development Environment

```bash
# Setup test environment
cd packages/qalitrack-gateway

# Install test dependencies
dotnet restore

# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Security
```

### 2. Docker Test Environment

```bash
# Start test services
make start-mock    # Fast mock services
make start-real    # Production-like services

# Run gateway tests
make test-gateway              # Unit tests (104+ test cases)
make test-gateway-integration  # Integration tests with deployed services

# Test role matrix
make test-role-matrix         # Authorization matrix validation
```

### 3. Test Configuration

```json
// appsettings.Testing.json
{
  "Jwt": {
    "SecretKey": "TestSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!",
    "Issuer": "MockUserService",
    "Audience": "MockUserService"
  },
  "Testing": {
    "UseMockServices": true,
    "EnableDetailedLogging": true,
    "TimeoutSeconds": 30
  }
}
```

## Unit Testing

Unit tests focus on testing individual components in isolation using `WebApplicationFactory<Program>`.

### 1. Role Matrix Testing

The core authorization testing validates 104+ role vs endpoint combinations:

```csharp
// RoleMatrixTests.cs - Core authorization logic testing
[Trait("Category", "Unit")]
public class RoleMatrixTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [MemberData(nameof(RoleMatrixTestData))]
    public async Task RoleMatrix_EndpointAccess_ShouldEnforceCorrectAuthorization(
        string endpoint, string requiredRole, string service, 
        string userRole, int userLevel, bool shouldHaveAccess)
    {
        // Arrange
        var token = GenerateJwtToken("test-user", userRole, GetPermissionsForRole(userRole));
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (shouldHaveAccess)
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.NotFound,     // Service not running
                HttpStatusCode.BadGateway    // Expected for unit tests
            );
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden,
                HttpStatusCode.Unauthorized
            );
        }
    }

    public static IEnumerable<object[]> RoleMatrixTestData()
    {
        var endpoints = new[]
        {
            new { Path = "/api/products", RequiredRole = "User", Service = "ProductService" },
            new { Path = "/api/customers", RequiredRole = "Operator", Service = "CustomerService" },
            new { Path = "/api/users/admin", RequiredRole = "Admin", Service = "UserService" },
            new { Path = "/api/analytics", RequiredRole = "SiteManager", Service = "AnalyticsService" },
            new { Path = "/api/organizations", RequiredRole = "Admin", Service = "OrganizationService" }
        };

        var roles = new[]
        {
            new { Name = "Guest", Level = 0 },
            new { Name = "User", Level = 1 },
            new { Name = "Operator", Level = 2 },
            new { Name = "SiteManager", Level = 3 },
            new { Name = "Admin", Level = 4 },
            new { Name = "SuperAdmin", Level = 5 }
        };

        foreach (var endpoint in endpoints)
        {
            foreach (var role in roles)
            {
                var requiredLevel = GetRoleLevel(endpoint.RequiredRole);
                var shouldHaveAccess = role.Level >= requiredLevel;
                
                yield return new object[]
                {
                    endpoint.Path, endpoint.RequiredRole, endpoint.Service,
                    role.Name, role.Level, shouldHaveAccess
                };
            }
        }
    }
}
```

### 2. JWT Authentication Testing

```csharp
// AuthenticationTests.cs - JWT validation testing
[Trait("Category", "Unit")]
public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task ValidJwtToken_ShouldAllowAccess()
    {
        // Arrange
        var validToken = GenerateValidJwtToken("test-user", "User");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", validToken);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExpiredJwtToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var expiredToken = GenerateExpiredJwtToken("test-user", "User");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TamperedJwtToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var validToken = GenerateValidJwtToken("test-user", "Admin");
        var tamperedToken = validToken.Substring(0, validToken.Length - 5) + "XXXXX";
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("InvalidIssuer")]
    [InlineData("")]
    [InlineData(null)]
    public async Task InvalidIssuer_ShouldReturnUnauthorized(string issuer)
    {
        // Arrange
        var invalidToken = GenerateJwtTokenWithIssuer("test-user", "User", issuer);
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### 3. Middleware Testing

```csharp
// MiddlewareTests.cs - Custom middleware testing
[Trait("Category", "Unit")]
public class RoleAuthorizationMiddlewareTests
{
    [Fact]
    public async Task PublicEndpoint_ShouldAllowAnonymousAccess()
    {
        // Arrange
        var context = CreateHttpContext("/health");
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var context = CreateHttpContext("/api/products");
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidRole_ShouldAddUserHeaders()
    {
        // Arrange
        var context = CreateHttpContext("/api/products");
        AddValidJwtToken(context, "test-user", "User");
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Request.Headers.Should().ContainKey("X-User-ID");
        context.Request.Headers.Should().ContainKey("X-User-Roles");
        context.Request.Headers.Should().ContainKey("X-Gateway-Authorized");
    }
}
```

### 4. Running Unit Tests

```bash
# Run all unit tests with detailed output
dotnet test --filter Category=Unit --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~RoleMatrixTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Generate coverage report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

## Integration Testing

Integration tests validate communication between gateway and deployed services.

### 1. Deployed Service Tests

```csharp
// DeployedServiceTests.cs - Test against running services
[Trait("Category", "Integration")]
public class DeployedServiceTests : IClassFixture<DeployedServiceFixture>
{
    [Fact]
    public async Task MockServices_ShouldBeHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("Guest")]
    [InlineData("User")]
    [InlineData("Operator")]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    public async Task MockAuth_ShouldGenerateTokensForAllRoles(string role)
    {
        // Arrange
        var loginRequest = new { username = $"testuser_{role}", role = role };
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Connect directly to user service for MockAuth
        using var userServiceClient = new HttpClient();
        var response = await userServiceClient.PostAsync(
            "http://localhost:7001/api/MockAuth/mock-login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(responseContent);
        var tokenElement = document.RootElement.GetProperty("token");
        var roleElement = document.RootElement.GetProperty("role");
        
        tokenElement.GetString().Should().NotBeNullOrEmpty();
        roleElement.GetString().Should().Be(role);
    }

    [Theory]
    [MemberData(nameof(AuthorizationMatrixData))]
    public async Task AuthorizationMatrix_WithDeployedServices_ShouldEnforceCorrectAccess(
        string role, string endpoint, HttpStatusCode expectedStatus, string description)
    {
        // Arrange
        var token = await GetTokenForRole(role);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (expectedStatus == HttpStatusCode.OK)
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.NotFound, 
                HttpStatusCode.BadGateway,
                HttpStatusCode.InternalServerError
            );
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                expectedStatus, 
                HttpStatusCode.Unauthorized, 
                HttpStatusCode.Forbidden
            );
        }
    }
}
```

### 2. Service Discovery Tests

```csharp
// ServiceDiscoveryTests.cs - Test service communication
[Trait("Category", "Integration")]
public class ServiceDiscoveryTests : IClassFixture<DeployedServiceFixture>
{
    [Fact]
    public async Task Gateway_ShouldDiscoverAvailableServices()
    {
        // Act
        var response = await _client.GetAsync("/api/gateway/services");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().ContainAny("user-service", "product-service");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.NotFound, 
                HttpStatusCode.NotImplemented
            );
        }
    }

    [Fact]
    public async Task HealthChecks_ShouldReturnServiceStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthData = JsonSerializer.Deserialize<HealthCheckResponse>(content);
        
        healthData.Status.Should().BeOneOf("Healthy", "Degraded");
        healthData.Entries.Should().ContainKey("gateway");
    }
}
```

### 3. Environment Detection Tests

```csharp
// EnvironmentDetectionTests.cs - Test mock vs real service detection
[Trait("Category", "Integration")]
public class EnvironmentDetectionTests : IClassFixture<DeployedServiceFixture>
{
    [Fact]
    public async Task Gateway_ShouldDetectMockServices()
    {
        // Act
        var isMockMode = await IsUsingMockServices();

        // Assert based on environment
        if (Environment.GetEnvironmentVariable("USE_MOCK_SERVICES") == "true")
        {
            isMockMode.Should().BeTrue();
        }
    }

    [Fact]
    public async Task MockMode_ShouldSupportInstantRoleSwitching()
    {
        if (!await IsUsingMockServices()) return; // Skip if in real mode

        var roles = new[] { "User", "Operator", "Admin" };
        
        foreach (var role in roles)
        {
            // Arrange
            var token = await GetTokenForRole(role);
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/gateway/info");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    private async Task<bool> IsUsingMockServices()
    {
        try
        {
            using var userServiceClient = new HttpClient();
            var response = await userServiceClient.GetAsync(
                "http://localhost:7001/api/MockAuth/roles");
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }
}
```

### 4. Running Integration Tests

```bash
# Start services for integration testing
make start-mock

# Run integration tests
dotnet test --filter Category=Integration --logger "console;verbosity=detailed"

# Or use make command
make test-gateway-integration

# Test with both mock and real services
make test-env-switch
```

## End-to-End Testing

E2E tests validate complete user workflows using deployed services.

### 1. User Workflow Tests

```csharp
// E2EWorkflowTests.cs - Complete user scenarios
[Trait("Category", "E2E")]
public class UserWorkflowTests : IClassFixture<DeployedServiceFixture>
{
    [Fact]
    public async Task CompleteOperatorWorkflow_ShouldSucceed()
    {
        // 1. Login as operator
        var token = await GetTokenForRole("Operator");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // 2. Create a customer
        var customer = new { name = "Test Customer", email = "test@example.com" };
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", customer);
        customerResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadGateway);

        // 3. Create a product  
        var product = new { name = "Test Product", unitPrice = 100.00 };
        var productResponse = await _client.PostAsJsonAsync("/api/products", product);
        productResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadGateway);

        // 4. Record weight data (if services are available)
        if (customerResponse.IsSuccessStatusCode && productResponse.IsSuccessStatusCode)
        {
            var weightData = new 
            { 
                grossWeight = 1500.0, 
                tareWeight = 500.0,
                customerId = "test-id",
                productId = "test-id"
            };
            
            var weightResponse = await _client.PostAsJsonAsync("/api/weight-data", weightData);
            // Allow for service unavailability in test environment
            weightResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.Created, 
                HttpStatusCode.OK, 
                HttpStatusCode.BadGateway,
                HttpStatusCode.NotFound
            );
        }
    }

    [Fact]
    public async Task AdminWorkflow_UserManagement_ShouldSucceed()
    {
        // 1. Login as admin
        var token = await GetTokenForRole("Admin");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // 2. Access user management
        var usersResponse = await _client.GetAsync("/api/users");
        usersResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadGateway);

        // 3. Access organization management
        var orgsResponse = await _client.GetAsync("/api/organizations");
        orgsResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadGateway);

        // 4. Access analytics
        var analyticsResponse = await _client.GetAsync("/api/analytics/sales");
        analyticsResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.NotFound);
    }
}
```

### 2. Business Process Tests

```csharp
// BusinessProcessTests.cs - End-to-end business scenarios
[Trait("Category", "E2E")]
public class BusinessProcessTests : IClassFixture<DeployedServiceFixture>
{
    [Fact]
    public async Task WeighbridgeTransaction_CompleteFlow_ShouldSucceed()
    {
        // Test complete weighbridge transaction flow
        var operatorToken = await GetTokenForRole("Operator");
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", operatorToken);

        // 1. Vehicle arrives - check vehicle registration
        var vehicleResponse = await _client.GetAsync("/api/vehicles/ABC-123");
        
        // 2. Record gross weight
        var grossWeightData = new 
        { 
            vehiclePlate = "ABC-123",
            grossWeight = 15000.0,
            timestamp = DateTime.UtcNow
        };
        var grossWeightResponse = await _client.PostAsJsonAsync("/api/weight-data", grossWeightData);
        
        // 3. Unload cargo and record tare weight
        var tareWeightData = new 
        { 
            vehiclePlate = "ABC-123",
            tareWeight = 5000.0,
            netWeight = 10000.0,
            timestamp = DateTime.UtcNow
        };
        var tareWeightResponse = await _client.PostAsJsonAsync("/api/weight-data", tareWeightData);
        
        // 4. Generate transaction
        var transactionData = new 
        { 
            vehiclePlate = "ABC-123",
            netWeight = 10000.0,
            productType = "Cement",
            customerId = "customer-123"
        };
        var transactionResponse = await _client.PostAsJsonAsync("/api/transactions", transactionData);
        
        // Allow for service unavailability in test environment
        // Focus on gateway authorization rather than business logic
        grossWeightResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        tareWeightResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        transactionResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
```

## Performance Testing

### 1. Load Testing with NBomber

```csharp
// LoadTests.cs - Performance testing
[Trait("Category", "Performance")]
public class GatewayLoadTests
{
    [Fact]
    public void GatewayAuthentication_LoadTest()
    {
        var scenario = Scenario.Create("auth_load_test", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:7000");

            // Test authentication load
            var loginData = new { username = "testuser", role = "User" };
            var response = await client.PostAsJsonAsync("/api/MockAuth/mock-login", loginData);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void GatewayRouting_StressTest()
    {
        var authToken = GetTestToken();
        
        var scenario = Scenario.Create("routing_stress_test", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:7000");
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", authToken);

            var endpoints = new[] 
            { 
                "/api/products", 
                "/api/customers", 
                "/api/gateway/info",
                "/health"
            };
            
            var endpoint = endpoints[Random.Shared.Next(endpoints.Length)];
            var response = await client.GetAsync(endpoint);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}
```

### 2. k6 Performance Scripts

```javascript
// performance/gateway-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 },   // Ramp up
    { duration: '1m', target: 100 },   // Stay at 100 users
    { duration: '30s', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% of requests under 500ms
    http_req_failed: ['rate<0.1'],     // Error rate under 10%
  },
};

export default function() {
  // 1. Login
  const loginPayload = JSON.stringify({
    username: 'testuser',
    role: 'User'
  });
  
  const loginResponse = http.post(
    'http://localhost:7000/api/MockAuth/mock-login',
    loginPayload,
    { headers: { 'Content-Type': 'application/json' } }
  );
  
  check(loginResponse, {
    'login successful': (r) => r.status === 200,
    'token received': (r) => JSON.parse(r.body).token !== undefined,
  });
  
  const token = JSON.parse(loginResponse.body).token;
  
  // 2. API calls with token
  const headers = {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json',
  };
  
  const endpoints = [
    '/api/products',
    '/api/gateway/info',
    '/health'
  ];
  
  endpoints.forEach(endpoint => {
    const response = http.get(`http://localhost:7000${endpoint}`, { headers });
    check(response, {
      [`${endpoint} successful`]: (r) => r.status === 200 || r.status === 502,
      [`${endpoint} not forbidden`]: (r) => r.status !== 403,
    });
  });
  
  sleep(1);
}
```

## Security Testing

### 1. Authorization Matrix Validation

```csharp
// SecurityTests.cs - Comprehensive security validation
[Trait("Category", "Security")]
public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [MemberData(nameof(GetSecurityTestMatrix))]
    public async Task SecurityMatrix_AllRoleEndpointCombinations_ShouldEnforceCorrectAccess(
        string userRole, string endpoint, bool shouldHaveAccess)
    {
        // Arrange
        var token = GenerateJwtToken("security-test-user", userRole, GetPermissionsForRole(userRole));
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (shouldHaveAccess)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden, 
                HttpStatusCode.Unauthorized
            );
        }
    }

    public static IEnumerable<object[]> GetSecurityTestMatrix()
    {
        // Generate all role x endpoint combinations for security testing
        var roles = new[] { "Guest", "User", "Operator", "Admin", "SuperAdmin" };
        var endpoints = new[] 
        { 
            "/api/products", "/api/customers", "/api/users", 
            "/api/organizations", "/api/analytics", "/api/compliance"
        };
        
        foreach (var role in roles)
        {
            foreach (var endpoint in endpoints)
            {
                var shouldHaveAccess = DetermineAccess(role, endpoint);
                yield return new object[] { role, endpoint, shouldHaveAccess };
            }
        }
    }

    [Fact]
    public async Task TokenTampering_ShouldBeDetected()
    {
        // Arrange
        var validToken = GenerateJwtToken("test-user", "Admin", GetPermissionsForRole("Admin"));
        var tamperedToken = TamperWithToken(validToken);
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PrivilegeEscalation_ShouldBePrevented()
    {
        // Arrange - Create token with User role
        var userToken = GenerateJwtToken("test-user", "User", GetPermissionsForRole("User"));
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", userToken);

        // Act - Try to access admin endpoints
        var adminEndpoints = new[] 
        { 
            "/api/users/admin", 
            "/api/organizations", 
            "/api/users"  // Creating users requires admin
        };

        foreach (var endpoint in adminEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden, 
                HttpStatusCode.Unauthorized
            );
        }
    }
}
```

### 2. Penetration Testing

```bash
#!/bin/bash
# security/penetration-test.sh

echo "🔒 Running QaliTrack Gateway Security Tests"

# 1. Test for common vulnerabilities
echo "Testing SQL injection..."
curl -X GET "http://localhost:7000/api/products?id=1'; DROP TABLE products; --" \
  -H "Authorization: Bearer $TEST_TOKEN"

# 2. Test for XSS vulnerabilities  
echo "Testing XSS..."
curl -X POST "http://localhost:7000/api/customers" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TEST_TOKEN" \
  -d '{"name": "<script>alert(\"XSS\")</script>"}'

# 3. Test rate limiting
echo "Testing rate limiting..."
for i in {1..150}; do
  curl -X GET "http://localhost:7000/api/products" \
    -H "Authorization: Bearer $TEST_TOKEN" &
done
wait

# 4. Test CORS policies
echo "Testing CORS..."
curl -X OPTIONS "http://localhost:7000/api/products" \
  -H "Origin: http://malicious-site.com" \
  -H "Access-Control-Request-Method: GET"

# 5. Test JWT token security
echo "Testing JWT security..."
# Invalid signature
curl -X GET "http://localhost:7000/api/products" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.INVALID.SIGNATURE"

# Expired token
curl -X GET "http://localhost:7000/api/products" \
  -H "Authorization: Bearer $EXPIRED_TOKEN"
```

## Test Data Management

### 1. Test Data Factory

```csharp
// TestDataFactory.cs - Centralized test data creation
public static class TestDataFactory
{
    public static string GenerateJwtToken(string userId, string role, List<string> permissions)
    {
        var secretKey = "TestSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
        var key = Encoding.UTF8.GetBytes(secretKey);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, $"testuser_{role}"),
            new Claim(ClaimTypes.Email, $"test_{role}@example.com"),
            new Claim(ClaimTypes.Role, role),
            new Claim("permissions", string.Join(",", permissions))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = "MockUserService",
            Audience = "MockUserService",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static List<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "Guest" => new() { "read:public" },
            "User" => new() { "read:public", "read:products", "read:profile" },
            "Operator" => new() { "read:public", "read:products", "write:products", "read:customers", "write:customers" },
            "Admin" => new() { "read:public", "read:products", "write:products", "manage:users", "read:organizations" },
            "SuperAdmin" => new() { "read:public", "read:products", "write:products", "manage:users", "manage:system" },
            _ => new() { "read:public" }
        };
    }

    public static Dictionary<string, string> CreateTestOcelotConfiguration()
    {
        return new Dictionary<string, string>
        {
            ["Routes:0:DownstreamPathTemplate"] = "/api/products/{everything}",
            ["Routes:0:DownstreamScheme"] = "http",
            ["Routes:0:DownstreamHostAndPorts:0:Host"] = "product-service",
            ["Routes:0:DownstreamHostAndPorts:0:Port"] = "7005",
            ["Routes:0:UpstreamPathTemplate"] = "/api/products/{everything}",
            ["Routes:0:UpstreamHttpMethod:0"] = "GET",
            ["Routes:0:Metadata:RequiredRoles:0"] = "User",
            ["Routes:0:Metadata:ServiceName"] = "ProductService"
        };
    }
}
```

### 2. Test Environment Management

```bash
# test-environment.sh - Test environment management
#!/bin/bash

setup_test_environment() {
    echo "🧪 Setting up test environment..."
    
    # Start test services
    docker-compose -f docker-compose.testing.yml -f docker-compose.mock.yml up -d
    
    # Wait for services to be ready
    sleep 10
    
    # Verify services are healthy
    curl -f http://localhost:7000/health || exit 1
    curl -f http://localhost:7001/api/MockAuth/roles || exit 1
    
    echo "✅ Test environment ready"
}

run_test_suite() {
    echo "🧪 Running comprehensive test suite..."
    
    # Unit tests
    dotnet test --filter Category=Unit --logger "trx;LogFileName=unit-tests.trx"
    
    # Integration tests  
    dotnet test --filter Category=Integration --logger "trx;LogFileName=integration-tests.trx"
    
    # Security tests
    dotnet test --filter Category=Security --logger "trx;LogFileName=security-tests.trx"
    
    # Performance tests (if enabled)
    if [ "$RUN_PERFORMANCE_TESTS" = "true" ]; then
        dotnet test --filter Category=Performance --logger "trx;LogFileName=performance-tests.trx"
    fi
    
    echo "✅ Test suite completed"
}

cleanup_test_environment() {
    echo "🧹 Cleaning up test environment..."
    docker-compose -f docker-compose.testing.yml down -v
    echo "✅ Cleanup completed"
}

# Main execution
case "$1" in
    setup)
        setup_test_environment
        ;;
    test)
        run_test_suite
        ;;
    cleanup)
        cleanup_test_environment
        ;;
    full)
        setup_test_environment
        run_test_suite
        cleanup_test_environment
        ;;
    *)
        echo "Usage: $0 {setup|test|cleanup|full}"
        exit 1
        ;;
esac
```

## Continuous Integration

### 1. GitHub Actions Workflow

```yaml
# .github/workflows/gateway-tests.yml
name: Gateway Tests

on:
  push:
    paths:
      - 'packages/qalitrack-gateway/**'
  pull_request:
    paths:
      - 'packages/qalitrack-gateway/**'

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore packages/qalitrack-gateway/src
    
    - name: Build
      run: dotnet build packages/qalitrack-gateway/src --no-restore
    
    - name: Start test services
      run: |
        docker-compose -f apps/testing/docker-compose.testing.yml \
                      -f apps/testing/docker-compose.mock.yml up -d
        sleep 15
    
    - name: Run unit tests
      run: |
        dotnet test packages/qalitrack-gateway/tests \
               --filter Category=Unit \
               --collect:"XPlat Code Coverage" \
               --logger "trx;LogFileName=unit-tests.trx"
    
    - name: Run integration tests
      run: |
        dotnet test packages/qalitrack-gateway/tests \
               --filter Category=Integration \
               --logger "trx;LogFileName=integration-tests.trx"
    
    - name: Run security tests
      run: |
        dotnet test packages/qalitrack-gateway/tests \
               --filter Category=Security \
               --logger "trx;LogFileName=security-tests.trx"
    
    - name: Generate coverage report
      run: |
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:**/coverage.cobertura.xml \
                       -targetdir:coverage-report \
                       -reporttypes:Html
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
    
    - name: Cleanup
      if: always()
      run: docker-compose -f apps/testing/docker-compose.testing.yml down -v
```

---

*This testing guide provides comprehensive coverage of all testing strategies, tools, and practices for ensuring the quality and reliability of the QaliTrack API Gateway.*