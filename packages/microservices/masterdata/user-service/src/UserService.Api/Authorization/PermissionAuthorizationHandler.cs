using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using UserService.Core.Interfaces;
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
                        
                        // 3. Check for token in Authorization header
                        var authorizationToken = GetTokenFromRequest();  // Renamed token to authorizationToken
                        if (authorizationToken != null)
                        {
                            LogTokenDetails(authorizationToken);  // Use authorizationToken here
                        }
                        else
                        {
                            _logger.LogWarning("No JWT token found in the request");
                        }
                        
                        context.Fail();
                        return;
                    }

                    // 4. If we get here, user is authenticated
                    _logger.LogInformation("User is authenticated");
                    // 5. Validate permission requirement
                    if (requirement == null || string.IsNullOrWhiteSpace(requirement.Permission))
                    {
                        _logger.LogWarning("Invalid permission requirement");
                        context.Fail();
                        return;
                    }

                    // 6. Ensure the token is valid and not revoked
                    var authorizationTokenForValidation = GetTokenFromRequest();  // Renamed to avoid conflict
                    if (authorizationTokenForValidation == null || !await _tokenService.ValidateTokenAsync(authorizationTokenForValidation))
                    {
                        _logger.LogWarning("Token is either invalid or revoked.");
                        context.Fail();
                        return;
                    }

                    _logger.LogInformation("Token validated successfully.");

                    // 7. Check user permissions
                    await CheckUserPermission(context, requirement);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during permission check");
                    context.Fail();
                }
                finally
                {
                    _logger.LogInformation("=== END PERMISSION CHECK ===");
                }
            }
        }

       

        // Extract the token from the request header
        private string GetTokenFromRequest()
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

        // Log the details of the JWT token for debugging
        private void LogTokenDetails(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
            
                    // Only log token expiration in development
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        _logger.LogWarning("Token expired at {ExpirationTime}", jwtToken.ValidTo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error processing JWT token");
            }
        }

        // Check the user's permissions based on the provided requirement
        private async Task CheckUserPermission(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Get roles from claims
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!roles.Any())
            {
                _logger.LogDebug("No roles found for user");
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
                    _logger.LogDebug("Permission {Permission} granted via role", requirement.Permission);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogDebug("No role grants permission {Permission}", requirement.Permission);
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
