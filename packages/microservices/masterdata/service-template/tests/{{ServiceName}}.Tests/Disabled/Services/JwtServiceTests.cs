using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using UserService.Core.Entities;
using UserService.Infrastructure.Services;
using Xunit;

namespace UserService.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public JwtServiceTests()
    {
        var configurationData = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!"},
            {"Jwt:Issuer", "UserService"},
            {"Jwt:Audience", "UserService"},
            {"Jwt:AccessTokenExpiry", "15"},
            {"Jwt:RefreshTokenExpiry", "10080"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    #region Access Token Generation Tests

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnValidJwt()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        var roles = new List<string> { "Admin", "User" };
        var permissions = new List<string> { "read:users", "write:users" };

        // Act
        var token = _jwtService.GenerateAccessToken(user, roles, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "username" && c.Value == user.Username);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == "roles" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == "roles" && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == "permissions" && c.Value == "read:users");
        jwt.Claims.Should().Contain(c => c.Type == "permissions" && c.Value == "write:users");
    }

    [Fact]
    public void GenerateAccessToken_WithEmptyRoles_ShouldReturnTokenWithoutRoles()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var roles = new List<string>();
        var permissions = new List<string>();

        // Act
        var token = _jwtService.GenerateAccessToken(user, roles, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        jwt.Claims.Should().NotContain(c => c.Type == "roles");
        jwt.Claims.Should().NotContain(c => c.Type == "permissions");
    }

    #endregion

    #region Refresh Token Generation Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
        token1.Length.Should().BeGreaterThan(20);
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        var isValid = _jwtService.ValidateToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var isValid = _jwtService.ValidateToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithNullOrEmptyToken_ShouldReturnFalse()
    {
        // Act & Assert
        _jwtService.ValidateToken(null).Should().BeFalse();
        _jwtService.ValidateToken("").Should().BeFalse();
        _jwtService.ValidateToken(" ").Should().BeFalse();
    }

    #endregion

    #region Token Data Extraction Tests

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        var extractedUserId = _jwtService.GetUserIdFromToken(token);

        // Assert
        extractedUserId.Should().Be(userId);
    }

    [Fact]
    public void GetUsernameFromToken_WithValidToken_ShouldReturnUsername()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        var extractedUsername = _jwtService.GetUsernameFromToken(token);

        // Assert
        extractedUsername.Should().Be("testuser");
    }

    [Fact]
    public void GetRolesFromToken_WithValidToken_ShouldReturnRoles()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var roles = new List<string> { "Admin", "User", "Manager" };
        var token = _jwtService.GenerateAccessToken(user, roles, new List<string>());

        // Act
        var extractedRoles = _jwtService.GetRolesFromToken(token);

        // Assert
        extractedRoles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void GetPermissionsFromToken_WithValidToken_ShouldReturnPermissions()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var permissions = new List<string> { "read:users", "write:users", "delete:users" };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), permissions);

        // Act
        var extractedPermissions = _jwtService.GetPermissionsFromToken(token);

        // Assert
        extractedPermissions.Should().BeEquivalentTo(permissions);
    }

    #endregion

    #region Token Expiry Tests

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectExpiry()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        var actualExpiry = jwt.ValidTo;
        
        actualExpiry.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void IsTokenExpired_WithExpiredToken_ShouldReturnTrue()
    {
        // Arrange - Create configuration with very short expiry
        var shortExpiryConfig = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!"},
            {"Jwt:Issuer", "UserService"},
            {"Jwt:Audience", "UserService"},
            {"Jwt:AccessTokenExpiry", "0"} // 0 minutes - expires immediately
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(shortExpiryConfig)
            .Build();

        var jwtService = new JwtService(config);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };

        var token = jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Wait a moment to ensure token is expired
        await Task.Delay(100);

        // Act
        var isExpired = jwtService.IsTokenExpired(token);

        // Assert
        isExpired.Should().BeTrue();
    }

    #endregion

    #region Token Revocation Tests

    [Fact]
    public void RevokeToken_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        _jwtService.RevokeToken(token);

        // Assert
        var isRevoked = _jwtService.IsTokenRevoked(token);
        isRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsTokenRevoked_WithNonRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>(), new List<string>());

        // Act
        var isRevoked = _jwtService.IsTokenRevoked(token);

        // Assert
        isRevoked.Should().BeFalse();
    }

    #endregion
}