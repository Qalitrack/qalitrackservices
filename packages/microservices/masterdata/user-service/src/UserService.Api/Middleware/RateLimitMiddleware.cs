using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using UserService.Core.DTOs;

namespace UserService.Api.Middleware;

public class RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
{
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path.Value ?? "";
        
        // Different limits for different endpoints
        var limit = GetRateLimit(endpoint);
        var window = TimeSpan.FromMinutes(1); // 1-minute window

        var clientInfo = _clients.GetOrAdd(clientId, new ClientRequestInfo());
        
        lock (clientInfo)
        {
            var now = DateTime.UtcNow;
            
            // Clean old requests outside the window
            clientInfo.RequestTimes.RemoveAll(time => now - time > window);
            
            if (clientInfo.RequestTimes.Count >= limit)
            {
                logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", 
                    clientId, endpoint);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var response = new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Rate limit exceeded. Please try again later.",
                    Data = new { retryAfter = window.TotalSeconds }
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                context.Response.WriteAsync(jsonResponse);
                return;
            }
            
            clientInfo.RequestTimes.Add(now);
        }

        await next(context);
    }

    private static string GetClientId(HttpContext context)
    {
        // Try to get user ID from JWT claims first
        var userId = context.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";
        
        // Fall back to IP address for anonymous requests
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    private static int GetRateLimit(string endpoint)
    {
        // Different limits for different endpoint patterns
        return endpoint switch
        {
            var path when path.Contains("/auth/login") => 5,  // Login attempts
            var path when path.Contains("/users") && path.Contains("POST") => 10, // User creation
            var path when path.Contains("/users") => 100, // General user operations
            _ => 60 // Default limit
        };
    }

    private class ClientRequestInfo
    {
        public List<DateTime> RequestTimes { get; } = new();
    }
}