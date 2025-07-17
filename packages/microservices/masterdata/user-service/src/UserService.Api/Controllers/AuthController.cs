using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly IUserStatusService _userStatusService;

        public AuthController(
            ITokenService tokenService, 
            IUserService userService,
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            IShiftService shiftService,
            ILogger<AuthController> logger,
            IMapper mapper,
            IUserStatusService userStatusService)
        {
            _tokenService = tokenService;
            _userService = userService;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _shiftService = shiftService;
            _logger = logger;
            _mapper = mapper;
            _userStatusService = userStatusService;
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

                if (user.IsDeleted)
                {
                    return Unauthorized(new { message = "Account has been deleted" });
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

                // Update user's online status using background service
                await _userRepository.UpdateUserActiveStatusAsync(user.Id.ToString(), true);
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
        [AllowAnonymous]
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

                // Update user's online status using background service
                await _userRepository.UpdateUserActiveStatusAsync(userId, true);


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

                // Try to get user ID from token even if it's expired
                var userId = await _tokenService.GetUserIdFromTokenAsync(token);
                
                if (userId == null)
                {
                    // If we can't get user ID, try to extract it manually from the token
                    try
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(token);
                        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => 
                            c.Type == ClaimTypes.NameIdentifier || 
                            c.Type == JwtRegisteredClaimNames.Sub);
                        
                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
                        {
                            userId = parsedUserId;
                        }
                        else
                        {
                            _logger.LogWarning("Could not extract user ID from token");
                            return BadRequest(new { message = "Invalid token format" });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse token");
                        return BadRequest(new { message = "Invalid token" });
                    }
                }
                
                // Update user's status directly in the database first
                await _userRepository.UpdateUserActiveStatusAsync(userId.Value.ToString(), false);
                
                // Delete all tokens for this user
                var tokensDeleted = await _tokenRepository.DeleteAllTokensForUserAsync(userId.Value);
                
                // Also enqueue the status update for background processing (for any other systems that might be listening)
                _userStatusService.EnqueueStatusUpdate(userId.Value.ToString(), false);
                
                if (tokensDeleted)
                {
                    _logger.LogInformation("Successfully logged out user {UserId} and deleted all tokens", userId.Value);
                    return (ActionResult)Ok(new { message = "Successfully logged out" });
                }
                else
                {
                    _logger.LogInformation("User {UserId} logged out, but no tokens were found to delete", userId.Value);
                    return (ActionResult)Ok(new { message = "Successfully logged out (no active sessions found)" });
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