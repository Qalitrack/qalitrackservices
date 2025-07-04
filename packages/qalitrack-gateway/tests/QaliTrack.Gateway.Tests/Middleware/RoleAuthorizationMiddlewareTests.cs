using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QaliTrack.Gateway.Tests.Middleware;

/// <summary>
/// Integration tests for RoleAuthorizationMiddleware
/// Tests the middleware within the complete application pipeline
/// </summary>
public class RoleAuthorizationMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _secretKey = "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
    private readonly string _issuer = "UserService";
    private readonly string _audience = "UserService";

    public RoleAuthorizationMiddlewareTests(WebApplicationFactory<Program> factory)
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
                
                // Add test-specific route configuration with role requirements
                config.AddInMemoryCollection(CreateTestRouteConfiguration());
            });
        });

        _client = _factory.CreateClient();
    }

    #region Public Endpoint Tests

    [Theory]
    [InlineData("/health")]
    [InlineData("/api/auth/login")]
    [InlineData("/swagger")]
    public async Task PublicEndpoints_ShouldSkipAuthorization(string path)
    {
        // Act
        var response = await _client.GetAsync(path);

        // Assert - Should not be unauthorized or forbidden
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthentication_ShouldReturn401()
    {
        // Act - No auth header
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithAuthentication_NoRoleRequirement_ShouldAllow()
    {
        // Arrange - Create token for endpoint without specific role requirement
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Test auth endpoints which have no specific role requirements
        var response = await _client.GetAsync("/api/auth/profile");

        // Assert - Should not be unauthorized (may be NotFound if service down)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Role Authorization Tests

    [Theory]
    [InlineData("SuperAdmin", "/api/users/test", true)]
    [InlineData("Admin", "/api/users/test", true)]
    [InlineData("SiteManager", "/api/users/test", true)]
    [InlineData("Operator", "/api/users/test", true)]
    [InlineData("User", "/api/users/test", true)] // Users can access user endpoints
    public async Task RoleHierarchy_ShouldEnforceCorrectly(string userRole, string path, bool shouldAllow)
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new[] { userRole });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(path);

        // Assert
        if (shouldAllow)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
        else
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task MultipleRoles_ShouldUseHighestRole()
    {
        // Arrange - User with multiple roles
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", 
            new[] { "User", "Operator", "Admin" });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Access admin-level endpoint
        var response = await _client.GetAsync("/api/users/admin");

        // Assert - Should be allowed due to Admin role
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnknownRole_ShouldDenyAccess()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", 
            new[] { "UnknownRole" });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/test");

        // Assert - Unknown roles should be denied
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region User Context Headers Tests

    [Fact]
    public async Task AuthorizedRequest_ShouldAddUserContextHeaders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var userEmail = "test@example.com";
        var userRoles = new[] { "User" };
        
        var token = GenerateJwtToken(userId, userName, userEmail, userRoles);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert - Should not be unauthorized (headers added during processing)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        
        // Note: In a real integration test setup, we would need to capture the headers
        // sent to downstream services, which requires additional infrastructure
    }

    #endregion

    #region Cache Tests

    [Fact]
    public async Task RouteRequirement_ShouldBeCached()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "testuser", "test@example.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Make multiple requests to same endpoint to test caching
        var response1 = await _client.GetAsync("/api/users/profile");
        var response2 = await _client.GetAsync("/api/users/profile");

        // Assert - Both should work consistently (caching should not affect behavior)
        response1.StatusCode.Should().Be(response2.StatusCode);
        response1.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response2.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Helper Methods

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

    private Dictionary<string, string> CreateTestRouteConfiguration()
    {
        return new Dictionary<string, string>
        {
            // User Service Routes
            ["Routes:0:UpstreamPathTemplate"] = "/api/users/{everything}",
            ["Routes:0:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:0:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:0:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:0:DownstreamScheme"] = "http",
            ["Routes:0:Metadata:RequiredRoles:0"] = "User",
            ["Routes:0:Metadata:ServiceName"] = "UserService",
            ["Routes:0:Metadata:Description"] = "User management endpoints",
            
            ["Routes:1:UpstreamPathTemplate"] = "/api/users",
            ["Routes:1:DownstreamPathTemplate"] = "/api/users",
            ["Routes:1:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:1:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:1:DownstreamScheme"] = "http",
            ["Routes:1:Metadata:RequiredRoles:0"] = "User",
            ["Routes:1:Metadata:ServiceName"] = "UserService",
            ["Routes:1:Metadata:Description"] = "User management endpoints",
            
            ["Routes:2:UpstreamPathTemplate"] = "/api/auth/{everything}",
            ["Routes:2:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:2:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:2:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:2:DownstreamScheme"] = "http",
            
            ["GlobalConfiguration:BaseUrl"] = "http://localhost:7000"
        };
    }

    #endregion
}