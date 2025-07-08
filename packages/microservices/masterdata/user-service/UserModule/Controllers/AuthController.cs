using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserModule.Services;
using System.IdentityModel.Tokens.Jwt;
using UserModule.Authorization;
using UserModule.Dtos.Permissions;

namespace UserModule.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize(AuthenticationSchemes = "Sanctum")]
public class AuthController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public AuthController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost("validate-permission")]
    [RequirePermission("auth.validate-permission")]
    public async Task<ActionResult<bool>> ValidatePermission([FromBody] ValidatePermissionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Permission))
        {
            return BadRequest("Permission name is required.");
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized("Invalid user ID in token.");
        }

        var hasPermission = await _permissionService.UserHasPermissionAsync(userId, dto.Permission);
        return Ok(hasPermission);
    }
}