
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Auth;
using UserService.Core.Interfaces;
using UserService.Core.Entities;
using AutoMapper;
using Serilog;
using UserService.Core.DTOs.User;

namespace UserService.Api.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly ITokenRepository _tokenRepository;
    private readonly IShiftService _shiftService;
    private readonly ILogger<AuthController> _logger;
    private readonly IMapper _mapper;

    public AuthController(
        ITokenService tokenService, 
        IUserService userService,
        ITokenRepository tokenRepository,
        IShiftService shiftService,
        ILogger<AuthController> logger,
        IMapper mapper)
    {
        _tokenService = tokenService;
        _userService = userService;
        _tokenRepository = tokenRepository;
        _shiftService = shiftService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.ValidateUserCredentials(loginDto.Email, loginDto.Password);
            
            if (user == null)
            {
                // If user is null, it could be either:
                // 1. User not found
                // 2. User exists but is deleted
                // 3. Invalid password
                // We want to return a consistent message for all these cases
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is not active" });
            }

            _logger.LogInformation("Starting login process for user {UserId} ({Email})", user.Id, user.Email);
            
            // First, check if the user has any active shift assigned (either strict or open)
            _logger.LogInformation("Checking if user {UserId} has any active shifts assigned", user.Id);
            bool hasActiveShiftForUser = await _shiftService.HasActiveStrictShiftForUserAsync(user.Id);
            
            if (hasActiveShiftForUser)
            {
                _logger.LogInformation("User {UserId} has an active shift assigned - allowing login", user.Id);
            }
            else
            {
                _logger.LogInformation("User {UserId} does not have an active shift assigned. Checking for any active strict shifts in the system", user.Id);
                
                // If the user doesn't have an active shift, check if there are any active strict shifts
                bool hasAnyActiveStrictShift = await _shiftService.HasActiveStrictShiftAsync();
                
                if (hasAnyActiveStrictShift)
                {
                    _logger.LogInformation("Found active strict shifts in the system. Checking if user {UserId} has admin role", user.Id);
                    
                    // Check if user has any admin role
                    bool hasAdminRole = user.UserRoles?.Any(ur => 
                        ur.Role != null && 
                        (ur.Role.Name?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true ||
                         ur.Role.Name?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true)
                    ) ?? false;

                    if (hasAdminRole)
                    {
                        _logger.LogInformation("User {UserId} has admin role - allowing login despite active strict shifts", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Login denied for user {UserId}: No active shift assigned and user is not an admin", user.Id);
                        return Unauthorized(new { message = "Login restricted during active shift. Please contact your administrator." });
                    }
                }
                else
                {
                    _logger.LogInformation("No active strict shifts found in the system - allowing login for user {UserId}", user.Id);
                }
            }

            var tokenResult = await _tokenService.GenerateTokenAsync(loginDto.Email, loginDto.Password);
            
            if (tokenResult == null)
            {
                _logger.LogError("Failed to generate token for user {UserId}", user.Id);
                return StatusCode(500, new { message = "Failed to generate authentication token" });
            }

            // Set the token in the response headers
            Response.Headers.Add("Authorization", $"Bearer {tokenResult.Token}");

            // Update user's online status in background
            _ = Task.Run(async () =>
            {
                try
                {
                    var updateUserDto = new UpdateUserDto
                    {
                        IsActive = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userService.UpdateAsync(user.Id, updateUserDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update user status in background for user {UserId}", user.Id);
                }
            }, default);

            var response = new LoginResponseDto
            {
                Token = tokenResult.Token,
                Id = user.Id.ToString(),
                Email = user.Email,
            };

            _logger.LogInformation("Login successful for user {UserId}", user.Id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for email: {Email}", loginDto?.Email ?? "unknown");
            return StatusCode(500, new { 
                message = "An error occurred during login", 
                error = ex.Message 
            });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var token = ExtractTokenFromHeader();
                
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "No token provided" });
            }

            var isValidToken = await _tokenService.ValidateTokenAsync(token);
            
            if (!isValidToken)
            {
                return Unauthorized(new { message = "Invalid or expired token" });
            }

            var userId = await _tokenService.GetUserIdFromTokenAsync(token);
                
            if (userId == null)
            {
                return BadRequest(new { message = "Unable to identify user from token" });
            }
            

            var revoked = await _tokenRepository.RevokeTokenAsync(userId.Value);
            
            if (revoked)
            {
                // Update user's online status in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var userIdStr = userId.Value.ToString();
                        var user = await _userService.GetByIdAsync(userIdStr);
                        if (user != null)
                        {
                            var updateUserDto = new UpdateUserDto
                            {
                                IsActive = false,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _userService.UpdateAsync(userIdStr, updateUserDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update user status in background for user {UserId}", userId.Value);
                    }
                }, default);
                
                return (ActionResult)Ok(new { message = "Successfully logged out" });
            }
            else
            {
                return BadRequest(new { message = "Failed to revoke token" });
            }

        }
        catch (Exception ex)
        {
            Log.Error("An error occurred during logout: " + ex.Message);
            return StatusCode(500, new { message = "An error occurred during logout", error = ex.Message });
        }
    }

    private string? ExtractTokenFromHeader()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader))
        {
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return authHeader.Trim();
        }

        return null;
    }
}
}
