using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.Json;
using QaliTrackGateway.Services;

namespace QaliTrackGateway.Middleware;

/// <summary>
/// Middleware for enforcing role-based authorization at the gateway level.
/// Implements the hybrid authorization model - coarse-grained service access control.
/// </summary>
public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly IAuthorizationCacheService _cacheService;
    private readonly ILogger<RoleAuthorizationMiddleware> _logger;
    private readonly Dictionary<string, int> _roleHierarchy;
    private readonly Dictionary<string, RouteRoleRequirement> _routeRoleRequirements;

    public RoleAuthorizationMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        IAuthorizationCacheService cacheService,
        ILogger<RoleAuthorizationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _cache = cache;
        _cacheService = cacheService;
        _logger = logger;
        
        // Initialize role hierarchy (higher number = higher privileges)
        _roleHierarchy = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Guest"] = 0,
            ["User"] = 1,
            ["Operator"] = 2,
            ["SiteManager"] = 3,
            ["Admin"] = 4,
            ["SuperAdmin"] = 5
        };

        // Load route role requirements from Ocelot configuration
        _routeRoleRequirements = LoadRouteRoleRequirements(configuration);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        
        // Skip authorization for public endpoints
        if (IsPublicEndpoint(path))
        {
            _logger.LogDebug("Allowing access to public endpoint: {Path}", path);
            await _next(context);
            return;
        }

        // Check if the route requires role-based authorization
        var roleRequirement = GetRouteRoleRequirement(path);
        if (roleRequirement == null)
        {
            _logger.LogDebug("No role requirement found for path: {Path}", path);
            await _next(context);
            return;
        }

        // Validate authentication
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthorized access attempt to protected endpoint: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Authentication required");
            return;
        }

        // Extract user roles and permissions
        var userRoles = GetUserRoles(context.User);
        var userPermissions = GetUserPermissions(context.User);
        
        // Check hybrid authorization (both roles and permissions must be satisfied)
        var hasRequiredAccess = await CheckHybridAccess(userRoles, userPermissions, roleRequirement, context.User);

        if (!hasRequiredAccess)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogWarning(
                "Access denied for user {UserId} to {Path}. Required roles: {RequiredRoles}, Required permissions: {RequiredPermissions}, " +
                "User roles: {UserRoles}, User permissions: {UserPermissions}",
                userId, path, 
                string.Join(",", roleRequirement.RequiredRoles), string.Join(",", roleRequirement.RequiredPermissions),
                string.Join(",", userRoles), string.Join(",", userPermissions));
            
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden: Insufficient role or permission privileges");
            return;
        }

        // Add user context headers for downstream services
        AddUserContextHeaders(context, roleRequirement);

        _logger.LogDebug(
            "Authorized access for user {UserId} to {ServiceName} ({Path})",
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            roleRequirement.ServiceName,
            path);

        await _next(context);
    }

    private bool IsPublicEndpoint(string path)
    {
        var publicEndpoints = new[]
        {
            "/api/auth/",
            "/health",
            "/services/",
            "/api/swagger",
            "/swagger"
        };

        return publicEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    private RouteRoleRequirement? GetRouteRoleRequirement(string path)
    {
        // Try to get from cache first
        var cacheKey = $"route_role_{path}";
        if (_cache.TryGetValue(cacheKey, out RouteRoleRequirement? cached))
        {
            return cached;
        }

        // Find matching route requirement
        var requirement = _routeRoleRequirements.Values
            .FirstOrDefault(r => PathMatches(path, r.PathPattern));

        if (requirement != null)
        {
            // Cache for 5 minutes
            _cache.Set(cacheKey, requirement, TimeSpan.FromMinutes(5));
        }

        return requirement;
    }

    private bool PathMatches(string requestPath, string pattern)
    {
        // Convert Ocelot pattern to regex-like matching
        // /api/vehicles/{everything} matches /api/vehicles/*
        var normalizedPattern = pattern
            .Replace("{everything}", "*")
            .ToLowerInvariant();

        if (normalizedPattern.EndsWith("*"))
        {
            var prefix = normalizedPattern[..^1]; // Remove the *
            return requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(requestPath, normalizedPattern, StringComparison.OrdinalIgnoreCase);
    }

    private List<string> GetUserRoles(ClaimsPrincipal user)
    {
        // Get roles from both role claim and roles claim for hybrid support
        var roleClaims = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var rolesClaim = user.FindFirst("roles")?.Value ?? "";
        
        if (!string.IsNullOrEmpty(rolesClaim))
        {
            var rolesFromClaim = rolesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim()).ToList();
            roleClaims.AddRange(rolesFromClaim);
        }
        
        return roleClaims.Distinct().ToList();
    }

    private List<string> GetUserPermissions(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        
        // Try to get from cache first using the cache service
        var cachedPermissions = _cacheService.GetUserPermissions(userId);
        if (cachedPermissions != null)
        {
            return cachedPermissions;
        }
        
        var permissionsClaim = user.FindFirst("permissions")?.Value ?? "";
        if (string.IsNullOrEmpty(permissionsClaim))
        {
            return new List<string>();
        }
        
        var permissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToList();
            
        // Cache permissions for 10 minutes to reduce JWT parsing overhead
        _cacheService.SetUserPermissions(userId, permissions, TimeSpan.FromMinutes(10));
        
        return permissions;
    }

    private async Task<bool> CheckHybridAccess(
        List<string> userRoles,
        List<string> userPermissions, 
        RouteRoleRequirement requirement, 
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        
        // Check if we have a cached authorization decision using the cache service
        var cachedDecision = _cacheService.GetAuthorizationDecision(userId, requirement.PathPattern);
        if (cachedDecision.HasValue)
        {
            return cachedDecision.Value;
        }
        
        // 1. Check role-based access (existing logic - maintained for backward compatibility)
        var roleAccess = await CheckRoleAccess(userRoles, requirement, user);
        
        // 2. Check permission-based access (new logic)
        var permissionAccess = CheckPermissionAccess(userPermissions, requirement, user);
        
        // 3. For hybrid authorization: both role AND permission must be satisfied
        // If no permissions are required, only check roles (backward compatibility)
        bool authDecision;
        if (!requirement.RequiredPermissions.Any())
        {
            authDecision = roleAccess;
        }
        else
        {
            // Both role and permission requirements must be met
            authDecision = roleAccess && permissionAccess;
        }
        
        // Cache the authorization decision for 5 minutes to improve performance
        _cacheService.SetAuthorizationDecision(userId, requirement.PathPattern, authDecision, TimeSpan.FromMinutes(5));
        
        return authDecision;
    }

    private Task<bool> CheckRoleAccess(
        List<string> userRoles, 
        RouteRoleRequirement requirement, 
        ClaimsPrincipal user)
    {
        if (!requirement.RequiredRoles.Any())
        {
            return Task.FromResult(true); // No specific role required
        }

        // Get the highest role level the user has
        var validUserRoles = userRoles.Where(role => _roleHierarchy.ContainsKey(role)).ToList();
        
        // If user has no valid roles, deny access
        if (!validUserRoles.Any())
        {
            return Task.FromResult(false);
        }
        
        var userHighestLevel = validUserRoles.Max(role => _roleHierarchy[role]);

        // Check if user meets any of the required role levels
        var hasRequiredLevel = requirement.RequiredRoles.Any(requiredRole =>
        {
            var requiredLevel = _roleHierarchy.GetValueOrDefault(requiredRole, int.MaxValue);
            return userHighestLevel >= requiredLevel;
        });

        // Log authorization decision for audit
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        
        _logger.LogDebug(
            "Role authorization check - User: {UserId} ({UserName}), Service: {ServiceName}, " +
            "Required: {RequiredRoles}, User Level: {UserLevel}, Result: {Authorized}",
            userId, userName, requirement.ServiceName,
            string.Join(",", requirement.RequiredRoles), userHighestLevel, hasRequiredLevel);

        return Task.FromResult(hasRequiredLevel);
    }

    private bool CheckPermissionAccess(
        List<string> userPermissions,
        RouteRoleRequirement requirement,
        ClaimsPrincipal user)
    {
        if (!requirement.RequiredPermissions.Any())
        {
            return true; // No specific permissions required
        }

        // Check if user has any of the required permissions
        var hasRequiredPermission = requirement.RequiredPermissions.Any(requiredPerm =>
            userPermissions.Contains(requiredPerm, StringComparer.OrdinalIgnoreCase));

        // Log permission check for audit
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        
        _logger.LogDebug(
            "Permission authorization check - User: {UserId} ({UserName}), Service: {ServiceName}, " +
            "Required: {RequiredPermissions}, User Permissions: {UserPermissions}, Result: {Authorized}",
            userId, userName, requirement.ServiceName,
            string.Join(",", requirement.RequiredPermissions), 
            string.Join(",", userPermissions), hasRequiredPermission);

        return hasRequiredPermission;
    }


    private void AddUserContextHeaders(HttpContext context, RouteRoleRequirement requirement)
    {
        var user = context.User;
        
        // Add user context headers for downstream services
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        var userRoles = GetUserRoles(user);
        var userPermissions = GetUserPermissions(user);

        if (!string.IsNullOrEmpty(userId))
            context.Request.Headers["X-User-ID"] = userId;
        
        if (!string.IsNullOrEmpty(userName))
            context.Request.Headers["X-User-Name"] = userName;
        
        if (!string.IsNullOrEmpty(userEmail))
            context.Request.Headers["X-User-Email"] = userEmail;
        
        if (userRoles.Any())
            context.Request.Headers["X-User-Roles"] = string.Join(",", userRoles);
        
        if (userPermissions.Any())
            context.Request.Headers["X-User-Permissions"] = string.Join(",", userPermissions);

        // Add service context
        context.Request.Headers["X-Service-Name"] = requirement.ServiceName;
        context.Request.Headers["X-Gateway-Authorized"] = "true";
        
        _logger.LogDebug(
            "Added user context headers for {ServiceName}: User={UserId}, Roles={Roles}, Permissions={Permissions}",
            requirement.ServiceName, userId, string.Join(",", userRoles), string.Join(",", userPermissions));
    }

    private Dictionary<string, RouteRoleRequirement> LoadRouteRoleRequirements(IConfiguration configuration)
    {
        var requirements = new Dictionary<string, RouteRoleRequirement>();

        try
        {
            // Load from Ocelot configuration
            var ocelotConfig = configuration.GetSection("Routes");
            
            foreach (var route in ocelotConfig.GetChildren())
            {
                var upstreamPath = route["UpstreamPathTemplate"];
                var metadata = route.GetSection("Metadata");
                
                if (!string.IsNullOrEmpty(upstreamPath) && metadata.Exists())
                {
                    var requiredRoles = metadata.GetSection("RequiredRoles")
                        .GetChildren()
                        .Select(x => x.Value ?? string.Empty)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    var requiredPermissions = metadata.GetSection("RequiredPermissions")
                        .GetChildren()
                        .Select(x => x.Value ?? string.Empty)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    if (requiredRoles.Any() || requiredPermissions.Any())
                    {
                        var requirement = new RouteRoleRequirement
                        {
                            PathPattern = upstreamPath,
                            RequiredRoles = requiredRoles,
                            RequiredPermissions = requiredPermissions,
                            ServiceName = metadata["ServiceName"] ?? "Unknown",
                            Description = metadata["Description"] ?? string.Empty
                        };

                        requirements[upstreamPath] = requirement;
                    }
                }
            }

            _logger.LogInformation(
                "Loaded {Count} route role requirements from configuration", 
                requirements.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load route role requirements from configuration");
        }

        return requirements;
    }
}

/// <summary>
/// Represents role and permission requirements for a specific route
/// </summary>
public class RouteRoleRequirement
{
    public string PathPattern { get; set; } = string.Empty;
    public List<string> RequiredRoles { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();  // New: permission requirements
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}