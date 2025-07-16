using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Auth;
using UserService.Core.Interfaces;
using UserService.Core.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using UserService.Core.DTOs.User;
using UserService.Infrastructure.Repositories;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IShiftService _shiftService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;

        public AuthController(
            ITokenService tokenService, 
            IUserService userService,
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            IShiftService shiftService,
            ILogger<AuthController> logger,
            IMapper mapper)
        {
            _tokenService = tokenService;
            _userService = userService;
            _userRepository = userRepository;
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
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Account is not active" });
                }

                // If it's the first login, redirect to password update
                if (user.IsFirstLogin)
                {
                    return Ok(new { 
                        message = "First login detected",
                        userId = user.Id,
                        redirectUrl = $"/api/auth/update-password/{user.Id}"
                    });
                }

                var token = await _tokenService.GenerateTokenAsync(user.Email, loginDto.Password);

                // Update user's online status in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _userRepository.UpdateUserActiveStatusAsync(user.Id, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update user status in background for user {UserId}", user.Id);
                    }
                }, default);

                var response = new LoginResponseDto
                {
                    Token = token.Token,
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserRoles = user.UserRoles?.Select(ur => ur.Role.Name).ToList()
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

        [HttpPut("update-password/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(string userId, [FromBody] UpdatePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.UpdatePassword(userId, dto);

                // Generate new token after password update
                var token = await _tokenService.GenerateTokenAsync(user.Email, dto.NewPassword);

                // Update user's online status in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _userRepository.UpdateUserActiveStatusAsync(userId, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update user status in background for user {UserId}", userId);
                    }
                }, default);

                var response = new LoginResponseDto
                {
                    Token = token.Token,
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserRoles = user.Roles
                };

                return Ok(new {
                    message = "Password updated successfully",
                    data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating password for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while updating password", error = ex.Message });
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
                            await _userRepository.UpdateUserActiveStatusAsync(userIdStr, false);
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
                _logger.LogError(ex, "An error occurred during logout");
                return StatusCode(500, new { 
                    message = "An error occurred during logout", 
                    error = ex.Message 
                });
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