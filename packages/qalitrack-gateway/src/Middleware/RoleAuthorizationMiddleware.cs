using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.Json;

namespace QaliTrackGateway.Middleware;

/// <summary>
/// Middleware for enforcing role-based authorization at the gateway level.
/// Implements the hybrid authorization model - coarse-grained service access control.
/// </summary>
public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RoleAuthorizationMiddleware> _logger;
    private readonly Dictionary<string, int> _roleHierarchy;
    private readonly Dictionary<string, RouteRoleRequirement> _routeRoleRequirements;

    public RoleAuthorizationMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RoleAuthorizationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        
        // Initialize role hierarchy (higher number = higher privileges)
        _roleHierarchy = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
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

        // Extract and validate user roles
        var userRoles = GetUserRoles(context.User);
        var hasRequiredRole = await CheckRoleAccess(userRoles, roleRequirement, context.User);

        if (!hasRequiredRole)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogWarning(
                "Access denied for user {UserId} to {Path}. Required: {RequiredRole}, User roles: {UserRoles}",
                userId, path, string.Join(",", roleRequirement.RequiredRoles), string.Join(",", userRoles));
            
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden: Insufficient role privileges");
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
        return user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    private async Task<bool> CheckRoleAccess(
        List<string> userRoles, 
        RouteRoleRequirement requirement, 
        ClaimsPrincipal user)
    {
        if (!requirement.RequiredRoles.Any())
        {
            return true; // No specific role required
        }

        // Get the highest role level the user has
        var validUserRoles = userRoles.Where(role => _roleHierarchy.ContainsKey(role)).ToList();
        
        // If user has no valid roles, deny access
        if (!validUserRoles.Any())
        {
            return false;
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
        
        _logger.LogInformation(
            "Role authorization check - User: {UserId} ({UserName}), Service: {ServiceName}, " +
            "Required: {RequiredRoles}, User Level: {UserLevel}, Result: {Authorized}",
            userId, userName, requirement.ServiceName,
            string.Join(",", requirement.RequiredRoles), userHighestLevel, hasRequiredLevel);

        return hasRequiredLevel;
    }

    private void AddUserContextHeaders(HttpContext context, RouteRoleRequirement requirement)
    {
        var user = context.User;
        
        // Add user context headers for downstream services
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        var userRoles = GetUserRoles(user);

        if (!string.IsNullOrEmpty(userId))
            context.Request.Headers["X-User-ID"] = userId;
        
        if (!string.IsNullOrEmpty(userName))
            context.Request.Headers["X-User-Name"] = userName;
        
        if (!string.IsNullOrEmpty(userEmail))
            context.Request.Headers["X-User-Email"] = userEmail;
        
        if (userRoles.Any())
            context.Request.Headers["X-User-Roles"] = string.Join(",", userRoles);

        // Add service context
        context.Request.Headers["X-Service-Name"] = requirement.ServiceName;
        context.Request.Headers["X-Gateway-Authorized"] = "true";
        
        _logger.LogDebug(
            "Added user context headers for {ServiceName}: User={UserId}, Roles={Roles}",
            requirement.ServiceName, userId, string.Join(",", userRoles));
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

                    if (requiredRoles.Any())
                    {
                        var requirement = new RouteRoleRequirement
                        {
                            PathPattern = upstreamPath,
                            RequiredRoles = requiredRoles,
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
/// Represents role requirements for a specific route
/// </summary>
public class RouteRoleRequirement
{
    public string PathPattern { get; set; } = string.Empty;
    public List<string> RequiredRoles { get; set; } = new();
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}