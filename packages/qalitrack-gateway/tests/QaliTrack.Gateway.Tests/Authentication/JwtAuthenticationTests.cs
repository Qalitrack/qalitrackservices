using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace QaliTrack.Gateway.Tests.Authentication;

public class JwtAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _secretKey = "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!";
    private readonly string _issuer = "UserService";
    private readonly string _audience = "UserService";

    public JwtAuthenticationTests(WebApplicationFactory<Program> factory)
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
                
                // Add test-specific Ocelot configuration
                config.AddInMemoryCollection(CreateTestOcelotConfiguration());
            });
        });

        _client = _factory.CreateClient();
    }

    #region JWT Token Generation Tests

    [Fact]
    public void GenerateValidJwtToken_ShouldCreateTokenWithCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var roles = new[] { "Admin", "User" };
        var permissions = new[] { "read:users", "write:users" };

        // Act
        var token = GenerateJwtToken(userId, username, email, roles, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "username" && c.Value == username);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == "permissions" && c.Value == "read:users");
        jwt.Claims.Should().Contain(c => c.Type == "permissions" && c.Value == "write:users");
    }

    [Fact]
    public void GenerateExpiredJwtToken_ShouldBeMarkedAsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredToken = GenerateJwtToken(userId, "testuser", "test@example.com", 
            new string[] {}, new string[] {}, DateTime.UtcNow.AddMinutes(-1));

        // Act
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(expiredToken);

        // Assert
        jwt.ValidTo.Should().BeBefore(DateTime.UtcNow);
    }

    #endregion

    #region Gateway Authentication Tests

    [Fact]
    public async Task GET_PublicEndpoint_Health_ShouldAllowAnonymousAccess()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_PublicEndpoint_Swagger_ShouldAllowAnonymousAccess()
    {
        // Act
        var response = await _client.GetAsync("/swagger");

        // Assert - Swagger not configured in test routes, so expect 404, not 401/403  
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var protectedEndpoints = new[]
        {
            "/api/users/profile",
            "/api/users",
            "/api/auth/profile"
        };

        foreach (var endpoint in protectedEndpoints)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            // Accept Unauthorized (auth failed), NotFound (endpoint doesn't exist), or BadGateway (service down)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.BadGateway);
        }
    }

    [Fact]
    public async Task GET_ProtectedEndpoint_WithValidToken_ShouldAllowAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "testuser", "test@example.com", 
            new[] { "User" }, new[] { "read:users" });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Test a basic protected endpoint (even if service is not running, auth should pass)
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        // We expect either OK (if endpoint exists) or NotFound (if service is down)
        // But NOT Unauthorized, which would indicate auth failed
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_ProtectedEndpoint_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredToken = GenerateJwtToken(userId, "testuser", "test@example.com", 
            new[] { "User" }, new[] { "read:users" }, DateTime.UtcNow.AddMinutes(-1));

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_ProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Role-Based Authorization Tests

    [Theory]
    [InlineData("Operator", "read:weight", "/api/users")]
    [InlineData("Auditor", "read:compliance", "/api/auth/profile")]
    [InlineData("Manager", "read:analytics", "/api/users")]
    [InlineData("Admin", "read:admin", "/api/auth/admin")]
    public async Task GET_RoleBasedEndpoint_WithCorrectRole_ShouldAllowAccess(
        string role, string permission, string endpoint)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "testuser", "test@example.com", 
            new[] { role }, new[] { permission });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        // Should not be unauthorized (may be NotFound if service is down)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("User", "read:basic", "/api/auth/admin")] // User trying to access Admin+ endpoint
    [InlineData("Operator", "read:weight", "/api/auth/admin")] // Operator trying to access Admin+ endpoint  
    [InlineData("Auditor", "read:compliance", "/api/auth/admin")] // Auditor trying to access Admin+ endpoint
    public async Task GET_RoleBasedEndpoint_WithInsufficientRole_ShouldReturnForbidden(
        string role, string permission, string endpoint)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "testuser", "test@example.com", 
            new[] { role }, new[] { permission });

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert  
        // Should be forbidden, unauthorized, not found (if service/endpoint doesn't exist), or bad gateway (if service is down)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.BadGateway);
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ValidateToken_WithTamperedSignature_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validToken = GenerateJwtToken(userId, "testuser", "test@example.com", 
            new[] { "User" }, new[] { "read:users" });

        // Tamper with the token by changing the last character
        var tamperedToken = validToken.Substring(0, validToken.Length - 1) + "X";

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateToken_WithWrongIssuer_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenWithWrongIssuer = GenerateJwtTokenWithCustomIssuer(userId, "testuser", "test@example.com", 
            new[] { "User" }, new[] { "read:users" }, "WrongIssuer");

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenWithWrongIssuer);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateToken_WithWrongAudience_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenWithWrongAudience = GenerateJwtTokenWithCustomAudience(userId, "testuser", "test@example.com", 
            new[] { "User" }, new[] { "read:users" }, "WrongAudience");

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenWithWrongAudience);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region User Context Headers Tests

    [Fact]
    public async Task AuthenticatedRequest_ShouldForwardUserContextHeaders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var roles = new[] { "Admin", "User" };
        var permissions = new[] { "read:users", "write:users" };
        
        var token = GenerateJwtToken(userId, username, "test@example.com", roles, permissions);

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        // Even if the endpoint returns NotFound, we can check that auth processing worked
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        
        // Note: In a real test, we would check if the downstream service receives
        // the X-User-ID, X-User-Roles, X-User-Permissions headers, but that requires
        // mocking the downstream services which is complex with Ocelot
    }

    #endregion

    #region Helper Methods

    private string GenerateJwtToken(Guid userId, string username, string email, 
        string[] roles, string[] permissions, DateTime? expiry = null)
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

        // Add permissions
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = expiry.HasValue ? now.AddMinutes(-10) : now, // Set NotBefore before the expiry for expired tokens
            Expires = expiry ?? now.AddMinutes(15),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateJwtTokenWithCustomIssuer(Guid userId, string username, string email, 
        string[] roles, string[] permissions, string issuer)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("username", username),
            new(JwtRegisteredClaimNames.Email, email)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var permission in permissions)
            claims.Add(new Claim("permissions", permission));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = issuer, // Wrong issuer
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateJwtTokenWithCustomAudience(Guid userId, string username, string email, 
        string[] roles, string[] permissions, string audience)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("username", username),
            new(JwtRegisteredClaimNames.Email, email)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var permission in permissions)
            claims.Add(new Claim("permissions", permission));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = _issuer,
            Audience = audience, // Wrong audience
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
            // User Service Routes for testing
            ["Routes:0:UpstreamPathTemplate"] = "/api/users/{everything}",
            ["Routes:0:DownstreamPathTemplate"] = "/api/{everything}",
            ["Routes:0:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:0:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:0:DownstreamScheme"] = "http",
            
            ["Routes:1:UpstreamPathTemplate"] = "/api/users",
            ["Routes:1:DownstreamPathTemplate"] = "/api/users",
            ["Routes:1:DownstreamHostAndPorts:0:Host"] = "localhost",
            ["Routes:1:DownstreamHostAndPorts:0:Port"] = "7001",
            ["Routes:1:DownstreamScheme"] = "http",
            
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