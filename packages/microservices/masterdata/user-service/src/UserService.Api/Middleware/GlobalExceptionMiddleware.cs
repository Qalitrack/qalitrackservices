using System.Net;
using System.Text.Json;
using UserService.Core.DTOs;

namespace UserService.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponseDto<object>
        {
            Success = false,
            Message = GetErrorMessage(exception),
            Data = null
        };

        switch (exception)
        {
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "The requested resource was not found.";
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "You are not authorized to access this resource.";
                break;

            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            case ArgumentNullException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Required parameter is missing.";
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid request parameters.";
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "The request timed out. Please try again.";
                break;

            case NotImplementedException:
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                response.Message = "This feature is not yet implemented.";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => "Resource not found",
            UnauthorizedAccessException => "Unauthorized access",
            InvalidOperationException => exception.Message,
            ArgumentNullException => "Required parameter is missing",
            ArgumentException => "Invalid argument",
            TimeoutException => "Request timeout",
            NotImplementedException => "Feature not implemented",
            _ => "Internal server error"
        };
    }
}