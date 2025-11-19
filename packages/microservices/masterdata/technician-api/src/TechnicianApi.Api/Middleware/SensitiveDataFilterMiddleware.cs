using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TechnicianApi.Api.Middleware;

public class SensitiveDataFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SensitiveDataFilterMiddleware> _logger;

    private static readonly HashSet<string> SensitiveFieldPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "secret",
        "token",
        "apikey",
        "api_key",
        "connectionstring",
        "connection_string",
        "privatekey",
        "private_key",
        "ssn",
        "creditcard",
        "credit_card",
        "cvv",
        "pin"
    };

    public SensitiveDataFilterMiddleware(
        RequestDelegate next,
        ILogger<SensitiveDataFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        if (context.Response.ContentType?.Contains("application/json") == true)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (!string.IsNullOrWhiteSpace(responseText))
            {
                var sanitized = SanitizeResponse(responseText);

                var sanitizedBytes = Encoding.UTF8.GetBytes(sanitized);
                context.Response.Body = originalBodyStream;
                context.Response.ContentLength = sanitizedBytes.Length;
                await context.Response.Body.WriteAsync(sanitizedBytes);
                return;
            }
        }

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private string SanitizeResponse(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText);
            var sanitized = SanitizeJsonElement(document.RootElement);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            return JsonSerializer.Serialize(sanitized, options);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse response as JSON for sanitization");
            return responseText;
        }
    }

    private object? SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object?>();
                foreach (var property in element.EnumerateObject())
                {
                    if (IsSensitiveField(property.Name))
                    {
                        obj[property.Name] = "***REDACTED***";
                    }
                    else
                    {
                        obj[property.Name] = SanitizeJsonElement(property.Value);
                    }
                }
                return obj;

            case JsonValueKind.Array:
                var array = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(SanitizeJsonElement(item));
                }
                return array;

            case JsonValueKind.String:
                var stringValue = element.GetString();
                return SanitizePotentialConnectionString(stringValue);

            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intValue))
                    return intValue;
                if (element.TryGetInt64(out var longValue))
                    return longValue;
                return element.GetDecimal();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return element.ToString();
        }
    }

    private static bool IsSensitiveField(string fieldName)
    {
        return SensitiveFieldPatterns.Any(pattern =>
            fieldName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static string? SanitizePotentialConnectionString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // Check for connection string patterns
        if (Regex.IsMatch(value, @"(server|data source|initial catalog|user id|password|pwd)=", RegexOptions.IgnoreCase))
        {
            return "***REDACTED CONNECTION STRING***";
        }

        // Check for SQL Server error messages that leak database info
        if (value.Contains("SqlException", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("at System.Data", StringComparison.OrdinalIgnoreCase))
        {
            return "A database error occurred. Please contact support.";
        }

        return value;
    }
}
