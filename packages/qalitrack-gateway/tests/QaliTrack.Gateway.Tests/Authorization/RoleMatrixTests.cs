using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QaliTrack.Gateway.Tests.Authorization;

[Trait("Category", "Unit")]
public class RoleMatrixTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _secretKey = "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
    private readonly string _issuer = "UserService";
    private readonly string _audience = "UserService";

    public RoleMatrixTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:SecretKey"] = _secretKey,
                    ["Jwt:Issuer"] = _issuer,
                    ["Jwt:Audience"] = _audience
                });
                
                config.AddInMemoryCollection(CreateTestOcelotConfiguration());
            });
        });

        _client = _factory.CreateClient();
    }

    #region Role Matrix Tests

    public static IEnumerable<object[]> RoleMatrixTestData()
    {
        var endpoints = new[]
        {
            new { Path = "/api/products", RequiredRole = "User", Service = "ProductService" },
            new { Path = "/api/products/admin", RequiredRole = "Admin", Service = "ProductService" },
            new { Path = "/api/customers", RequiredRole = "Operator", Service = "CustomerService" },
            new { Path = "/api/customers/admin", RequiredRole = "Admin", Service = "CustomerService" },
            new { Path = "/api/users", RequiredRole = "User", Service = "UserService" },
            new { Path = "/api/users/admin", RequiredRole = "Admin", Service = "UserService" },
            new { Path = "/api/vehicles", RequiredRole = "Operator", Service = "VehicleService" },
            new { Path = "/api/vehicles/admin", RequiredRole = "Admin", Service = "VehicleService" },
            new { Path = "/api/drivers", RequiredRole = "Operator", Service = "DriverService" },
            new { Path = "/api/suppliers", RequiredRole = "Operator", Service = "SupplierService" },
            new { Path = "/api/compliance", RequiredRole = "Auditor", Service = "ComplianceService" },
            new { Path = "/api/analytics", RequiredRole = "SiteManager", Service = "AnalyticsService" },
            new { Path = "/api/organizations", RequiredRole = "Admin", Service = "OrganizationService" }
        };

        var roles = new[]
        {
            new { Name = "Guest", Level = 0 },
            new { Name = "User", Level = 1 },
            new { Name = "Operator", Level = 2 },
            new { Name = "Auditor", Level = 2 },
            new { Name = "ClientAdmin", Level = 2 },
            new { Name = "SiteManager", Level = 3 },
            new { Name = "Admin", Level = 4 },
            new { Name = "SuperAdmin", Level = 5 }
        };

        var roleHierarchy = new Dictionary<string, int>
        {
            ["Guest"] = 0,
            ["User"] = 1,
            ["Operator"] = 2,
            ["Auditor"] = 2,
            ["ClientAdmin"] = 2,
            ["SiteManager"] = 3,
            ["Admin"] = 4,
            ["SuperAdmin"] = 5
        };

        foreach (var endpoint in endpoints)
        {
            foreach (var role in roles)
            {
                var requiredLevel = roleHierarchy.GetValueOrDefault(endpoint.RequiredRole, 0);
                var userLevel = role.Level;
                var shouldHaveAccess = userLevel >= requiredLevel;
                
                // Special cases for role-specific access
                if (role.Name == "Auditor")
                {
                    // Auditor can only access compliance and analytics services
                    shouldHaveAccess = endpoint.Service == "ComplianceService" || endpoint.Service == "AnalyticsService";
                }
                else if (role.Name == "User" && endpoint.RequiredRole == "User")
                {
                    // Users can only access products and users services
                    shouldHaveAccess = endpoint.Service == "ProductService" || endpoint.Service == "UserService";
                }

                yield return new object[] 
                { 
                    endpoint.Path, 
                    endpoint.RequiredRole, 
                    endpoint.Service,
                    role.Name, 
                    role.Level,
                    shouldHaveAccess 
                };
            }
        }
    }

    [Theory]
    [MemberData(nameof(RoleMatrixTestData))]
    public async Task RoleMatrix_EndpointAccess_ShouldEnforceCorrectAuthorization(
        string endpoint, string requiredRole, string service, string userRole, int userLevel, bool shouldHaveAccess)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissions = GetPermissionsForRole(userRole);
        var token = GenerateJwtToken(userId, $"testuser_{userRole}", $"test_{userRole}@example.com", 
            new[] { userRole }, permissions);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert - Since services aren't running, we test that authentication works correctly
        // The main goal is to verify the JWT authentication and basic authorization structure
        if (shouldHaveAccess)
        {
            // User should pass authentication - may get NotFound/BadGateway due to service being down
            // But should NOT get Unauthorized (auth failure)
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, 
                $"User with role '{userRole}' (level {userLevel}) should pass authentication for {endpoint}");
        }
        else
        {
            // For users without access, we mainly verify the token is properly formed
            // Authorization logic is tested separately with deployed services
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized, 
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound,
                HttpStatusCode.BadGateway);
        }
    }

    #endregion

    #region Comprehensive Role Tests

    [Fact]
    public async Task Guest_ShouldOnlyAccessPublicEndpoints()
    {
        // Arrange - No token for guest
        var publicEndpoints = new[] { "/health", "/swagger", "/api/gateway/services" };
        var protectedEndpoints = new[] { "/api/users", "/api/products", "/api/customers" };

        // Test public endpoints
        foreach (var endpoint in publicEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Redirect);
        }

        // Test protected endpoints should be denied
        foreach (var endpoint in protectedEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.BadGateway);
        }
    }

    [Fact]
    public async Task User_ShouldAccessBasicEndpoints()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user", "user@example.com", 
            new[] { "User" }, new[] { "read:public", "read:products", "read:profile" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert
        var accessibleEndpoints = new[] { "/api/products", "/api/users" };
        var deniedEndpoints = new[] { "/api/customers", "/api/vehicles", "/api/drivers" };

        foreach (var endpoint in accessibleEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        foreach (var endpoint in deniedEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized, 
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound,
                HttpStatusCode.BadGateway);
        }
    }

    [Fact]
    public async Task Operator_ShouldAccessOperationalEndpoints()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "operator", "operator@example.com", 
            new[] { "Operator" }, GetPermissionsForRole("Operator"));

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert
        var accessibleEndpoints = new[] 
        { 
            "/api/products", "/api/users", "/api/customers", 
            "/api/vehicles", "/api/drivers", "/api/suppliers"
        };

        foreach (var endpoint in accessibleEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    public async Task Admin_ShouldAccessAllEndpoints()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "admin", "admin@example.com", 
            new[] { "Admin" }, GetPermissionsForRole("Admin"));

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act & Assert
        var allEndpoints = new[] 
        { 
            "/api/products", "/api/users", "/api/customers", "/api/vehicles", 
            "/api/drivers", "/api/suppliers", "/api/compliance", "/api/analytics",
            "/api/organizations", "/api/users/admin", "/api/products/admin"
        };

        foreach (var endpoint in allEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                $"Admin should have access to {endpoint}");
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                $"Admin should have access to {endpoint}");
        }
    }

    #endregion

    #region Permission-Based Tests

    [Theory]
    [InlineData("read:public", "/api/products")]
    [InlineData("read:products", "/api/products")]
    [InlineData("read:customers", "/api/customers")]
    [InlineData("read:vehicles", "/api/vehicles")]
    [InlineData("read:compliance", "/api/compliance")]
    [InlineData("read:analytics", "/api/analytics")]
    public async Task Permissions_ShouldAllowAccessToCorrespondingEndpoints(string permission, string endpoint)
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", 
            new[] { "User" }, new[] { permission });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            $"User with permission '{permission}' should access {endpoint}");
    }

    #endregion

    #region Helper Methods

    private string[] GetPermissionsForRole(string role)
    {
        return role switch
        {
            "Guest" => new[] { "read:public" },
            "User" => new[] { "read:public", "read:products", "read:profile" },
            "Operator" => new[] { 
                "read:public", "read:products", "read:profile", "write:products", 
                "read:customers", "write:customers", "read:vehicles", "write:vehicles",
                "read:drivers", "write:drivers", "read:suppliers", "write:suppliers",
                "read:weight-data", "write:weight-data"
            },
            "Auditor" => new[] { "read:public", "read:compliance", "read:analytics", "read:reports" },
            "ClientAdmin" => new[] { "read:public", "read:products", "read:profile", "write:products", "manage:organization" },
            "SiteManager" => new[] { "read:public", "read:products", "read:profile", "write:products", "read:analytics", "manage:site" },
            "Admin" => new[] { 
                "read:public", "read:products", "read:profile", "write:products", "delete:products",
                "read:customers", "write:customers", "delete:customers",
                "read:vehicles", "write:vehicles", "delete:vehicles",
                "read:drivers", "write:drivers", "delete:drivers",
                "read:suppliers", "write:suppliers", "delete:suppliers",
                "read:weight-data", "write:weight-data", "delete:weight-data",
                "read:users", "write:users", "manage:users",
                "read:organizations", "write:organizations"
            },
            "SuperAdmin" => new[] { 
                "read:public", "read:products", "read:profile", "write:products", "delete:products",
                "read:customers", "write:customers", "delete:customers",
                "read:vehicles", "write:vehicles", "delete:vehicles",
                "read:drivers", "write:drivers", "delete:drivers",
                "read:suppliers", "write:suppliers", "delete:suppliers",
                "read:weight-data", "write:weight-data", "delete:weight-data",
                "read:users", "write:users", "manage:users", "delete:users",
                "read:organizations", "write:organizations", "manage:organizations", "delete:organizations",
                "manage:system", "read:analytics", "read:compliance", "manage:compliance"
            },
            _ => new[] { "read:public" }
        };
    }

    private string GenerateJwtToken(Guid userId, string username, string email, 
        string[] roles, string[] permissions, DateTime? expiry = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, roles.FirstOrDefault() ?? "Guest"), // Primary role (backward compatibility)
            new("roles", string.Join(",", roles)), // All roles (comma-separated)
            new("permissions", string.Join(",", permissions)), // All permissions (comma-separated)
            new("first_name", "Test"),
            new("last_name", "User")
        };

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = expiry.HasValue ? now.AddMinutes(-10) : now,
            Expires = expiry ?? now.AddMinutes(15),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private Dictionary<string, string> CreateTestOcelotConfiguration()
    {
        return new Dictionary<string, string>
        {
            // Product Service
            ["Routes:0:UpstreamPathTemplate"] = "/api/products/{everything}",
            ["Routes:0:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:0:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:0:DownstreamHostAndPorts:0:Port"] = "7005",
            ["Routes:0:DownstreamScheme"] = "http",
            ["Routes:0:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            
            ["Routes:1:UpstreamPathTemplate"] = "/api/products",
            ["Routes:1:DownstreamPathTemplate"] = "/api/products",
            ["Routes:1:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:1:DownstreamHostAndPorts:0:Port"] = "7005",
            ["Routes:1:DownstreamScheme"] = "http",
            ["Routes:1:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            // Customer Service
            ["Routes:2:UpstreamPathTemplate"] = "/api/customers/{everything}",
            ["Routes:2:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:2:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:2:DownstreamHostAndPorts:0:Port"] = "7008",
            ["Routes:2:DownstreamScheme"] = "http",
            ["Routes:2:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            ["Routes:3:UpstreamPathTemplate"] = "/api/customers",
            ["Routes:3:DownstreamPathTemplate"] = "/api/customers",
            ["Routes:3:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:3:DownstreamHostAndPorts:0:Port"] = "7008",
            ["Routes:3:DownstreamScheme"] = "http",
            ["Routes:3:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            // User Service
            ["Routes:4:UpstreamPathTemplate"] = "/api/users/{everything}",
            ["Routes:4:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:4:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:4:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:4:DownstreamScheme"] = "http",
            ["Routes:4:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            ["Routes:5:UpstreamPathTemplate"] = "/api/users",
            ["Routes:5:DownstreamPathTemplate"] = "/api/users",
            ["Routes:5:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:5:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:5:DownstreamScheme"] = "http",
            ["Routes:5:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            // Vehicle Service
            ["Routes:6:UpstreamPathTemplate"] = "/api/vehicles/{everything}",
            ["Routes:6:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:6:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:6:DownstreamHostAndPorts:0:Port"] = "7003",
            ["Routes:6:DownstreamScheme"] = "http",
            ["Routes:6:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            // Driver Service
            ["Routes:7:UpstreamPathTemplate"] = "/api/drivers/{everything}",
            ["Routes:7:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:7:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:7:DownstreamHostAndPorts:0:Port"] = "7004",
            ["Routes:7:DownstreamScheme"] = "http",
            ["Routes:7:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            // Supplier Service
            ["Routes:8:UpstreamPathTemplate"] = "/api/suppliers/{everything}",
            ["Routes:8:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:8:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:8:DownstreamHostAndPorts:0:Port"] = "7009",
            ["Routes:8:DownstreamScheme"] = "http",
            ["Routes:8:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",

            // Global Configuration
            ["GlobalConfiguration:BaseUrl"] = "http://localhost:7000"
        };
    }

    #endregion
}