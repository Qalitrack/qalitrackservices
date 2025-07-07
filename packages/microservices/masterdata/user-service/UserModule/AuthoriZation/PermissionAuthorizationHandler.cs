using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using UserModule.Services;

namespace UserModule.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await _permissionService.UserHasPermissionAsync(userId, requirement.Permission);
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}