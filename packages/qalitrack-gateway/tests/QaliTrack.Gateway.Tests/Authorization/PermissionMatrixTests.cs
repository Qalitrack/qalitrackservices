using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QaliTrack.Gateway.Tests.Authorization;

[Trait("Category", "Unit")]
public class PermissionMatrixTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _secretKey = "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
    private readonly string _issuer = "UserService";
    private readonly string _audience = "UserService";

    public PermissionMatrixTests(WebApplicationFactory<Program> factory)
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

    #region Permission Matrix Test Data

    public static IEnumerable<object[]> PermissionMatrixTestData()
    {
        var permissionEndpoints = new[]
        {
            // Product Service
            new { Path = "/api/products", RequiredRole = "User", RequiredPermission = "read:products", Service = "ProductService" },
            new { Path = "/api/products/123", RequiredRole = "User", RequiredPermission = "read:products", Service = "ProductService" },
            new { Path = "/api/products/admin/123", RequiredRole = "Admin", RequiredPermission = "write:products", Service = "ProductService" },
            
            // User Service
            new { Path = "/api/users/123", RequiredRole = "User", RequiredPermission = "read:profile", Service = "UserService" },
            new { Path = "/api/users/admin/123", RequiredRole = "Admin", RequiredPermission = "manage:users", Service = "UserService" },
            
            // Customer Service
            new { Path = "/api/customers", RequiredRole = "Operator", RequiredPermission = "read:customers", Service = "CustomerService" },
            new { Path = "/api/customers/123", RequiredRole = "Operator", RequiredPermission = "read:customers", Service = "CustomerService" },
        };

        var testRoles = new[]
        {
            new { Name = "Guest", Permissions = new[] { "read:public" } },
            new { Name = "User", Permissions = new[] { "read:public", "read:products", "read:profile" } },
            new { Name = "Operator", Permissions = new[] { 
                "read:public", "read:products", "read:profile", "write:products", 
                "read:customers", "write:customers", "read:vehicles", "write:vehicles",
                "read:drivers", "write:drivers", "read:suppliers", "write:suppliers",
                "read:weight-data", "write:weight-data"
            }},
            new { Name = "Admin", Permissions = new[] { 
                "read:public", "read:products", "read:profile", "write:products", "delete:products",
                "read:customers", "write:customers", "delete:customers",
                "read:vehicles", "write:vehicles", "delete:vehicles",
                "read:drivers", "write:drivers", "delete:drivers",
                "read:suppliers", "write:suppliers", "delete:suppliers",
                "read:weight-data", "write:weight-data", "delete:weight-data",
                "read:users", "write:users", "manage:users",
                "read:organizations", "write:organizations"
            }},
            new { Name = "SuperAdmin", Permissions = new[] { 
                "read:public", "read:products", "read:profile", "write:products", "delete:products",
                "read:customers", "write:customers", "delete:customers",
                "read:vehicles", "write:vehicles", "delete:vehicles",
                "read:drivers", "write:drivers", "delete:drivers",
                "read:suppliers", "write:suppliers", "delete:suppliers",
                "read:weight-data", "write:weight-data", "delete:weight-data",
                "read:users", "write:users", "manage:users", "delete:users",
                "read:organizations", "write:organizations", "manage:organizations", "delete:organizations",
                "manage:system", "read:analytics", "read:compliance", "manage:compliance"
            }}
        };

        var roleHierarchy = new Dictionary<string, int>
        {
            ["Guest"] = 0,
            ["User"] = 1,
            ["Operator"] = 2,
            ["Admin"] = 4,
            ["SuperAdmin"] = 5
        };

        foreach (var endpoint in permissionEndpoints)
        {
            foreach (var role in testRoles)
            {
                // Check if role meets minimum requirement
                var requiredLevel = roleHierarchy.GetValueOrDefault(endpoint.RequiredRole, 0);
                var userLevel = roleHierarchy.GetValueOrDefault(role.Name, 0);
                var roleAccess = userLevel >= requiredLevel;
                
                // Check if user has required permission
                var permissionAccess = role.Permissions.Contains(endpoint.RequiredPermission);
                
                // Both role AND permission must be satisfied for hybrid authorization
                var shouldHaveAccess = roleAccess && permissionAccess;

                yield return new object[] 
                { 
                    endpoint.Path, 
                    endpoint.RequiredRole,
                    endpoint.RequiredPermission,
                    endpoint.Service,
                    role.Name, 
                    role.Permissions,
                    shouldHaveAccess 
                };
            }
        }
    }

    [Theory]
    [MemberData(nameof(PermissionMatrixTestData))]
    public async Task PermissionMatrix_EndpointAccess_ShouldEnforceHybridAuthorization(
        string endpoint, string requiredRole, string requiredPermission, string service, 
        string userRole, string[] userPermissions, bool shouldHaveAccess)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtTokenWithPermissions(userId, $"testuser_{userRole}", $"test_{userRole}@example.com", 
            userRole, userPermissions);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (shouldHaveAccess)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, 
                $"User with role '{userRole}' and permissions [{string.Join(", ", userPermissions)}] should pass authentication for {endpoint}");
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                $"User with role '{userRole}' and permission '{requiredPermission}' should have access to {endpoint}");
        }
        else
        {
            // Should be denied due to insufficient role or missing permission
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized, 
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound,
                HttpStatusCode.BadGateway);
        }
    }

    #endregion

    #region Specific Permission Tests

    [Theory]
    [InlineData("read:products", "User", "/api/products", true)]
    [InlineData("read:public", "Guest", "/api/products", false)] // Has role but lacks permission
    [InlineData("read:customers", "Operator", "/api/customers", true)]
    [InlineData("read:products", "User", "/api/customers", false)] // Has wrong permission
    [InlineData("manage:users", "Admin", "/api/users/admin/123", true)]
    [InlineData("read:profile", "User", "/api/users/admin/123", false)] // Insufficient permission
    [InlineData("write:products", "Admin", "/api/products/admin/123", true)]
    [InlineData("read:products", "User", "/api/products/admin/123", false)] // Wrong permission level
    public async Task SpecificPermissions_ShouldEnforceCorrectAccess(
        string userPermission, string userRole, string endpoint, bool shouldHaveAccess)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissions = new[] { "read:public", userPermission }; // Always include read:public
        var token = GenerateJwtTokenWithPermissions(userId, $"testuser", $"test@example.com", 
            userRole, permissions);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (shouldHaveAccess)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                $"User with permission '{userPermission}' should have access to {endpoint}");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound,
                HttpStatusCode.BadGateway)
                .And.Subject.Should().NotBe(HttpStatusCode.OK,
                $"User with permission '{userPermission}' should be denied access to {endpoint}");
        }
    }

    #endregion

    #region Permission Inheritance Tests

    [Theory]
    [InlineData("Admin", "/api/products", new[] { "read:products", "write:products", "delete:products" })]
    [InlineData("Operator", "/api/customers", new[] { "read:customers", "write:customers" })]
    [InlineData("User", "/api/products", new[] { "read:products" })]
    public async Task RolePermissionInheritance_ShouldGrantCorrectPermissions(
        string role, string endpoint, string[] expectedPermissions)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var allPermissions = GetPermissionsForRole(role);
        var token = GenerateJwtTokenWithPermissions(userId, $"testuser", $"test@example.com", 
            role, allPermissions);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert - User should have access if they have any of the expected permissions
        var hasRequiredPermission = expectedPermissions.Any(perm => allPermissions.Contains(perm));
        if (hasRequiredPermission)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                $"Role '{role}' should have access to {endpoint} with permissions: {string.Join(", ", expectedPermissions)}");
        }
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task EmptyPermissions_ShouldDenyAccess()
    {
        // Arrange - User with role but no permissions
        var userId = Guid.NewGuid();
        var token = GenerateJwtTokenWithPermissions(userId, "testuser", "test@example.com", 
            "User", new string[0]);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "User with no permissions should be denied access");
    }

    [Fact]
    public async Task InvalidPermission_ShouldDenyAccess()
    {
        // Arrange - User with invalid permission
        var userId = Guid.NewGuid();
        var token = GenerateJwtTokenWithPermissions(userId, "testuser", "test@example.com", 
            "User", new[] { "invalid:permission" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "User with invalid permission should be denied access");
    }

    [Fact]
    public async Task MultiplePermissions_ShouldAllowAccessWithAnyValidPermission()
    {
        // Arrange - User with multiple permissions, including required one
        var userId = Guid.NewGuid();
        var permissions = new[] { "read:public", "write:customers", "read:products", "delete:vehicles" };
        var token = GenerateJwtTokenWithPermissions(userId, "testuser", "test@example.com", 
            "User", permissions);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            "User with 'read:products' among multiple permissions should have access");
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task PermissionEscalation_ShouldNotAllowUnauthorizedAccess()
    {
        // Arrange - User tries to access admin endpoint with user-level permissions
        var userId = Guid.NewGuid();
        var permissions = new[] { "read:public", "read:products", "read:profile" };
        var token = GenerateJwtTokenWithPermissions(userId, "testuser", "test@example.com", 
            "User", permissions);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/admin/123");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized)
            .And.Subject.Should().NotBe(HttpStatusCode.OK,
            "User should not be able to escalate to admin permissions");
    }

    [Fact]
    public async Task CrossServicePermission_ShouldNotGrantAccess()
    {
        // Arrange - User with product permissions trying to access customer service
        var userId = Guid.NewGuid();
        var permissions = new[] { "read:public", "read:products", "write:products" };
        var token = GenerateJwtTokenWithPermissions(userId, "testuser", "test@example.com", 
            "Operator", permissions); // Has role but wrong permissions

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "User with product permissions should not access customer service");
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

    private string GenerateJwtTokenWithPermissions(Guid userId, string username, string email, 
        string role, string[] permissions, DateTime? expiry = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role), // Primary role (backward compatibility)
            new("roles", role), // Single role for now
            new("permissions", string.Join(",", permissions)), // Comma-separated permissions
            new("first_name", "Test"),
            new("last_name", "User")
        };

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
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
            ["Routes:0:Metadata:RequiredRoles:0"] = "User",
            ["Routes:0:Metadata:RequiredPermissions:0"] = "read:products",
            ["Routes:0:Metadata:ServiceName"] = "ProductService",
            
            ["Routes:1:UpstreamPathTemplate"] = "/api/products",
            ["Routes:1:DownstreamPathTemplate"] = "/api/products",
            ["Routes:1:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:1:DownstreamHostAndPorts:0:Port"] = "7005",
            ["Routes:1:DownstreamScheme"] = "http",
            ["Routes:1:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:1:Metadata:RequiredRoles:0"] = "User",
            ["Routes:1:Metadata:RequiredPermissions:0"] = "read:products",
            ["Routes:1:Metadata:ServiceName"] = "ProductService",

            // Customer Service
            ["Routes:2:UpstreamPathTemplate"] = "/api/customers/{everything}",
            ["Routes:2:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:2:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:2:DownstreamHostAndPorts:0:Port"] = "7008",
            ["Routes:2:DownstreamScheme"] = "http",
            ["Routes:2:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:2:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:2:Metadata:RequiredPermissions:0"] = "read:customers",
            ["Routes:2:Metadata:ServiceName"] = "CustomerService",

            ["Routes:3:UpstreamPathTemplate"] = "/api/customers",
            ["Routes:3:DownstreamPathTemplate"] = "/api/customers",
            ["Routes:3:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:3:DownstreamHostAndPorts:0:Port"] = "7008",
            ["Routes:3:DownstreamScheme"] = "http",
            ["Routes:3:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:3:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:3:Metadata:RequiredPermissions:0"] = "read:customers",
            ["Routes:3:Metadata:ServiceName"] = "CustomerService",

            // User Service
            ["Routes:4:UpstreamPathTemplate"] = "/api/users/{everything}",
            ["Routes:4:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:4:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:4:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:4:DownstreamScheme"] = "http",
            ["Routes:4:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:4:Metadata:RequiredRoles:0"] = "User",
            ["Routes:4:Metadata:RequiredPermissions:0"] = "read:profile",
            ["Routes:4:Metadata:ServiceName"] = "UserService",

            ["Routes:5:UpstreamPathTemplate"] = "/api/users",
            ["Routes:5:DownstreamPathTemplate"] = "/api/users",
            ["Routes:5:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:5:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:5:DownstreamScheme"] = "http",
            ["Routes:5:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:5:Metadata:RequiredRoles:0"] = "User",
            ["Routes:5:Metadata:RequiredPermissions:0"] = "read:profile",
            ["Routes:5:Metadata:ServiceName"] = "UserService",

            // Vehicle Service
            ["Routes:6:UpstreamPathTemplate"] = "/api/vehicles/{everything}",
            ["Routes:6:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:6:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:6:DownstreamHostAndPorts:0:Port"] = "7003",
            ["Routes:6:DownstreamScheme"] = "http",
            ["Routes:6:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:6:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:6:Metadata:RequiredPermissions:0"] = "read:vehicles",
            ["Routes:6:Metadata:ServiceName"] = "VehicleService",

            // Driver Service
            ["Routes:7:UpstreamPathTemplate"] = "/api/drivers/{everything}",
            ["Routes:7:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:7:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:7:DownstreamHostAndPorts:0:Port"] = "7004",
            ["Routes:7:DownstreamScheme"] = "http",
            ["Routes:7:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:7:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:7:Metadata:ServiceName"] = "DriverService",

            // Supplier Service
            ["Routes:8:UpstreamPathTemplate"] = "/api/suppliers/{everything}",
            ["Routes:8:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:8:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:8:DownstreamHostAndPorts:0:Port"] = "7009",
            ["Routes:8:DownstreamScheme"] = "http",
            ["Routes:8:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:8:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:8:Metadata:ServiceName"] = "SupplierService",

            ["Routes:9:UpstreamPathTemplate"] = "/api/suppliers",
            ["Routes:9:DownstreamPathTemplate"] = "/api/suppliers",
            ["Routes:9:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:9:DownstreamHostAndPorts:0:Port"] = "7009",
            ["Routes:9:DownstreamScheme"] = "http",
            ["Routes:9:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:9:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:9:Metadata:ServiceName"] = "SupplierService",

            // Vehicles Service
            ["Routes:10:UpstreamPathTemplate"] = "/api/vehicles",
            ["Routes:10:DownstreamPathTemplate"] = "/api/vehicles",
            ["Routes:10:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:10:DownstreamHostAndPorts:0:Port"] = "7003",
            ["Routes:10:DownstreamScheme"] = "http",
            ["Routes:10:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:10:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:10:Metadata:RequiredPermissions:0"] = "read:vehicles",
            ["Routes:10:Metadata:ServiceName"] = "VehicleService",

            // Drivers Service
            ["Routes:11:UpstreamPathTemplate"] = "/api/drivers",
            ["Routes:11:DownstreamPathTemplate"] = "/api/drivers",
            ["Routes:11:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:11:DownstreamHostAndPorts:0:Port"] = "7004",
            ["Routes:11:DownstreamScheme"] = "http",
            ["Routes:11:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:11:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:11:Metadata:ServiceName"] = "DriverService",

            // Compliance Service
            ["Routes:12:UpstreamPathTemplate"] = "/api/compliance",
            ["Routes:12:DownstreamPathTemplate"] = "/api/compliance",
            ["Routes:12:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:12:DownstreamHostAndPorts:0:Port"] = "7013",
            ["Routes:12:DownstreamScheme"] = "http",
            ["Routes:12:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:12:Metadata:RequiredRoles:0"] = "Auditor",
            ["Routes:12:Metadata:RequiredPermissions:0"] = "read:compliance",
            ["Routes:12:Metadata:ServiceName"] = "ComplianceService",

            // Analytics Service
            ["Routes:13:UpstreamPathTemplate"] = "/api/analytics",
            ["Routes:13:DownstreamPathTemplate"] = "/api/analytics",
            ["Routes:13:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:13:DownstreamHostAndPorts:0:Port"] = "7016",
            ["Routes:13:DownstreamScheme"] = "http",
            ["Routes:13:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:13:Metadata:RequiredRoles:0"] = "SiteManager",
            ["Routes:13:Metadata:RequiredPermissions:0"] = "read:analytics",
            ["Routes:13:Metadata:ServiceName"] = "AnalyticsService",

            // Organizations Service
            ["Routes:14:UpstreamPathTemplate"] = "/api/organizations",
            ["Routes:14:DownstreamPathTemplate"] = "/api/organizations",
            ["Routes:14:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:14:DownstreamHostAndPorts:0:Port"] = "7002",
            ["Routes:14:DownstreamScheme"] = "http",
            ["Routes:14:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:14:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:14:Metadata:ServiceName"] = "OrganizationService",

            // Organizations Service with {everything} pattern
            ["Routes:22:UpstreamPathTemplate"] = "/api/organizations/{everything}",
            ["Routes:22:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:22:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:22:DownstreamHostAndPorts:0:Port"] = "7002",
            ["Routes:22:DownstreamScheme"] = "http",
            ["Routes:22:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:22:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:22:Metadata:ServiceName"] = "OrganizationService",

            // Analytics Service with {everything} pattern
            ["Routes:23:UpstreamPathTemplate"] = "/api/analytics/{everything}",
            ["Routes:23:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:23:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:23:DownstreamHostAndPorts:0:Port"] = "7016",
            ["Routes:23:DownstreamScheme"] = "http",
            ["Routes:23:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:23:Metadata:RequiredRoles:0"] = "SiteManager",
            ["Routes:23:Metadata:RequiredPermissions:0"] = "read:analytics",
            ["Routes:23:Metadata:ServiceName"] = "AnalyticsService",

            // Archive Service (for SiteManager tests)
            ["Routes:24:UpstreamPathTemplate"] = "/api/archive",
            ["Routes:24:DownstreamPathTemplate"] = "/api/archive",
            ["Routes:24:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:24:DownstreamHostAndPorts:0:Port"] = "7018",
            ["Routes:24:DownstreamScheme"] = "http",
            ["Routes:24:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:24:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:24:Metadata:ServiceName"] = "ArchiveService",

            // Archive Service with {everything} pattern
            ["Routes:25:UpstreamPathTemplate"] = "/api/archive/{everything}",
            ["Routes:25:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:25:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:25:DownstreamHostAndPorts:0:Port"] = "7018",
            ["Routes:25:DownstreamScheme"] = "http",
            ["Routes:25:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:25:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:25:Metadata:ServiceName"] = "ArchiveService",

            // Admin endpoint routes
            ["Routes:15:UpstreamPathTemplate"] = "/api/products/admin",
            ["Routes:15:DownstreamPathTemplate"] = "/api/products/admin",
            ["Routes:15:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:15:DownstreamHostAndPorts:0:Port"] = "7005",
            ["Routes:15:DownstreamScheme"] = "http",
            ["Routes:15:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:15:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:15:Metadata:ServiceName"] = "ProductService",

            ["Routes:16:UpstreamPathTemplate"] = "/api/customers/admin",
            ["Routes:16:DownstreamPathTemplate"] = "/api/customers/admin",
            ["Routes:16:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:16:DownstreamHostAndPorts:0:Port"] = "7008",
            ["Routes:16:DownstreamScheme"] = "http",
            ["Routes:16:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:16:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:16:Metadata:ServiceName"] = "CustomerService",

            ["Routes:17:UpstreamPathTemplate"] = "/api/users/admin",
            ["Routes:17:DownstreamPathTemplate"] = "/api/users/admin",
            ["Routes:17:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:17:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:17:DownstreamScheme"] = "http",
            ["Routes:17:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:17:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:17:Metadata:RequiredPermissions:0"] = "manage:users",
            ["Routes:17:Metadata:ServiceName"] = "UserService",

            ["Routes:18:UpstreamPathTemplate"] = "/api/vehicles/admin",
            ["Routes:18:DownstreamPathTemplate"] = "/api/vehicles/admin",
            ["Routes:18:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:18:DownstreamHostAndPorts:0:Port"] = "7003",
            ["Routes:18:DownstreamScheme"] = "http",
            ["Routes:18:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:18:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:18:Metadata:ServiceName"] = "VehicleService",

            // Public endpoints (no authentication required)
            ["Routes:19:UpstreamPathTemplate"] = "/health",
            ["Routes:19:DownstreamPathTemplate"] = "/health",
            ["Routes:19:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:19:DownstreamHostAndPorts:0:Port"] = "7000",
            ["Routes:19:DownstreamScheme"] = "http",

            ["Routes:20:UpstreamPathTemplate"] = "/swagger",
            ["Routes:20:DownstreamPathTemplate"] = "/swagger",
            ["Routes:20:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:20:DownstreamHostAndPorts:0:Port"] = "7000",
            ["Routes:20:DownstreamScheme"] = "http",

            ["Routes:21:UpstreamPathTemplate"] = "/api/gateway/services",
            ["Routes:21:DownstreamPathTemplate"] = "/api/gateway/services",
            ["Routes:21:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:21:DownstreamHostAndPorts:0:Port"] = "7000",
            ["Routes:21:DownstreamScheme"] = "http",

            // Global Configuration
            ["GlobalConfiguration:BaseUrl"] = "http://localhost:7000"
        };
    }

    #endregion
}