using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Api.Controllers;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = new ServiceResponse<UserDto>
        {
            Success = true,
            Data = new UserDto
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com"
            }
        };

        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterUserDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterUserDto
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "weak"
        };

        var response = new ServiceResponse<UserDto>
        {
            Success = false,
            Message = "Validation failed"
        };

        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterUserDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Register_WithException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterUserDto>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithTokens()
    {
        // Arrange
        var request = new LoginDto
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var response = new ServiceResponse<AuthResponseDto>
        {
            Success = true,
            Data = new AuthResponseDto
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            }
        };

        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginDto
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var response = new ServiceResponse<AuthResponseDto>
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Value.Should().BeEquivalentTo(response);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnOkWithNewTokens()
    {
        // Arrange
        var request = new RefreshTokenDto
        {
            RefreshToken = "valid_refresh_token"
        };

        var response = new ServiceResponse<AuthResponseDto>
        {
            Success = true,
            Data = new AuthResponseDto
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            }
        };

        _authServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenDto
        {
            RefreshToken = "invalid_refresh_token"
        };

        var response = new ServiceResponse<AuthResponseDto>
        {
            Success = false,
            Message = "Invalid refresh token"
        };

        _authServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var response = new ServiceResponse<bool>
        {
            Success = true,
            Data = true
        };

        _authServiceMock.Setup(x => x.LogoutAsync(userId, sessionId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Logout(userId, sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public async Task RequestPasswordReset_WithValidEmail_ShouldReturnOk()
    {
        // Arrange
        var email = "test@example.com";

        var response = new ServiceResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Password reset email sent"
        };

        _authServiceMock.Setup(x => x.RequestPasswordResetAsync(email))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RequestPasswordReset(email);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task RequestPasswordReset_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = "nonexistent@example.com";

        var response = new ServiceResponse<bool>
        {
            Success = false,
            Message = "Email not found"
        };

        _authServiceMock.Setup(x => x.RequestPasswordResetAsync(email))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RequestPasswordReset(email);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var request = new ResetPasswordDto
        {
            Token = "valid_token",
            NewPassword = "NewPassword123!"
        };

        var response = new ServiceResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Password reset successfully"
        };

        _authServiceMock.Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ResetPasswordDto
        {
            Token = "invalid_token",
            NewPassword = "NewPassword123!"
        };

        var response = new ServiceResponse<bool>
        {
            Success = false,
            Message = "Invalid or expired token"
        };

        _authServiceMock.Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Email Confirmation Tests

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var token = "valid_confirmation_token";

        var response = new ServiceResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Email confirmed successfully"
        };

        _authServiceMock.Setup(x => x.ConfirmEmailAsync(token))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ConfirmEmail(token);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var token = "invalid_confirmation_token";

        var response = new ServiceResponse<bool>
        {
            Success = false,
            Message = "Invalid confirmation token"
        };

        _authServiceMock.Setup(x => x.ConfirmEmailAsync(token))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ConfirmEmail(token);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResendEmailConfirmation_WithValidEmail_ShouldReturnOk()
    {
        // Arrange
        var email = "test@example.com";

        var response = new ServiceResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Confirmation email sent"
        };

        _authServiceMock.Setup(x => x.ResendEmailConfirmationAsync(email))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ResendEmailConfirmation(email);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        var response = new ServiceResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Password changed successfully"
        };

        _authServiceMock.Setup(x => x.ChangePasswordAsync(userId, It.IsAny<ChangePasswordDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ChangePassword(userId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!"
        };

        var response = new ServiceResponse<bool>
        {
            Success = false,
            Message = "Current password is incorrect"
        };

        _authServiceMock.Setup(x => x.ChangePasswordAsync(userId, It.IsAny<ChangePasswordDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ChangePassword(userId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}