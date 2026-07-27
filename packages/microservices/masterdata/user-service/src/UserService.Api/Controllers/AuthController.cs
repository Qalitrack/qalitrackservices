using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Auth;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using UserService.Core.DTOs.User;
using UserService.Core.Interfaces.Services;
using UserService.Core.Services;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : BaseController
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IShiftService _shiftService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;
        private readonly IUserStatusService _userStatusService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IShiftLoginRestrictionService _shiftLoginRestrictionService;
        private readonly PasswordPolicyService _passwordPolicyService;

        public AuthController(
            ITokenService tokenService,
            IUserService userService,
            IShiftService shiftService,
            ILogger<AuthController> logger,
            IMapper mapper,
            IUserStatusService userStatusService,
            ITwoFactorService twoFactorService,
            IShiftLoginRestrictionService shiftLoginRestrictionService,
            PasswordPolicyService passwordPolicyService)
        {
            _tokenService = tokenService;
            _userService = userService;
            _shiftService = shiftService;
            _logger = logger;
            _mapper = mapper;
            _userStatusService = userStatusService;
            _twoFactorService = twoFactorService;
            _shiftLoginRestrictionService = shiftLoginRestrictionService;
            _passwordPolicyService = passwordPolicyService;
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

                // ONLY check shift-based login restrictions (NO ATTENDANCE YET)
                var (canLogin, restrictionReason) = await _shiftLoginRestrictionService.CanUserLoginAsync(user.Id.ToString());

                if (!canLogin)
                {
                    _logger.LogWarning("Login denied for user {UserId} due to shift restrictions: {Reason}",
                        user.Id, restrictionReason);
                    return Unauthorized(new
                    {
                        message = restrictionReason,
                        errorCode = "SHIFT_RESTRICTION"
                    });
                }

                // If it's the first login, redirect to password update
                if (user.IsFirstLogin)
                {
                    return Ok(new
                    {
                        message = "First login detected",
                        userId = user.Id,
                        redirectUrl = $"/api/auth/update-password/{user.Id}"
                    });
                }

                // Check if 2FA is enabled (admin-toggleable, DB-backed via PasswordPolicy)
                var policy = await _passwordPolicyService.GetPolicyAsync();
                var is2FAEnabled = policy.TwoFactorEnabled;

                if (is2FAEnabled)
                {
                    // 2FA is enabled - create session and send code
                    var sessionId = await _twoFactorService.CreateTwoFactorSessionAsync(user.Id.ToString());
                    var codeResult = await _twoFactorService.GenerateAndSendCodeAsync(user.Id.ToString(), user.Email);

                    if (!codeResult.Success)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = codeResult.Message,
                            Errors = (string[]?)null,
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
                else
                {
                    // 2FA is disabled - generate token directly
                    var userDto = _mapper.Map<UserReadDto>(user);
                    var token = await _tokenService.GenerateTokenForAuthenticatedUserAsync(userDto);

                    // Update user active status
                    await _userService.UpdateUserActiveStatusAsync(user.Id.ToString(), true);

                    // Handle attendance after successful login
                    var attendanceHandled = await _shiftLoginRestrictionService.HandleLoginAttendanceAsync(user.Id.ToString());

                    _logger.LogInformation(
                        "User {UserId} successfully logged in without 2FA. Attendance handled: {AttendanceHandled}",
                        user.Id, attendanceHandled
                    );

                    var loginResponse = new LoginResponseDto
                    {
                        Token = token.Token,
                        Id = userDto.Id.ToString(),
                        Email = userDto.Email,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        UserRoles = userDto.Roles?.ToList() ?? new List<string>()
                    };

                    var loginMessage = attendanceHandled
                        ? "Successfully logged in and auto clocked-in to assigned shift"
                        : "Successfully logged in";

                    return Ok(new
                    {
                        data = loginResponse,
                        message = loginMessage,
                        attendanceHandled = attendanceHandled
                    });
                }
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error during login for email: {Email}", loginDto?.Email ?? "unknown");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = (string[]?)null,
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

                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = (string[]?)null,
                    StatusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user email: {Email}", loginDto?.Email ?? "unknown");
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = (string[]?)null,
                    StatusCode = 400
                });
            }
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorRequestDto request)
        {
            try
            {
                // Log incoming request
                _logger.LogInformation(
                    "VERIFY-2FA RECEIVED | SessionId='{Sid}' | Code='{Code}' | ModelStateValid={Valid}",
                    request?.SessionId ?? "(null)",
                    request?.Code ?? "(null)",
                    ModelState.IsValid
                );

                if (!ModelState.IsValid)
                {
                    var modelErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.LogWarning("Verify-2FA ModelState invalid: {Errors}", modelErrors);
                    return BadRequest(ModelState);
                }

                // Get userId BEFORE verification (before session is potentially deleted)
                var userId = await _twoFactorService.GetUserIdFromSessionAsync(request.SessionId);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Early session validation failed | sessionId = {Sid}", request.SessionId);
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid or expired session",
                        Errors = (string[]?)null,
                        StatusCode = 400
                    });
                }

                _logger.LogInformation("Session valid before verification → userId = {UserId}", userId);

                // Verify the 2FA code (this may delete the session key on success)
                var verifyResult = await _twoFactorService.VerifyCodeAsync(request.SessionId, request.Code);

                if (!verifyResult.Success)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = verifyResult.Message,
                        Errors = (string[]?)null,
                        StatusCode = 400
                    });
                }

                // Get user data using the already-fetched userId
                UserReadDto? user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found after successful 2FA | userId = {UserId}", userId);
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "User not found",
                        Errors = (string[]?)null,
                        StatusCode = 400
                    });
                }

                // Generate token
                var token = await _tokenService.GenerateTokenForAuthenticatedUserAsync(user);

                // Update user active status
                await _userService.UpdateUserActiveStatusAsync(userId, true);

                // Handle attendance after successful login completion
                var attendanceHandled = await _shiftLoginRestrictionService.HandleLoginAttendanceAsync(userId);

                _logger.LogInformation(
                    "User {UserId} successfully completed 2FA verification and logged in. Attendance handled: {AttendanceHandled}",
                    userId, attendanceHandled
                );

                var response = new LoginResponseDto
                {
                    Token = token.Token,
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserRoles = user.Roles?.ToList() ?? new List<string>()
                };

                var loginMessage = attendanceHandled
                    ? "Successfully logged in and auto clocked-in to assigned shift"
                    : "Successfully logged in";

                return Ok(new
                {
                    data = response,
                    message = loginMessage,
                    attendanceHandled = attendanceHandled
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during 2FA verification for session: {SessionId}", request?.SessionId);
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred during verification",
                    Errors = (string[]?)null,
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

                // Generate new token after password update
                var token = await _tokenService.GenerateTokenForAuthenticatedUserAsync(user);

                // Update user's online status
                await _userService.UpdateUserActiveStatusAsync(userId, true);

                // Check shift restrictions (NO ATTENDANCE YET)
                var (canLogin, restrictionReason) = await _shiftLoginRestrictionService.CanUserLoginAsync(userId);

                if (!canLogin)
                {
                    _logger.LogWarning("Login denied for user {UserId} after password update due to shift restrictions: {Reason}",
                        userId, restrictionReason);
                    return Unauthorized(new
                    {
                        message = restrictionReason,
                        errorCode = "SHIFT_RESTRICTION"
                    });
                }

                // NOW handle attendance after successful password update and login
                var attendanceHandled = await _shiftLoginRestrictionService.HandleLoginAttendanceAsync(userId);

                var response = new LoginResponseDto
                {
                    Token = token.Token,
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserRoles = user.Roles?.ToList() ?? new List<string>(),
                };

                var message = attendanceHandled
                    ? "Password updated successfully and auto clocked-in to assigned shift"
                    : "Password updated successfully";

                return Ok(new
                {
                    message = message,
                    data = response,
                    attendanceHandled = attendanceHandled
                });
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = (string[]?)null,
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

                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = (string[]?)null,
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
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred while updating password",
                    Errors = (string[]?)null,
                    StatusCode = 400
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
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
                        _logger.LogError(ex, "Error parsing token during logout");
                        return BadRequest(new { message = "Invalid token" });
                    }
                }

                // Handle logout attendance (auto clock-out for strict shifts)
                await _shiftLoginRestrictionService.HandleUserLogoutAsync(userId.Value.ToString(), DateTime.UtcNow);

                // Update user offline status
                await _userService.UpdateUserActiveStatusAsync(userId.Value.ToString(), false);

                // Delete user tokens
                var tokensDeleted = await _tokenService.DeleteAllTokensForUserAsync(userId.Value);

                // Enqueue status update
                _userStatusService.EnqueueStatusUpdate(userId.Value.ToString(), false);

                _logger.LogInformation("User {UserId} successfully logged out", userId.Value);

                if (tokensDeleted)
                {
                    return Ok(new { message = "Successfully logged out" });
                }
                else
                {
                    return Ok(new { message = "Successfully logged out (no active sessions found)" });
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

                return BadRequest(new
                {
                    Success = false,
                    Message = errorMessage,
                    Errors = (string[]?)null,
                    StatusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout");
                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    Errors = (string[]?)null,
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