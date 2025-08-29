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
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IShiftService _shiftService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;
        private readonly IUserStatusService _userStatusService;
        private readonly ITwoFactorService _twoFactorService;

        public AuthController(
            ITokenService tokenService, 
            IUserService userService,
            IShiftService shiftService,
            ILogger<AuthController> logger,
            IMapper mapper,
            IUserStatusService userStatusService,
            ITwoFactorService twoFactorService)
        {
            _tokenService = tokenService;
            _userService = userService;
            _shiftService = shiftService;
            _logger = logger;
            _mapper = mapper;
            _userStatusService = userStatusService;
            _twoFactorService = twoFactorService;
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

                    // NEW: Check shift-based login restrictions
                    var shiftRestrictionService = HttpContext.RequestServices.GetRequiredService<IShiftLoginRestrictionService>();
                    var (canLogin, restrictionReason) = await shiftRestrictionService.CanUserLoginAsync(user.Id.ToString());
                    
                    if (!canLogin)
                    {
                        return Unauthorized(new { 
                            message = restrictionReason,
                            errorCode = "SHIFT_RESTRICTION"
                        });
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

                    // 2FA is mandatory for all users - create session and send code
                    var sessionId = await _twoFactorService.CreateTwoFactorSessionAsync(user.Id.ToString());
                    var codeResult = await _twoFactorService.GenerateAndSendCodeAsync(user.Id.ToString(), user.Email);

                    if (!codeResult.Success)
                    {
                        return BadRequest(new { 
                            Success = false, 
                            Message = codeResult.Message, 
                            Errors = (string[])null, 
                            StatusCode = 400 
                        });
                    }

                    var response = new TwoFactorResponseDto
                    {
                        Requires2FA = true,
                        SessionId = sessionId,
                        Message = "Verification code sent to your email address. Please enter the code to complete login.",
                        Email = MaskEmail(user.Email)
                    };

                    return Ok(response);
                }
                catch (System.ComponentModel.DataAnnotations.ValidationException ex)
                {
                    _logger.LogWarning(ex, "Validation error during login for email: {Email}", loginDto?.Email ?? "unknown");
                    return BadRequest(new { 
                        Success = false, 
                        Message = ex.Message, 
                        Errors = (string[])null, 
                        StatusCode = 400 
                    });
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    string errorMessage = pgEx.SqlState switch
                    {
                        "23503" => "Referenced record does not exist",
                        "23514" => "Data validation failed - check constraint violation",
                        _ => $"Database error: {pgEx.MessageText}"
                    };

                    return BadRequest(new { 
                        Success = false, 
                        Message = errorMessage, 
                        Errors = (string[])null, 
                        StatusCode = 400 
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "An error occurred during login", 
                        Errors = (string[])null, 
                        StatusCode = 400 
                    });
                }
            }
            

        [HttpPost("verify-2fa")]
            public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorRequestDto request)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    // Verify the 2FA code
                    var verifyResult = await _twoFactorService.VerifyCodeAsync(request.SessionId, request.Code);

                    if (!verifyResult.Success)
                    {
                        return BadRequest(new { 
                            Success = false, 
                            Message = verifyResult.Message, 
                            Errors = (string[])null, 
                            StatusCode = 400 
                        });
                    }

                    // Get user from session (the same user object used during login)
                    var userId = await _twoFactorService.GetUserIdFromSessionAsync(request.SessionId);
                    if (string.IsNullOrEmpty(userId))
                    {
                        return BadRequest(new { 
                            Success = false, 
                            Message = "Invalid or expired session", 
                            Errors = (string[])null, 
                            StatusCode = 400 
                        });
                    }

                    // You already have the `user` data from the login, so there's no need to query the database again
                    UserReadDto? user = await _userService.GetByIdAsync(userId); // This line can be skipped if you store the user from login in the session
                    
                    
                    if (user.Roles != null)
                    {
                        foreach (var role in user.Roles)
                        {
                            _logger.LogInformation("Role: {RoleName}", role);
                        }
                    }
                    var token = await _tokenService.GenerateTokenForAuthenticatedUserAsync(user);

                    await _userService.UpdateUserActiveStatusAsync(userId, true);

                    var response = new LoginResponseDto
                    {
                        Token = token.Token,
                        Id = user.Id.ToString(),
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserRoles = user.Roles?.ToList() ?? new List<string>()
                    };

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "An error occurred during verification", 
                        Errors = (string[])null, 
                        StatusCode = 400 
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

                // Generate new token after password update - using same method as normal login
                var token = await _tokenService.GenerateTokenForAuthenticatedUserAsync(user);

                // Update user's online status using service layer
                await _userService.UpdateUserActiveStatusAsync(userId, true);

                var response = new LoginResponseDto
                {
                    Token = token.Token,
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserRoles = user.Roles?.ToList() ?? new List<string>(),
                };

                return Ok(new {
                    message = "Password updated successfully",
                    data = response
                });
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating password for user {UserId}", userId);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while updating password", 
                    Errors = (string[])null, 
                    StatusCode = 400 
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

                var userId = await _tokenService.GetUserIdFromTokenAsync(token);
                
                if (userId == null)
                {
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
                            return BadRequest(new { message = "Invalid token format" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { message = "Invalid token" });
                    }
                }
                
                await _userService.UpdateUserActiveStatusAsync(userId.Value.ToString(), false);
                
                var tokensDeleted = await _tokenService.DeleteAllTokensForUserAsync(userId.Value);
                
                _userStatusService.EnqueueStatusUpdate(userId.Value.ToString(), false);
                
                if (tokensDeleted)
                {
                    return (ActionResult)Ok(new { message = "Successfully logged out" });
                }
                else
                {
                    return (ActionResult)Ok(new { message = "Successfully logged out (no active sessions found)" });
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout");
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred during logout", 
                    Errors = (string[])null, 
                    StatusCode = 400 
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

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return $"{localPart[0]}***@{domain}";

            var maskedLocal = $"{localPart[0]}***{localPart[^1]}";
            return $"{maskedLocal}@{domain}";
        }
    }
}