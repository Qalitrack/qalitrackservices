using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Api.Authorization
{
    public class PermissionAuthorizationHandler(
        IUserService userService,
        ILogger<PermissionAuthorizationHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService)
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly ILogger<PermissionAuthorizationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService)); // Inject TokenService
        
        // Injected Token Service for validation

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var requestId = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["RequestId"] = requestId,
                ["Permission"] = requirement?.Permission ?? "null",
                ["Path"] = httpContext?.Request.Path.ToString() ?? "unknown"
            }))
            {
                try
                {
                    // 2. Check authentication status
                    if (context.User?.Identity?.IsAuthenticated != true)
                    { 
                        context.Fail();
                        return;
                    }

                  // 5. Validate permission requirement
                    if (requirement == null || string.IsNullOrWhiteSpace(requirement.Permission))
                    {
                        context.Fail();
                        return;
                    }

                    // 6. Ensure the token is valid and not revoked
                    var authorizationTokenForValidation = GetTokenFromRequest();  // Renamed to avoid conflict
                    if (authorizationTokenForValidation == null || !await _tokenService.ValidateTokenAsync(authorizationTokenForValidation))
                    {
                        context.Fail();
                        return;
                    }


                    await CheckUserPermission(context, requirement);
                }
                catch (Exception)
                {
                    context.Fail();
                }
                finally
                {
                    _logger.LogInformation("=== END PERMISSION CHECK ===");
                }
            }
        }

       

        // Extract the token from the request header
        private string? GetTokenFromRequest()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.Request.Headers.TryGetValue("Authorization", out var authHeader) == true)
                {
                    var authHeaderValue = authHeader.FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeaderValue) && 
                        authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return authHeaderValue.Substring("Bearer ".Length).Trim();
                    }
                    return authHeaderValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting token from request");
            }
            return null;
        }

        private async Task CheckUserPermission(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Get roles from claims
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!roles.Any())
            {
                context.Fail();
                return;
            }

            try
            {
                // Get permissions for all roles
                var rolePermissions = new List<string>();
                foreach (var role in roles)
                {
                    var permissions = await _userService.GetPermissionsForRoleAsync(role);
                    rolePermissions.AddRange(permissions);
                }

                // Check if any role has the required permission
                var hasPermission = rolePermissions.Any(p => 
                    string.Equals(p, requirement.Permission, StringComparison.OrdinalIgnoreCase));

                if (hasPermission)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role permissions");
                context.Fail();
            }
        }
        
    }
}
