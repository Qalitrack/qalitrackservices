using Serilog.Context;

namespace TechnicianApi.Api.Middleware;

public class RequestLoggingSanitizerMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "X-API-Key",
        "X-Auth-Token",
        "Authentication"
    };

    private static readonly HashSet<string> SensitiveQueryParams = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "token",
        "apikey",
        "api_key",
        "secret",
        "key"
    };

    public RequestLoggingSanitizerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Sanitize headers for logging
        var sanitizedHeaders = SanitizeHeaders(context.Request.Headers);

        // Sanitize query parameters for logging
        var sanitizedQuery = SanitizeQueryString(context.Request.QueryString.Value);

        // Push sanitized data to Serilog context
        using (LogContext.PushProperty("RequestHeaders", sanitizedHeaders))
        using (LogContext.PushProperty("RequestQuery", sanitizedQuery))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            await _next(context);
        }
    }

    private static Dictionary<string, string> SanitizeHeaders(IHeaderDictionary headers)
    {
        var sanitized = new Dictionary<string, string>();

        foreach (var header in headers)
        {
            if (SensitiveHeaders.Contains(header.Key))
            {
                sanitized[header.Key] = "***REDACTED***";
            }
            else
            {
                sanitized[header.Key] = header.Value.ToString();
            }
        }

        return sanitized;
    }

    private static string? SanitizeQueryString(string? queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
            return queryString;

        var sanitized = queryString;

        foreach (var param in SensitiveQueryParams)
        {
            // Simple pattern matching for query parameters
            var pattern = $"{param}=[^&]*";
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                pattern,
                $"{param}=***REDACTED***",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return sanitized;
    }
}
