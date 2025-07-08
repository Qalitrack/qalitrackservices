using UserModule.Services;
namespace UserModule.Middleware
{
    public class ShiftEndLogoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ShiftEndLogoutMiddleware> _logger;
        private static DateTime _lastShiftCheck = DateTime.MinValue;
        private static readonly object _lockObject = new();
        private static readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public ShiftEndLogoutMiddleware(RequestDelegate next, ILogger<ShiftEndLogoutMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var shouldCheck = false;
            lock (_lockObject)
            {
                if (DateTime.UtcNow - _lastShiftCheck >= _checkInterval)
                {
                    _lastShiftCheck = DateTime.UtcNow;
                    shouldCheck = true;
                }
            }

            if (shouldCheck)
            {
                try
                {
                    using var scope = context.RequestServices.CreateScope();
                    var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();

                    if (await shiftService.IsStrictModeEnabledAsync())
                    {
                        await shiftService.LogoutUsersAfterShiftEndAsync();
                        _logger.LogInformation("Shift end logout check completed at {CheckTime} in strict mode", DateTime.UtcNow);
                    }
                    else
                    {
                        _logger.LogInformation("Non-strict mode, skipped shift end logout check at {CheckTime}", DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during shift end logout check");
                }
            }

            await _next(context);
        }
    }

    public static class ShiftEndLogoutMiddlewareExtensions
    {
        public static IApplicationBuilder UseShiftEndLogout(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ShiftEndLogoutMiddleware>();
        }
    }
}