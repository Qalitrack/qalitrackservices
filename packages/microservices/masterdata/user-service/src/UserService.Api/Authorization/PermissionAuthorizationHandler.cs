using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using UserService.Core.Interfaces;

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
                        // Authentication check failed - proceeding to token validation
                        
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

        // Log request headers for debugging
        private void LogRequestHeaders(HttpContext httpContext)
        {
            if (httpContext?.Request.Headers != null)
            {
                foreach (var header in httpContext.Request.Headers)
                {
                    _logger.LogInformation($"  {header.Key}: {header.Value}");
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
            var userId = GetUserIdFromClaims(context.User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("User ID not found in claims");
                context.Fail();
                return;
            }

            try
            {
                var userPermissions = await _userService.GetUserPermissionsAsync(userId);
                if (userPermissions == null)
                {
                    _logger.LogWarning("No permissions found for user {UserId}", userId);
                    context.Fail();
                    return;
                }

                var permissionNames = (userPermissions as IEnumerable<object> ?? Enumerable.Empty<object>())
                    .Select(p => GetPermissionName(p))
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                var hasPermission = permissionNames.Any(p => 
                    string.Equals(p, requirement.Permission, StringComparison.OrdinalIgnoreCase));

                if (hasPermission)
                {
                    _logger.LogDebug("Permission {Permission} granted for user {UserId}", 
                        requirement.Permission, userId);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogDebug("Permission {Permission} denied for user {UserId}", 
                        requirement.Permission, userId);
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions for user {UserId}", userId);
                context.Fail();
            }
        }

        // Extract UserId from claims
        private static string GetUserIdFromClaims(ClaimsPrincipal user)
        {
            if (user == null) return null;

            var claimTypes = new[]
            {
                JwtRegisteredClaimNames.Sub,
                ClaimTypes.NameIdentifier,
                "sub",
                "nameid",
                ClaimTypes.Upn,
                ClaimTypes.Email,
                "user_id",
                "uid",
                "id"
            };

            foreach (var claimType in claimTypes)
            {
                var claim = user.FindFirst(claimType);
                if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    return claim.Value;
                }
            }

            return null;
        }

        // Get permission name from permission object
        private static string GetPermissionName(object permission)
        {
            if (permission == null) return string.Empty;
            if (permission is string str) return str;

            try
            {
                var type = permission.GetType();
                var property = type.GetProperty("Name") ?? 
                             type.GetProperty("PermissionName") ??
                             type.GetProperty("Code") ??
                             type.GetProperty("Value");

                return property?.GetValue(permission)?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
