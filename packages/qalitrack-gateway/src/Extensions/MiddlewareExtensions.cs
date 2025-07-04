using QaliTrackGateway.Middleware;

namespace QaliTrackGateway.Extensions;

/// <summary>
/// Extension methods for configuring middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds role-based authorization middleware to the pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RoleAuthorizationMiddleware>();
    }
}