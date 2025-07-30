using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using UserService.Core.DTOs;

namespace UserService.Api.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate POST and PUT requests with JSON content
        if ((context.Request.Method == "POST" || context.Request.Method == "PUT") &&
            context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    // Try to parse as JSON to validate structure
                    using var jsonDoc = JsonDocument.Parse(body);
                    
                    // Additional validation could be added here
                    // For now, we'll rely on model validation in controllers
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Invalid JSON in request: {Error}", ex.Message);
                    
                    var errorResponse = new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid JSON format in request body.",
                        Data = null
                    };

                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    
                    var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    await context.Response.WriteAsync(jsonResponse);
                    return;
                }
            }
        }

        await _next(context);
    }
}