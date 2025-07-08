using Xunit;
using UserService.Core.DTOs;

namespace UserService.Tests;

public class BasicTests
{
    [Fact]
    public void ServiceResponse_Should_Initialize_Correctly()
    {
        // Arrange & Act
        var response = new ServiceResponse<UserDto>
        {
            Success = true,
            Data = new UserDto(),
            Message = "Test message"
        };

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Test message", response.Message);
    }

    [Fact]
    public void LoginRequestDto_Should_Initialize_Correctly()
    {
        // Arrange & Act
        var loginRequest = new LoginRequestDto
        {
            Username = "testuser",
            Password = "testpassword",
            RememberMe = true
        };

        // Assert
        Assert.Equal("testuser", loginRequest.Username);
        Assert.Equal("testpassword", loginRequest.Password);
        Assert.True(loginRequest.RememberMe);
    }

    [Fact]
    public void RegisterRequestDto_Should_Initialize_Correctly()
    {
        // Arrange & Act
        var registerRequest = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "testpassword",
            ConfirmPassword = "testpassword",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        Assert.Equal("testuser", registerRequest.Username);
        Assert.Equal("test@example.com", registerRequest.Email);
        Assert.Equal("testpassword", registerRequest.Password);
        Assert.Equal("testpassword", registerRequest.ConfirmPassword);
        Assert.Equal("Test", registerRequest.FirstName);
        Assert.Equal("User", registerRequest.LastName);
    }

    [Fact]
    public void ConfirmEmailDto_Should_Initialize_Correctly()
    {
        // Arrange & Act
        var confirmEmail = new ConfirmEmailDto
        {
            UserId = "user123",
            Token = "token123"
        };

        // Assert
        Assert.Equal("user123", confirmEmail.UserId);
        Assert.Equal("token123", confirmEmail.Token);
    }

    [Fact]
    public void ChangePasswordRequestDto_Should_Initialize_Correctly()
    {
        // Arrange & Act
        var changePassword = new ChangePasswordRequestDto
        {
            CurrentPassword = "current",
            NewPassword = "new",
            ConfirmNewPassword = "new"
        };

        // Assert
        Assert.Equal("current", changePassword.CurrentPassword);
        Assert.Equal("new", changePassword.NewPassword);
        Assert.Equal("new", changePassword.ConfirmNewPassword);
    }
}