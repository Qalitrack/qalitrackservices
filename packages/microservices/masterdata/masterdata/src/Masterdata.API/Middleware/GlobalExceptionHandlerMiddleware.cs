using System;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Masterdata.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
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
            // Log full exception details internally (with stack trace)
            _logger.LogError(ex, "Unhandled exception occurred. Request: {Method} {Path}", 
                context.Request.Method, 
                context.Request.Path);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;
        var errorResponse = new ErrorResponse
        {
            RequestId = context.TraceIdentifier ?? Guid.NewGuid().ToString()
        };

        switch (exception)
        {
            case ApplicationException ex:
                if (ex.Message.Contains("Invalid Token"))
                {
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.Message = "Invalid or expired token";
                    errorResponse.ErrorCode = "INVALID_TOKEN";
                    break;
                }
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = SanitizeMessage(ex.Message);
                errorResponse.ErrorCode = "BAD_REQUEST";
                break;
                
            case KeyNotFoundException ex:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "The requested resource was not found";
                errorResponse.ErrorCode = "NOT_FOUND";
                break;
                
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Unauthorized access";
                errorResponse.ErrorCode = "UNAUTHORIZED";
                break;
                
            case DbUpdateException ex:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "A database error occurred. Please try again later.";
                errorResponse.ErrorCode = "DATABASE_ERROR";
                
                // Only in development, show sanitized details
                if (_env.IsDevelopment())
                {
                    errorResponse.Details = "Database update failed. Check logs for details.";
                }
                break;
                
            case DbException ex:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "A database error occurred. Please try again later.";
                errorResponse.ErrorCode = "DATABASE_ERROR";
                
                // Only in development, show sanitized details
                if (_env.IsDevelopment())
                {
                    errorResponse.Details = $"Database Error: {ex.GetType().Name}. Check logs for details.";
                }
                break;
                
            case ArgumentNullException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "A required parameter was not provided";
                errorResponse.ErrorCode = "MISSING_PARAMETER";
                
                if (_env.IsDevelopment())
                {
                    errorResponse.Details = $"Parameter: {ex.ParamName}";
                }
                break;
                
            case ArgumentException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Invalid parameter provided";
                errorResponse.ErrorCode = "INVALID_PARAMETER";
                
                if (_env.IsDevelopment())
                {
                    errorResponse.Details = SanitizeMessage(ex.Message);
                }
                break;
                
            default:
                // Unhandled error - NEVER expose internal details to client
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An unexpected error occurred. Please contact support with the request ID.";
                errorResponse.ErrorCode = "INTERNAL_ERROR";
                
                // Even in development, don't expose full stack traces to the response
                // Developers should check logs instead
                if (_env.IsDevelopment())
                {
                    errorResponse.Details = $"Exception Type: {exception.GetType().Name}. Check application logs for full details.";
                }
                break;
        }
        
        var options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        var json = JsonSerializer.Serialize(errorResponse, options);
        
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Removes potentially sensitive information from error messages
    /// </summary>
    private string SanitizeMessage(string message)
    {
        // Remove SQL-like patterns
        message = System.Text.RegularExpressions.Regex.Replace(
            message, 
            @"(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|JOIN|TABLE)\s+\S+", 
            "[SQL Query]", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Remove connection strings
        message = System.Text.RegularExpressions.Regex.Replace(
            message, 
            @"(Server|Data Source|Initial Catalog|User ID|Password|Integrated Security)=[^;]+", 
            "[Connection Info]", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Remove file paths
        message = System.Text.RegularExpressions.Regex.Replace(
            message, 
            @"[A-Za-z]:\\[\w\\\s\.\-]+", 
            "[File Path]");
        
        return message;
    }
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = "An error occurred while processing your request.";
    public string? Details { get; set; }
    public string? ErrorCode { get; set; }
    public string RequestId { get; set; } = string.Empty;
}