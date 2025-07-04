using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Services;
using UserService.Infrastructure.Data;
using Xunit;

namespace UserService.Tests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly UserDbContext _context;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemory(Guid.NewGuid().ToString())
            .Options;
        
        _context = new UserDbContext(options);
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        
        _authService = new AuthenticationService(
            _context,
            _passwordServiceMock.Object,
            _jwtServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region User Registration Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        _passwordServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");
        _emailServiceMock.Setup(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Username.Should().Be("testuser");
        result.Data.Email.Should().Be("test@example.com");

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        userInDb.Should().NotBeNull();
        userInDb.Status.Should().Be(UserStatus.Pending);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldReturnError()
    {
        // Arrange
        await _context.Users.AddAsync(new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash"
        });
        await _context.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnError()
    {
        // Arrange
        await _context.Users.AddAsync(new User
        {
            Username = "user1",
            Email = "existing@example.com",
            PasswordHash = "hash"
        });
        await _context.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Username = "user2",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email already exists");
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Status = UserStatus.Active,
            EmailConfirmed = true
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new LoginDto
        {
            Username = "testuser",
            Password = "password123"
        };

        _passwordServiceMock.Setup(x => x.VerifyPassword("password123", "hashedpassword"))
            .Returns(true);
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .Returns("access_token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Status = UserStatus.Active
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new LoginDto
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _passwordServiceMock.Setup(x => x.VerifyPassword("wrongpassword", "hashedpassword"))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithLockedAccount_ShouldReturnError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Status = UserStatus.Locked,
            LockoutEnd = DateTime.UtcNow.AddHours(1)
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new LoginDto
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Account is locked");
    }

    [Fact]
    public async Task LoginAsync_WithUnconfirmedEmail_ShouldReturnError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new LoginDto
        {
            Username = "testuser",
            Password = "password123"
        };

        _passwordServiceMock.Setup(x => x.VerifyPassword("password123", "hashedpassword"))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email not confirmed");
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Status = UserStatus.Active
        };
        await _context.Users.AddAsync(user);

        var session = new UserSession
        {
            UserId = user.Id,
            RefreshToken = "valid_refresh_token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        await _context.UserSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenDto
        {
            RefreshToken = "valid_refresh_token"
        };

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .Returns("new_access_token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.AccessToken.Should().Be("new_access_token");
        result.Data.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnError()
    {
        // Arrange
        var session = new UserSession
        {
            RefreshToken = "expired_refresh_token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };
        await _context.UserSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenDto
        {
            RefreshToken = "expired_refresh_token"
        };

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid or expired refresh token");
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public async Task RequestPasswordResetAsync_WithValidEmail_ShouldSendResetEmail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Status = UserStatus.Active
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _emailServiceMock.Setup(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RequestPasswordResetAsync("test@example.com");

        // Assert
        result.Success.Should().BeTrue();
        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync("test@example.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldUpdatePassword()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordResetToken = "valid_token",
            PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1),
            Status = UserStatus.Active
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new ResetPasswordDto
        {
            Token = "valid_token",
            NewPassword = "NewPassword123!"
        };

        _passwordServiceMock.Setup(x => x.HashPassword("NewPassword123!"))
            .Returns("new_hashed_password");

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hashed_password");
        user.PasswordResetToken.Should().BeNull();
    }

    #endregion

    #region Email Confirmation Tests

    [Fact]
    public async Task ConfirmEmailAsync_WithValidToken_ShouldConfirmEmail()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            EmailConfirmationToken = "valid_token",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ConfirmEmailAsync("valid_token");

        // Assert
        result.Success.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.EmailConfirmationToken.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithInvalidToken_ShouldReturnError()
    {
        // Act
        var result = await _authService.ConfirmEmailAsync("invalid_token");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid confirmation token");
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}