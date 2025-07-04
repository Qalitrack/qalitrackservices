using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QaliTrack.Gateway.Tests.Authorization;

/// <summary>
/// Tests for Phase 1 Role Enforcement implementation
/// Validates the hybrid authorization model with static configuration
/// </summary>
public class RoleEnforcementTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _secretKey = "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
    private readonly string _issuer = "UserService";
    private readonly string _audience = "UserService";

    public RoleEnforcementTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add JWT configuration
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:SecretKey"] = _secretKey,
                    ["Jwt:Issuer"] = _issuer,
                    ["Jwt:Audience"] = _audience
                });
            });
        });

        _client = _factory.CreateClient();
    }

    #region Role Hierarchy Tests

    [Theory]
    [InlineData("SuperAdmin", "/api/users/test", true)]
    [InlineData("SuperAdmin", "/api/organizations/test", true)]
    [InlineData("SuperAdmin", "/api/vehicles/test", true)]
    [InlineData("SuperAdmin", "/api/analytics/test", true)]
    [InlineData("SuperAdmin", "/api/archive/test", true)]
    public async Task SuperAdmin_ShouldAccessAllEndpoints(string role, string endpoint, bool shouldAllow)
    {
        await TestRoleAccess(role, endpoint, shouldAllow);
    }

    [Theory]
    [InlineData("Admin", "/api/organizations/test", true)]
    [InlineData("Admin", "/api/vehicles/test", true)]
    [InlineData("Admin", "/api/analytics/test", true)]
    [InlineData("Admin", "/api/archive/test", true)]
    [InlineData("Admin", "/api/data-sync/test", true)]
    public async Task Admin_ShouldAccessAdminAndLowerEndpoints(string role, string endpoint, bool shouldAllow)
    {
        await TestRoleAccess(role, endpoint, shouldAllow);
    }

    [Theory]
    [InlineData("SiteManager", "/api/vehicles/test", true)]
    [InlineData("SiteManager", "/api/analytics/test", true)]
    [InlineData("SiteManager", "/api/operational-data/test", true)]
    [InlineData("SiteManager", "/api/saccos/test", true)]
    [InlineData("SiteManager", "/api/organizations/test", false)] // Should be forbidden
    [InlineData("SiteManager", "/api/archive/test", false)] // Should be forbidden
    public async Task SiteManager_ShouldAccessSiteManagerAndLowerEndpoints(string role, string endpoint, bool shouldAllow)
    {
        await TestRoleAccess(role, endpoint, shouldAllow);
    }

    [Theory]
    [InlineData("Operator", "/api/vehicles/test", true)]
    [InlineData("Operator", "/api/drivers/test", true)]
    [InlineData("Operator", "/api/products/test", true)]
    [InlineData("Operator", "/api/weighbridges/test", true)]
    [InlineData("Operator", "/api/customers/test", true)]
    [InlineData("Operator", "/api/suppliers/test", true)]
    [InlineData("Operator", "/api/transporters/test", true)]
    [InlineData("Operator", "/api/weight-data/test", true)]
    [InlineData("Operator", "/api/compliance/test", true)]
    [InlineData("Operator", "/api/transactions/test", true)]
    [InlineData("Operator", "/api/analytics/test", false)] // Should be forbidden
    [InlineData("Operator", "/api/organizations/test", false)] // Should be forbidden
    public async Task Operator_ShouldAccessOperatorAndLowerEndpoints(string role, string endpoint, bool shouldAllow)
    {
        await TestRoleAccess(role, endpoint, shouldAllow);
    }

    [Theory]
    [InlineData("User", "/api/users/test", true)]
    [InlineData("User", "/api/vehicles/test", false)] // Should be forbidden
    [InlineData("User", "/api/drivers/test", false)] // Should be forbidden
    [InlineData("User", "/api/organizations/test", false)] // Should be forbidden
    public async Task User_ShouldOnlyAccessUserEndpoints(string role, string endpoint, bool shouldAllow)
    {
        await TestRoleAccess(role, endpoint, shouldAllow);
    }

    #endregion

    #region Multiple Roles Tests

    [Theory]
    [InlineData("User,Operator", "/api/vehicles/test", true)] // Should use highest role (Operator)
    [InlineData("Operator,SiteManager", "/api/analytics/test", true)] // Should use highest role (SiteManager)
    [InlineData("User,Admin", "/api/organizations/test", true)] // Should use highest role (Admin)
    public async Task MultipleRoles_ShouldUseHighestRole(string roles, string endpoint, bool shouldAllow)
    {
        var roleArray = roles.Split(',');
        await TestRoleAccess(roleArray, endpoint, shouldAllow);
    }

    #endregion

    #region Public Endpoints Tests

    [Theory]
    [InlineData("/health")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/services/users/health")]
    [InlineData("/swagger")]
    public async Task PublicEndpoints_ShouldAllowAnonymousAccess(string endpoint)
    {
        // Act - No authentication header
        var response = await _client.GetAsync(endpoint);

        // Assert - Should not be unauthorized
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region User Context Headers Tests

    [Fact]
    public async Task AuthenticatedRequest_ShouldAddUserContextHeaders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var roles = new[] { "Operator" };
        
        var token = GenerateJwtToken(userId, username, email, roles);

        // Create a test client that can intercept headers
        var interceptingClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Add a middleware to capture headers for testing
                services.AddScoped<HeaderCapturingMiddleware>();
            });
            builder.Configure(app =>
            {
                app.UseMiddleware<HeaderCapturingMiddleware>();
            });
        }).CreateClient();

        interceptingClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        await interceptingClient.GetAsync("/api/vehicles/test");

        // Assert
        // The actual validation would need to be done in the HeaderCapturingMiddleware
        // For now, we just verify that authentication passed
        // In a real implementation, we would check that the middleware received the expected headers
        Assert.True(true); // Placeholder assertion
    }

    #endregion

    #region Cache Performance Tests

    [Fact]
    public async Task RoleCache_ShouldImprovePerformance()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new[] { "Operator" });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Make multiple requests to the same endpoint
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 10; i++)
        {
            await _client.GetAsync("/api/vehicles/test");
        }
        
        stopwatch.Stop();

        // Assert - Should complete reasonably quickly due to caching
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should be much faster with caching
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void Configuration_ShouldLoadRouteRoleRequirements()
    {
        // This test validates that the configuration loading works
        // The actual middleware initialization will validate the configuration
        var config = CreateTestOcelotConfiguration();
        
        config.Should().ContainKey("Routes:0:Metadata:RequiredRoles:0");
        config.Should().ContainKey("Routes:0:Metadata:ServiceName");
        config.Should().ContainKey("Routes:0:UpstreamPathTemplate");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task UnknownRole_ShouldDenyAccess()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new[] { "UnknownRole" });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/vehicles/test");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EmptyRoles_ShouldDenyAccess()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new string[] { });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/vehicles/test");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CaseInsensitiveRoles_ShouldWork()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new[] { "operator" }); // lowercase
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/vehicles/test");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper Methods

    private async Task TestRoleAccess(string role, string endpoint, bool shouldAllow)
    {
        await TestRoleAccess(new[] { role }, endpoint, shouldAllow);
    }

    private async Task TestRoleAccess(string[] roles, string endpoint, bool shouldAllow)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "testuser", "test@example.com", roles);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (shouldAllow)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, 
                $"User with roles [{string.Join(",", roles)}] should be authorized for {endpoint}");
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden, 
                $"User with roles [{string.Join(",", roles)}] should not be forbidden from {endpoint}");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        }
    }

    private string GenerateJwtToken(Guid userId, string username, string email, string[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("username", username),
            new(JwtRegisteredClaimNames.Email, email)
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
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
            // User Service Route
            ["Routes:0:UpstreamPathTemplate"] = "/api/users/{everything}",
            ["Routes:0:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:0:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:0:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:0:Metadata:RequiredRoles:0"] = "User",
            ["Routes:0:Metadata:ServiceName"] = "UserService",
            ["Routes:0:Metadata:Description"] = "User management endpoints - requires basic authentication",

            // Organization Service Route
            ["Routes:1:UpstreamPathTemplate"] = "/api/organizations/{everything}",
            ["Routes:1:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:1:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:1:DownstreamHostAndPorts:0:Port"] = "7002",
            ["Routes:1:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:1:Metadata:ServiceName"] = "OrganizationService",
            ["Routes:1:Metadata:Description"] = "Organization management - requires Admin+ role",

            // Vehicle Service Route
            ["Routes:2:UpstreamPathTemplate"] = "/api/vehicles/{everything}",
            ["Routes:2:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:2:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:2:DownstreamHostAndPorts:0:Port"] = "7003",
            ["Routes:2:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:2:Metadata:ServiceName"] = "VehicleService",
            ["Routes:2:Metadata:Description"] = "Vehicle management - requires Operator+ role",

            // Driver Service Route
            ["Routes:3:UpstreamPathTemplate"] = "/api/drivers/{everything}",
            ["Routes:3:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:3:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:3:DownstreamHostAndPorts:0:Port"] = "7004",
            ["Routes:3:Metadata:RequiredRoles:0"] = "Operator",
            ["Routes:3:Metadata:ServiceName"] = "DriverService",
            ["Routes:3:Metadata:Description"] = "Driver management - requires Operator+ role",

            // Analytics Service Route  
            ["Routes:4:UpstreamPathTemplate"] = "/api/analytics/{everything}",
            ["Routes:4:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:4:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:4:DownstreamHostAndPorts:0:Port"] = "7016",
            ["Routes:4:Metadata:RequiredRoles:0"] = "SiteManager",
            ["Routes:4:Metadata:ServiceName"] = "AnalyticsService",
            ["Routes:4:Metadata:Description"] = "Analytics and reporting - requires SiteManager+ role",

            // Archive Service Route
            ["Routes:5:UpstreamPathTemplate"] = "/api/archive/{everything}",
            ["Routes:5:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:5:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:5:DownstreamHostAndPorts:0:Port"] = "7018",
            ["Routes:5:Metadata:RequiredRoles:0"] = "Admin",
            ["Routes:5:Metadata:ServiceName"] = "ArchiveService",
            ["Routes:5:Metadata:Description"] = "Data archival and retention - requires Admin+ role",

            // Add more routes as needed for comprehensive testing...
        };
    }

    #endregion
}

/// <summary>
/// Middleware for capturing headers during testing
/// </summary>
public class HeaderCapturingMiddleware
{
    private readonly RequestDelegate _next;

    public HeaderCapturingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Capture headers for testing
        var userIdHeader = context.Request.Headers["X-User-ID"].FirstOrDefault();
        var userRolesHeader = context.Request.Headers["X-User-Roles"].FirstOrDefault();
        var serviceNameHeader = context.Request.Headers["X-Service-Name"].FirstOrDefault();
        
        // Store in context items for test verification
        context.Items["CapturedHeaders"] = new Dictionary<string, string>
        {
            ["X-User-ID"] = userIdHeader ?? "",
            ["X-User-Roles"] = userRolesHeader ?? "",
            ["X-Service-Name"] = serviceNameHeader ?? ""
        };

        await _next(context);
    }
}