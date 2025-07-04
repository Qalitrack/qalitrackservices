using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;

namespace UserService.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var user = await _userService.GetCompleteUserAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return InternalServerError("An error occurred while retrieving profile");
        }
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var user = await _userService.UpdateUserAsync(userId, request);
            return Ok(user, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return InternalServerError("An error occurred while updating profile");
        }
    }

    /// <summary>
    /// Get detailed user profile (with additional information)
    /// </summary>
    [HttpGet("profile/details")]
    public async Task<IActionResult> GetProfileDetails()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound("User profile not found");
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile details");
            return InternalServerError("An error occurred while retrieving profile details");
        }
    }

    /// <summary>
    /// Update detailed user profile
    /// </summary>
    [HttpPut("profile/details")]
    public async Task<IActionResult> UpdateProfileDetails([FromBody] UpdateUserProfileDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var profile = await _userService.UpdateUserProfileAsync(userId, request);
            return Ok(profile, "Profile details updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile details");
            return InternalServerError("An error occurred while updating profile details");
        }
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetUserRoles()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var roles = await _userService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles");
            return InternalServerError("An error occurred while retrieving user roles");
        }
    }

    /// <summary>
    /// Get user sessions
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetUserSessions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var sessions = await _userService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sessions");
            return InternalServerError("An error occurred while retrieving user sessions");
        }
    }

    /// <summary>
    /// Revoke a specific user session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> RevokeSession(string sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _userService.RevokeUserSessionAsync(userId, sessionId);
            if (!result)
            {
                return NotFound("Session not found");
            }

            return Ok<object?>(null, "Session revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking user session");
            return InternalServerError("An error occurred while revoking session");
        }
    }

    /// <summary>
    /// Revoke all user sessions
    /// </summary>
    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllSessions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _userService.RevokeAllUserSessionsAsync(userId);
            if (!result)
            {
                return BadRequest("Failed to revoke sessions");
            }

            return Ok<object?>(null, "All sessions revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all user sessions");
            return InternalServerError("An error occurred while revoking sessions");
        }
    }

    /// <summary>
    /// Check if username is available
    /// </summary>
    [HttpGet("check-username/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUsername(string username)
    {
        try
        {
            var available = await _userService.IsUsernameAvailableAsync(username);
            return Ok(new { available }, available ? "Username is available" : "Username is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability");
            return InternalServerError("An error occurred while checking username");
        }
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    [HttpGet("check-email/{email}")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmail(string email)
    {
        try
        {
            var available = await _userService.IsEmailAvailableAsync(email);
            return Ok(new { available }, available ? "Email is available" : "Email is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability");
            return InternalServerError("An error occurred while checking email");
        }
    }
}