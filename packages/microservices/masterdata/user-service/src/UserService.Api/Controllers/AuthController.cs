using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;

namespace UserService.Api.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthenticationService _authService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var result = await _authService.LoginAsync(request);
            return Ok(result, "Login successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return InternalServerError("An error occurred during login");
        }
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var result = await _authService.RegisterAsync(request);
            return Created(result, "Registration successful");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return InternalServerError("An error occurred during registration");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required");
            }

            var result = await _authService.RefreshTokenAsync(request);
            return Ok(result, "Token refreshed successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return InternalServerError("An error occurred during token refresh");
        }
    }

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        try
        {
            await _authService.LogoutAsync(request);
            return Ok<object?>(null, "Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return InternalServerError("An error occurred during logout");
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest("New passwords do not match");
            }

            var result = await _authService.ChangePasswordAsync(userId, request);
            if (!result)
            {
                return BadRequest("Failed to change password. Please check your current password.");
            }

            return Ok<object?>(null, "Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return InternalServerError("An error occurred while changing password");
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required");
            }

            await _authService.ResetPasswordAsync(request);
            return Ok<object?>(null, "Password reset email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request");
            return InternalServerError("An error occurred while processing password reset");
        }
    }

    /// <summary>
    /// Confirm password reset with token
    /// </summary>
    [HttpPost("reset-password/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmDto request)
    {
        try
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest("Passwords do not match");
            }

            var result = await _authService.ResetPasswordConfirmAsync(request);
            if (!result)
            {
                return BadRequest("Invalid or expired reset token");
            }

            return Ok<object?>(null, "Password reset successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset confirmation");
            return InternalServerError("An error occurred while resetting password");
        }
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto request)
    {
        try
        {
            var result = await _authService.ConfirmEmailAsync(request);
            if (!result)
            {
                return BadRequest("Invalid confirmation token");
            }

            return Ok<object?>(null, "Email confirmed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email confirmation");
            return InternalServerError("An error occurred while confirming email");
        }
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    [HttpPost("resend-confirmation")]
    [Authorize]
    public async Task<IActionResult> ResendEmailConfirmation()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _authService.SendEmailConfirmationAsync(userId);
            if (!result)
            {
                return BadRequest("Failed to send confirmation email");
            }

            return Ok<object?>(null, "Confirmation email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while resending email confirmation");
            return InternalServerError("An error occurred while sending confirmation email");
        }
    }
}