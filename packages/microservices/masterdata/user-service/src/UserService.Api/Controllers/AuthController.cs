using Microsoft.AspNetCore.Mvc;
using UserService.Core.Interfaces;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;
using System.Linq;
using Serilog;

namespace UserService.Api.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly ITokenRepository _tokenRepository;

    public AuthController(
        ITokenService tokenService, 
        IUserService userService,
        ITokenRepository tokenRepository)
    {
        _tokenService = tokenService;
        _userService = userService;
        _tokenRepository = tokenRepository;
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

            if (user.Status != UserStatus.Active)
            {
                return Unauthorized(new { message = "Account is not active" });
            }

            var personalAccessToken = await _tokenService.GenerateTokenAsync(loginDto.Email, loginDto.Password);

            var response = new LoginResponseDto
            {
                Token = personalAccessToken.Token,
                Id = user.Id.ToString(),
                Role = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? "User",
            };

            return Ok(response);

        }
        catch (Exception ex)
        {
            Log.Error("An error occurred during login: " + ex.Message);
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
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
