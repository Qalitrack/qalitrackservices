using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TechnicianApi.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorId = Guid.NewGuid().ToString();

        // Log full exception details (not sent to client)
        _logger.LogError(exception, "Error ID: {ErrorId} - Unhandled exception occurred", errorId);

        var response = new ErrorResponse
        {
            ErrorId = errorId,
            Message = GetSafeMessage(exception),
            StatusCode = GetStatusCode(exception)
        };

        // Only include details in development
        if (_env.IsDevelopment())
        {
            response.Details = exception.Message;
            response.StackTrace = exception.StackTrace;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static string GetSafeMessage(Exception exception)
    {
        return exception switch
        {
            DbUpdateException => "A database operation failed. Please contact support.",
            InvalidOperationException => "The requested operation is invalid.",
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            ArgumentException => "Invalid request parameters.",
            _ => "An unexpected error occurred. Please contact support."
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            DbUpdateException => (int)HttpStatusCode.Conflict,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}

public class ErrorResponse
{
    public string ErrorId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
}
