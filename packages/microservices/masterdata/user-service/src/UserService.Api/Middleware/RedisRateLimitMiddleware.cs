using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using UserService.Core.DTOs;

namespace UserService.Api.Middleware;

public class RedisRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisRateLimitMiddleware> _logger;

    public RedisRateLimitMiddleware(
        RequestDelegate next, 
        IDistributedCache distributedCache,
        ILogger<RedisRateLimitMiddleware> logger)
    {
        _next = next;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path.Value ?? "";
        
        // Different limits for different endpoints - optimized for 1000+ users
        var limit = GetRateLimit(endpoint);
        var window = TimeSpan.FromMinutes(1);
        
        var key = $"rate_limit:{clientId}:{endpoint}:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
        
        try
        {
            var currentCountStr = await _distributedCache.GetStringAsync(key);
            var currentCount = string.IsNullOrEmpty(currentCountStr) ? 0 : int.Parse(currentCountStr);
            
            if (currentCount >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Count: {Count}/{Limit}", 
                    clientId, endpoint, currentCount, limit);
                
                await WriteRateLimitResponse(context, window);
                return;
            }
            
            // Increment counter with expiration
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = window
            };
            
            await _distributedCache.SetStringAsync(key, (currentCount + 1).ToString(), options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limiting for client {ClientId}", clientId);
            // Continue processing if rate limiting fails - don't block legitimate requests
        }

        await _next(context);
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
        // Adjusted limits for 1000+ users - more restrictive to prevent abuse
        return endpoint switch
        {
            var path when path.Contains("/auth/login") => 10,  // Login attempts - increased slightly
            var path when path.Contains("/users") && path.Contains("POST") => 20, // User creation - reduced
            var path when path.Contains("/users/paged") => 200, // Paginated requests - higher limit
            var path when path.Contains("/users") => 150, // General user operations - increased
            _ => 100 // Default limit - increased for better UX
        };
    }

    private static async Task WriteRateLimitResponse(HttpContext context, TimeSpan window)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Add("Retry-After", window.TotalSeconds.ToString());
        
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
        
        await context.Response.WriteAsync(jsonResponse);
    }
}