using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Transaction.Api.Middleware;

/// <summary>
/// Middleware to prevent sensitive data leaks in API responses and errors
/// </summary>
public class DataLeakPreventionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DataLeakPreventionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    // List of sensitive field patterns to sanitize
    private static readonly string[] SensitiveFieldPatterns = new[]
    {
        "password",
        "pwd",
        "secret",
        "token",
        "apikey",
        "api_key",
        "connectionstring",
        "ssn",
        "socialsecurity",
        "creditcard",
        "cardnumber",
        "cvv",
        "pin",
        "private",
        "encryption",
        "salt",
        "hash"
    };

    // Paths to internal database information to mask
    private static readonly string[] InternalPathPatterns = new[]
    {
        "/var/",
        "/home/",
        "/usr/",
        "C:\\",
        "D:\\",
        "Program Files"
    };

    public DataLeakPreventionMiddleware(
        RequestDelegate next,
        ILogger<DataLeakPreventionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Execute the next middleware
            await _next(context);

            // Only process responses with content
            if (context.Response.StatusCode >= 400 && responseBody.Length > 0)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                // Sanitize the response
                var sanitizedResponse = SanitizeResponse(responseText, context.Response.StatusCode);

                // Write sanitized response
                responseBody.SetLength(0);
                await using (var writer = new StreamWriter(responseBody, leaveOpen: true))
                {
                    await writer.WriteAsync(sanitizedResponse);
                    await writer.FlushAsync();
                }
            }

            // Copy sanitized response to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            // Log the original exception with full details
            _logger.LogError(ex, "Unhandled exception occurred during request {Path}", context.Request.Path);

            // Return sanitized error response
            await HandleExceptionAsync(context, ex);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private string SanitizeResponse(string response, int statusCode)
    {
        if (string.IsNullOrEmpty(response))
            return response;

        try
        {
            // Parse JSON if possible
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;

            // Create sanitized object
            var sanitized = SanitizeJsonElement(root);

            return JsonSerializer.Serialize(sanitized, new JsonSerializerOptions
            {
                WriteIndented = !_environment.IsProduction(),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            // If not JSON, sanitize as text
            return SanitizeText(response);
        }
    }

    private object? SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var property in element.EnumerateObject())
                {
                    var key = property.Name;
                    var value = property.Value;

                    // Check if property name is sensitive
                    if (IsSensitiveField(key))
                    {
                        dict[key] = "***REDACTED***";
                    }
                    else if (key.Equals("StackTrace", StringComparison.OrdinalIgnoreCase))
                    {
                        // Sanitize stack traces in production
                        dict[key] = _environment.IsProduction()
                            ? "Stack trace hidden in production"
                            : SanitizeStackTrace(value.GetString());
                    }
                    else if (key.Equals("Message", StringComparison.OrdinalIgnoreCase) ||
                             key.Equals("ExceptionMessage", StringComparison.OrdinalIgnoreCase))
                    {
                        dict[key] = SanitizeErrorMessage(value.GetString());
                    }
                    else
                    {
                        dict[key] = SanitizeJsonElement(value);
                    }
                }
                return dict;

            case JsonValueKind.Array:
                return element.EnumerateArray().Select(SanitizeJsonElement).ToList();

            case JsonValueKind.String:
                return SanitizeText(element.GetString() ?? string.Empty);

            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                return GetPrimitiveValue(element);

            default:
                return null;
        }
    }

    private static object? GetPrimitiveValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetString()
        };
    }

    private static bool IsSensitiveField(string fieldName)
    {
        return SensitiveFieldPatterns.Any(pattern =>
            fieldName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private string SanitizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove internal file paths
        foreach (var pattern in InternalPathPatterns)
        {
            text = text.Replace(pattern, "[REDACTED_PATH]");
        }

        // Remove database connection strings
        text = Regex.Replace(text,
            @"(Server|Data Source|Initial Catalog|User ID|Password|Uid|Pwd)=([^;]+)",
            "$1=***REDACTED***",
            RegexOptions.IgnoreCase);

        // Remove IP addresses in production
        if (_environment.IsProduction())
        {
            text = Regex.Replace(text,
                @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
                "[IP_REDACTED]");
        }

        // Remove potential API keys/tokens (long alphanumeric strings)
        text = Regex.Replace(text,
            @"\b[A-Za-z0-9]{32,}\b",
            "[TOKEN_REDACTED]");

        return text;
    }

    private string? SanitizeStackTrace(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return stackTrace;

        // In production, completely hide stack trace
        if (_environment.IsProduction())
            return "Stack trace hidden in production environment";

        // In development, sanitize paths but keep the trace
        return SanitizeText(stackTrace);
    }

    private string? SanitizeErrorMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        // Sanitize internal details but keep useful information
        var sanitized = SanitizeText(message);

        // In production, make messages more generic for internal errors
        if (_environment.IsProduction())
        {
            // List of technical terms to hide in production
            var technicalTerms = new[]
            {
                "NullReferenceException",
                "IndexOutOfRangeException",
                "ArgumentNullException",
                "InvalidCastException",
                "DbUpdateException",
                "SqlException"
            };

            foreach (var term in technicalTerms)
            {
                if (sanitized.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    return "An internal error occurred. Please contact support if the issue persists.";
                }
            }
        }

        return sanitized;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        object response;

        // In development, include sanitized stack trace
        if (_environment.IsDevelopment())
        {
            response = new
            {
                error = "An error occurred while processing your request.",
                message = SanitizeErrorMessage(exception.Message),
                statusCode = 500,
                traceId = context.TraceIdentifier,
                exceptionType = exception.GetType().Name,
                stackTrace = SanitizeStackTrace(exception.StackTrace)
            };
        }
        else
        {
            response = new
            {
                error = "An error occurred while processing your request.",
                message = "Please contact support if the issue persists.",
                statusCode = 500,
                traceId = context.TraceIdentifier
            };
        }

        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class DataLeakPreventionMiddlewareExtensions
{
    public static IApplicationBuilder UseDataLeakPrevention(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DataLeakPreventionMiddleware>();
    }
}
