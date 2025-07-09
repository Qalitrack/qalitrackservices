using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MockUserService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MockUserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MockAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MockAuthController> _logger;

    public MockAuthController(IConfiguration configuration, ILogger<MockAuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("mock-login")]
    public IActionResult MockLogin([FromBody] MockLoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Mock login request for user: {request.Username}, role: {request.Role}");

            // Validate role
            if (!MockRoles.AllRoles.Contains(request.Role))
            {
                return BadRequest($"Invalid role. Allowed roles: {string.Join(", ", MockRoles.AllRoles)}");
            }

            // Generate user info with roles hierarchy and permissions
            var user = new UserInfo
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = $"{request.Username}@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = request.Role,  // Primary role (backward compatibility)
                Roles = MockRoles.GetRolesHierarchy(request.Role),  // All roles in hierarchy
                Permissions = MockRoles.GetPermissions(request.Role)  // All permissions for the role
            };

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(1);

            var response = new MockLoginResponse
            {
                Token = token,
                Role = request.Role,  // Primary role (backward compatibility)
                Roles = user.Roles,  // All roles in hierarchy
                Username = request.Username,
                ExpiresAt = expiresAt,
                User = user
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mock login");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "ProductionSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "MockUserService",
                ValidAudience = _configuration["Jwt:Audience"] ?? "MockUserService",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out var validatedToken);
            
            var username = principal.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var rolesClaim = principal.FindFirst("roles")?.Value ?? "";
            var roles = string.IsNullOrEmpty(rolesClaim) ? new List<string>() : rolesClaim.Split(',').ToList();
            var permissionsClaim = principal.FindFirst("permissions")?.Value ?? "";
            var permissions = string.IsNullOrEmpty(permissionsClaim) ? new List<string>() : permissionsClaim.Split(',').ToList();

            var jwtToken = validatedToken as JwtSecurityToken;
            var expiresAt = jwtToken?.ValidTo ?? DateTime.UtcNow;

            var response = new ValidateTokenResponse
            {
                IsValid = true,
                Username = username,
                Role = role,  // Primary role (backward compatibility)
                Roles = roles,  // All roles in hierarchy
                Permissions = permissions,
                ExpiresAt = expiresAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Ok(new ValidateTokenResponse { IsValid = false });
        }
    }

    [HttpGet("roles")]
    public IActionResult GetAvailableRoles()
    {
        var roles = MockRoles.AllRoles.Select(role => new
        {
            Role = role,
            Permissions = MockRoles.GetPermissions(role)
        });

        return Ok(roles);
    }

    [HttpGet("test-protected")]
    [Authorize]
    public IActionResult TestProtectedEndpoint()
    {
        var user = HttpContext.User;
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "This is a protected endpoint",
            Username = username,
            Role = role,
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false
        });
    }

    private string GenerateJwtToken(UserInfo user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "ProductionSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!");
        var issuer = _configuration["Jwt:Issuer"] ?? "MockUserService";
        var audience = _configuration["Jwt:Audience"] ?? "MockUserService";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),  // Primary role (backward compatibility)
            new Claim("roles", string.Join(",", user.Roles)),  // All roles in hierarchy
            new Claim("permissions", string.Join(",", user.Permissions)),  // All permissions
            new Claim("first_name", user.FirstName),
            new Claim("last_name", user.LastName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}