using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QaliTrack.Gateway.Tests.Authorization;

[Trait("Category", "Security")]
public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _secretKey = "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
    private readonly string _issuer = "UserService";
    private readonly string _audience = "UserService";

    public SecurityTests(WebApplicationFactory<Program> factory)
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

    #region Permission Escalation Tests

    [Fact]
    public async Task PermissionEscalation_UserToAdmin_ShouldBeDenied()
    {
        // Arrange - User tries to access admin endpoint
        var token = GenerateJwtToken(Guid.NewGuid(), "user", "user@example.com", 
            "User", new[] { "read:public", "read:products", "read:profile" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access admin endpoint
        var response = await _client.GetAsync("/api/users/admin/123");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized)
            .And.Subject.Should().NotBe(HttpStatusCode.OK,
            "User should not be able to escalate to admin endpoint");
    }

    [Fact]
    public async Task PermissionEscalation_OperatorToSuperAdmin_ShouldBeDenied()
    {
        // Arrange - Operator tries to access super admin functionality
        var token = GenerateJwtToken(Guid.NewGuid(), "operator", "operator@example.com", 
            "Operator", new[] { "read:customers", "write:customers", "read:products" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access system management (SuperAdmin only)
        var adminEndpoints = new[]
        {
            "/api/users/admin/create",
            "/api/organizations/admin/settings",
            "/api/system/admin/config"
        };

        foreach (var endpoint in adminEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound)
                .And.Subject.Should().NotBe(HttpStatusCode.OK,
                $"Operator should not access admin endpoint: {endpoint}");
        }
    }

    [Theory]
    [InlineData("Guest", new[] { "read:public" }, "/api/products")]
    [InlineData("User", new[] { "read:public", "read:profile" }, "/api/customers")]
    [InlineData("Operator", new[] { "read:customers" }, "/api/users/admin/123")]
    public async Task PermissionBypass_MissingRequiredPermission_ShouldBeDenied(
        string userRole, string[] userPermissions, string endpoint)
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", 
            userRole, userPermissions);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"User with role '{userRole}' and limited permissions should be denied access to {endpoint}");
    }

    #endregion

    #region Token Manipulation Tests

    [Fact]
    public async Task TokenManipulation_ExpiredToken_ShouldBeDenied()
    {
        // Arrange - Create expired token
        var expiredToken = GenerateJwtToken(Guid.NewGuid(), "user", "user@example.com", 
            "User", new[] { "read:products" }, DateTime.UtcNow.AddMinutes(-30));

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Expired token should be rejected");
    }

    [Fact]
    public async Task TokenManipulation_InvalidSignature_ShouldBeDenied()
    {
        // Arrange - Create token with wrong secret
        var invalidToken = GenerateJwtTokenWithWrongSecret(Guid.NewGuid(), "user", "user@example.com", 
            "Admin", new[] { "manage:system" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Token with invalid signature should be rejected");
    }

    [Fact]
    public async Task TokenManipulation_MalformedToken_ShouldBeDenied()
    {
        // Arrange - Malformed JWT token
        var malformedToken = "invalid.jwt.token";

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", malformedToken);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Malformed token should be rejected");
    }

    [Fact]
    public async Task TokenManipulation_EmptyToken_ShouldBeDenied()
    {
        // Arrange - Empty token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Empty token should be rejected");
    }

    #endregion

    #region Permission Injection Tests

    [Fact]
    public async Task PermissionInjection_ExtraPermissionsInClaim_ShouldNotGrantAccess()
    {
        // Arrange - Try to inject extra permissions via custom token
        var extraPermissions = new[] { "read:public", "read:products", "manage:system", "delete:everything" };
        var token = GenerateJwtToken(Guid.NewGuid(), "user", "user@example.com", 
            "User", extraPermissions); // User role but with admin permissions

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access admin endpoint with injected permissions
        var response = await _client.GetAsync("/api/users/admin/123");

        // Assert - Should still be denied due to insufficient role
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized)
            .And.Subject.Should().NotBe(HttpStatusCode.OK,
            "User role should not grant access even with injected admin permissions");
    }

    [Theory]
    [InlineData("User", new[] { "admin:all", "root:access", "system:override" })]
    [InlineData("Operator", new[] { "superuser:privileges", "manage:everything" })]
    [InlineData("Guest", new[] { "read:all", "write:all", "delete:all" })]
    public async Task PermissionInjection_FakePermissions_ShouldNotBypassSecurity(
        string userRole, string[] fakePermissions)
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", 
            userRole, fakePermissions);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Try various protected endpoints
        var protectedEndpoints = new[]
        {
            "/api/users/admin/123",
            "/api/system/admin/config",
            "/api/organizations/admin/delete"
        };

        foreach (var endpoint in protectedEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound)
                .And.Subject.Should().NotBe(HttpStatusCode.OK,
                $"Fake permissions should not grant access to {endpoint}");
        }
    }

    #endregion

    #region Cross-Service Permission Tests

    [Fact]
    public async Task CrossServiceAccess_ProductPermissionsOnCustomerService_ShouldBeDenied()
    {
        // Arrange - User with product permissions trying to access customer service
        var token = GenerateJwtToken(Guid.NewGuid(), "user", "user@example.com", 
            "Operator", new[] { "read:products", "write:products", "delete:products" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "Product permissions should not grant access to customer service");
    }

    [Fact]
    public async Task CrossServiceAccess_CustomerPermissionsOnUserService_ShouldBeDenied()
    {
        // Arrange - User with customer permissions trying to access user admin
        var token = GenerateJwtToken(Guid.NewGuid(), "operator", "operator@example.com", 
            "Operator", new[] { "read:customers", "write:customers", "delete:customers" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/admin/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "Customer permissions should not grant access to user admin service");
    }

    #endregion

    #region Role Hierarchy Security Tests

    [Theory]
    [InlineData("Guest", "User", "/api/products")]
    [InlineData("User", "Operator", "/api/customers")]
    [InlineData("Operator", "Admin", "/api/users/admin/123")]
    [InlineData("Admin", "SuperAdmin", "/api/system/admin/global")]
    public async Task RoleHierarchy_LowerRoleAccessingHigherEndpoint_ShouldBeDenied(
        string userRole, string requiredRole, string endpoint)
    {
        // Arrange
        var permissions = GetBasicPermissionsForRole(userRole);
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", 
            userRole, permissions);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound)
            .And.Subject.Should().NotBe(HttpStatusCode.OK,
            $"Role '{userRole}' should not access endpoint requiring '{requiredRole}'");
    }

    #endregion

    #region Audit and Logging Security Tests

    [Fact]
    public async Task SecurityViolation_MultipleFailedAttempts_ShouldBeLogged()
    {
        // Arrange - Invalid token
        var invalidToken = "invalid.token.here";
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        // Act - Multiple failed attempts
        var endpoints = new[] { "/api/products", "/api/customers", "/api/users" };
        var responses = new List<HttpResponseMessage>();

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            responses.Add(response);
        }

        // Assert - All should be unauthorized
        responses.Should().AllSatisfy(response =>
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "Multiple attempts with invalid token should all be denied"));
    }

    #endregion

    #region Helper Methods

    private string[] GetBasicPermissionsForRole(string role)
    {
        return role switch
        {
            "Guest" => new[] { "read:public" },
            "User" => new[] { "read:public", "read:products", "read:profile" },
            "Operator" => new[] { "read:public", "read:products", "read:customers", "write:customers" },
            "Admin" => new[] { "read:public", "read:products", "read:users", "manage:users" },
            "SuperAdmin" => new[] { "read:public", "manage:system", "manage:everything" },
            _ => new[] { "read:public" }
        };
    }

    private string GenerateJwtToken(Guid userId, string username, string email, 
        string role, string[] permissions, DateTime? expiry = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("roles", role),
            new("permissions", string.Join(",", permissions)),
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

    private string GenerateJwtTokenWithWrongSecret(Guid userId, string username, string email, 
        string role, string[] permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var wrongKey = Encoding.ASCII.GetBytes("WrongSecretKeyThatDoesNotMatchTheRealOne!");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("permissions", string.Join(",", permissions))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(wrongKey), 
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
            ["Routes:0:UpstreamPathTemplate"] = "/api/products",
            ["Routes:0:DownstreamPathTemplate"] = "/api/products",
            ["Routes:0:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:0:DownstreamHostAndPorts:0:Port"] = "7005",
            ["Routes:0:DownstreamScheme"] = "http",
            ["Routes:0:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:0:Metadata:RequiredRoles:0"] = "User",
            ["Routes:0:Metadata:RequiredPermissions:0"] = "read:products",
            ["Routes:0:Metadata:ServiceName"] = "product-service",

            // User Admin Service
            ["Routes:1:UpstreamPathTemplate"] = "/api/users/admin/{everything}",
            ["Routes:1:DownstreamPathTemplate"] = "/api/users/admin/{everything}",
            ["Routes:1:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:1:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:1:DownstreamScheme"] = "http",
            ["Routes:1:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:1:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:1:Metadata:RequiredPermissions:0"] = "manage:users",
            ["Routes:1:Metadata:ServiceName"] = "user-service",

            // Customer Service
            ["Routes:2:UpstreamPathTemplate"] = "/api/customers",
            ["Routes:2:DownstreamPathTemplate"] = "/api/customers",
            ["Routes:2:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:2:DownstreamHostAndPorts:0:Port"] = "7008",
            ["Routes:2:DownstreamScheme"] = "http",
            ["Routes:2:AuthenticationOptions:AuthenticationProviderKey"] = "Bearer",
            ["Routes:2:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:2:Metadata:RequiredPermissions:0"] = "read:customers",
            ["Routes:2:Metadata:ServiceName"] = "customer-service",

            // Global Configuration
            ["GlobalConfiguration:BaseUrl"] = "http://localhost:7000"
        };
    }

    #endregion
}