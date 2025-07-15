using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using UserService.Core.Interfaces;

namespace UserService.Api.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionAuthorizationHandler(
            IUserService userService,
            ILogger<PermissionAuthorizationHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

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
                    _logger.LogInformation("=== START PERMISSION CHECK ===");
                    
                    // 1. Log all request headers
                    LogRequestHeaders(httpContext);

                    // 2. Check authentication status
                    if (context.User?.Identity?.IsAuthenticated != true)
                    {
                        _logger.LogWarning("!!! USER NOT AUTHENTICATED !!!");
                        _logger.LogWarning("Authentication Type: {AuthType}", 
                            context.User?.Identity?.AuthenticationType ?? "null");
                        
                        // 3. Check for token in Authorization header
                        var token = GetTokenFromRequest();
                        if (token != null)
                        {
                            LogTokenDetails(token);
                        }
                        else
                        {
                            _logger.LogWarning("No JWT token found in the request");
                        }
                        
                        context.Fail();
                        return;
                    }

                    // 4. If we get here, user is authenticated
                    _logger.LogInformation("User is authenticated as: {User}", 
                        context.User.Identity?.Name ?? "unknown");

                    // 5. Validate permission requirement
                    if (requirement == null || string.IsNullOrWhiteSpace(requirement.Permission))
                    {
                        _logger.LogWarning("Invalid permission requirement");
                        context.Fail();
                        return;
                    }

                    // 6. Check user permissions
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

        private void LogRequestHeaders(HttpContext httpContext)
        {
            if (httpContext?.Request.Headers != null)
            {
                _logger.LogInformation("=== REQUEST HEADERS ===");
                foreach (var header in httpContext.Request.Headers)
                {
                    _logger.LogInformation($"  {header.Key}: {header.Value}");
                }
            }
        }

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

        private void LogTokenDetails(string token)
        {
            try
            {
                _logger.LogInformation("=== JWT TOKEN FOUND ===");
                _logger.LogInformation($"Token: {token}");

                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    
                    _logger.LogInformation("=== JWT HEADER ===");
                    _logger.LogInformation(JsonSerializer.Serialize(jwtToken.Header, new JsonSerializerOptions { WriteIndented = true }));

                    _logger.LogInformation("=== JWT PAYLOAD ===");
                    _logger.LogInformation(JsonSerializer.Serialize(jwtToken.Payload, new JsonSerializerOptions { WriteIndented = true }));

                    // Check token expiration
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        _logger.LogWarning("!!! TOKEN EXPIRED !!!");
                        _logger.LogWarning($"Token expired at: {jwtToken.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
                        _logger.LogWarning($"Current time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                    }
                    else
                    {
                        _logger.LogInformation("Token is valid");
                        _logger.LogInformation($"Expires at: {jwtToken.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
                    }
                }
                else
                {
                    _logger.LogWarning("Token is not a valid JWT");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading JWT token");
            }
        }

        private async Task CheckUserPermission(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var claims = context.User.Claims?.ToList() ?? new List<Claim>();
            
            // Get user ID
            var userId = GetUserIdFromClaims(context.User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims. Available claim types: {ClaimTypes}",
                    string.Join(", ", claims.Select(c => c.Type).Distinct()));
                context.Fail();
                return;
            }

            _logger.LogInformation("Checking permission for user {UserId}", userId);

            try
            {
                // Get user permissions
                var userPermissions = await _userService.GetUserPermissionsAsync(userId);
                if (userPermissions == null)
                {
                    _logger.LogWarning("No permissions found for user {UserId}", userId);
                    context.Fail();
                    return;
                }

                // Process permissions
                var permissionNames = (userPermissions as IEnumerable<object> ?? Enumerable.Empty<object>())
                    .Select(p => GetPermissionName(p))
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                _logger.LogInformation("User {UserId} has {Count} permissions: {Permissions}",
                    userId,
                    permissionNames.Count,
                    string.Join(", ", permissionNames));

                // Check permission
                var hasPermission = permissionNames.Any(p => 
                    string.Equals(p, requirement.Permission, StringComparison.OrdinalIgnoreCase));

                if (hasPermission)
                {
                    _logger.LogInformation("Permission granted for user {UserId}", userId);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("Permission denied. User {UserId} lacks permission: {Permission}",
                        userId, requirement.Permission);
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions for user {UserId}", userId);
                context.Fail();
            }
        }

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