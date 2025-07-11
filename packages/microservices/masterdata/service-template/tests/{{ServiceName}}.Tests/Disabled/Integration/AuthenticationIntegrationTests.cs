using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UserService.Api;
using UserService.Core.DTOs;
using UserService.Infrastructure.Data;
using Xunit;

namespace UserService.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly UserDbContext _context;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<UserDbContext>));
                services.RemoveAll(typeof(UserDbContext));

                // Add InMemory database for testing
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<UserDbContext>();
    }

    #region Registration Integration Tests

    [Fact]
    public async Task POST_Register_WithValidData_ShouldCreateUserAndReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "integrationtest",
            Email = "integration@test.com",
            Password = "IntegrationTest123!",
            FirstName = "Integration",
            LastName = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse<UserDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Username.Should().Be("integrationtest");
        result.Data.Email.Should().Be("integration@test.com");

        // Verify user was created in database
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "integrationtest");
        userInDb.Should().NotBeNull();
        userInDb.Email.Should().Be("integration@test.com");
    }

    [Fact]
    public async Task POST_Register_WithDuplicateUsername_ShouldReturnBadRequest()
    {
        // Arrange - Create user first
        var firstRequest = new RegisterRequestDto
        {
            Username = "duplicate",
            Email = "first@test.com",
            Password = "Test123!",
            FirstName = "First",
            LastName = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", firstRequest);

        // Second registration with same username
        var secondRequest = new RegisterRequestDto
        {
            Username = "duplicate",
            Email = "second@test.com",
            Password = "Test123!",
            FirstName = "Second",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse<UserDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Username already exists");
    }

    #endregion

    #region Login Integration Tests

    [Fact]
    public async Task POST_Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange - Register user first
        var registerRequest = new RegisterRequestDto
        {
            Username = "logintest",
            Email = "login@test.com",
            Password = "LoginTest123!",
            FirstName = "Login",
            LastName = "Test"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Manually confirm email for login test
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "logintest");
        user.EmailConfirmed = true;
        user.Status = UserService.Core.Entities.UserStatus.Active;
        await _context.SaveChangesAsync();

        var loginRequest = new LoginDto
        {
            Username = "logintest",
            Password = "LoginTest123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task POST_Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginDto
        {
            Username = "nonexistent",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Login_WithUnconfirmedEmail_ShouldReturnUnauthorized()
    {
        // Arrange - Register user but don't confirm email
        var registerRequest = new RegisterRequestDto
        {
            Username = "unconfirmed",
            Email = "unconfirmed@test.com",
            Password = "Unconfirmed123!",
            FirstName = "Unconfirmed",
            LastName = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginDto
        {
            Username = "unconfirmed",
            Password = "Unconfirmed123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email not confirmed");
    }

    #endregion

    #region Token Refresh Integration Tests

    [Fact]
    public async Task POST_RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange - Register and login user first
        var authTokens = await RegisterAndLoginUser("refreshtest", "refresh@test.com", "RefreshTest123!");

        var refreshRequest = new RefreshTokenDto
        {
            RefreshToken = authTokens.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.AccessToken.Should().NotBe(authTokens.AccessToken);
        result.Data.RefreshToken.Should().NotBe(authTokens.RefreshToken);
    }

    [Fact]
    public async Task POST_RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenDto
        {
            RefreshToken = "invalid_refresh_token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Password Reset Integration Tests

    [Fact]
    public async Task POST_RequestPasswordReset_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange - Register user first
        await RegisterAndLoginUser("resettest", "reset@test.com", "ResetTest123!");

        // Act
        var response = await _client.PostAsync("/api/auth/reset-password?email=reset@test.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify reset token was set in database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "reset@test.com");
        user.Should().NotBeNull();
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetTokenExpires.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task POST_ResetPassword_WithValidToken_ShouldUpdatePassword()
    {
        // Arrange - Register user and request password reset
        await RegisterAndLoginUser("passwordreset", "passwordreset@test.com", "OldPassword123!");
        await _client.PostAsync("/api/auth/reset-password?email=passwordreset@test.com", null);

        // Get the reset token from the database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "passwordreset@test.com");
        var resetToken = user.PasswordResetToken;

        var resetRequest = new ResetPasswordDto
        {
            Token = resetToken,
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password/confirm", resetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify password was changed by trying to login with new password
        var loginRequest = new LoginDto
        {
            Username = "passwordreset",
            Password = "NewPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Email Confirmation Integration Tests

    [Fact]
    public async Task POST_ConfirmEmail_WithValidToken_ShouldActivateUser()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Username = "confirmtest",
            Email = "confirm@test.com",
            Password = "ConfirmTest123!",
            FirstName = "Confirm",
            LastName = "Test"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Get the confirmation token from the database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "confirmtest");
        var confirmationToken = user.EmailConfirmationToken;

        // Act
        var response = await _client.PostAsync($"/api/auth/confirm-email?token={confirmationToken}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify email was confirmed
        await _context.Entry(user).ReloadAsync();
        user.EmailConfirmed.Should().BeTrue();
        user.Status.Should().Be(UserService.Core.Entities.UserStatus.Active);
        user.EmailConfirmationToken.Should().BeNull();
    }

    [Fact]
    public async Task POST_ResendEmailConfirmation_WithValidEmail_ShouldGenerateNewToken()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Username = "resendtest",
            Email = "resend@test.com",
            Password = "ResendTest123!",
            FirstName = "Resend",
            LastName = "Test"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "resendtest");
        var originalToken = user.EmailConfirmationToken;

        // Act
        var response = await _client.PostAsync("/api/auth/resend-confirmation?email=resend@test.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify new token was generated
        await _context.Entry(user).ReloadAsync();
        user.EmailConfirmationToken.Should().NotBeNullOrEmpty();
        user.EmailConfirmationToken.Should().NotBe(originalToken);
    }

    #endregion

    #region Helper Methods

    private async Task<LoginResponseDto> RegisterAndLoginUser(string username, string email, string password)
    {
        // Register
        var registerRequest = new RegisterRequestDto
        {
            Username = username,
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Confirm email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        user.EmailConfirmed = true;
        user.Status = UserService.Core.Entities.UserStatus.Active;
        await _context.SaveChangesAsync();

        // Login
        var loginRequest = new LoginDto
        {
            Username = username,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await loginResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ServiceResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result.Data;
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
        _scope.Dispose();
        _client.Dispose();
    }
}