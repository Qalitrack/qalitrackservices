
namespace UserModule.Middleware
{
    public class SanctumAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SanctumAuthenticationMiddleware> _logger;

        public SanctumAuthenticationMiddleware(RequestDelegate next, ILogger<SanctumAuthenticationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Proceed to next middleware (including UseAuthentication and UseAuthorization)
            await _next(context);

            // Post-authentication logic: Log if the user is authenticated for /api/users endpoints
            if (context.Request.Path.StartsWithSegments("/api/users") &&
                !context.Request.Path.Value.Contains("/login") &&
                !context.Request.Path.Value.Contains("/first-login-password"))
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var userId = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                    _logger.LogInformation("User {UserId} authenticated for path: {Path}", userId ?? "unknown", context.Request.Path);
                }
                else
                {
                    _logger.LogWarning("Unauthenticated request to path: {Path}", context.Request.Path);
                }
            }
        }
    }

    public static class SanctumAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSanctumAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SanctumAuthenticationMiddleware>();
        }
    }
}