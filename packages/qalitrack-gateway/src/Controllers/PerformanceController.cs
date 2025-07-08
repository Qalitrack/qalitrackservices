using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QaliTrackGateway.Services;

namespace QaliTrackGateway.Controllers;

/// <summary>
/// Performance monitoring and metrics controller
/// </summary>
[ApiController]
[Route("api/performance")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PerformanceController : ControllerBase
{
    private readonly IAuthorizationCacheService _cacheService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        IAuthorizationCacheService cacheService,
        ILogger<PerformanceController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get authorization cache statistics
    /// </summary>
    [HttpGet("auth-cache-stats")]
    public ActionResult<object> GetAuthCacheStats()
    {
        try
        {
            var (totalEntries, hitRate) = _cacheService.GetCacheStats();
            
            var stats = new
            {
                TotalCacheEntries = totalEntries,
                EstimatedHitRate = hitRate,
                CacheType = "Authorization",
                Timestamp = DateTime.UtcNow,
                Recommendations = GetCacheRecommendations(totalEntries, hitRate)
            };
            
            _logger.LogInformation("Auth cache stats requested: {TotalEntries} entries, {HitRate:P} hit rate", 
                totalEntries, hitRate);
                
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auth cache statistics");
            return StatusCode(500, new { error = "Failed to retrieve cache statistics" });
        }
    }

    /// <summary>
    /// Clear authorization cache for a specific user
    /// </summary>
    [HttpDelete("auth-cache/users/{userId}")]
    public ActionResult ClearUserCache(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "User ID is required" });
            }
            
            _cacheService.ClearUserCache(userId);
            
            _logger.LogInformation("User cache cleared for {UserId} by admin {AdminId}", 
                userId, User.FindFirst("sub")?.Value);
                
            return Ok(new { message = $"Cache cleared for user {userId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to clear user cache" });
        }
    }

    /// <summary>
    /// Get performance recommendations based on cache metrics
    /// </summary>
    [HttpGet("recommendations")]
    public ActionResult<object> GetPerformanceRecommendations()
    {
        try
        {
            var (totalEntries, hitRate) = _cacheService.GetCacheStats();
            var recommendations = GetCacheRecommendations(totalEntries, hitRate);
            
            var result = new
            {
                CacheMetrics = new { TotalEntries = totalEntries, HitRate = hitRate },
                Recommendations = recommendations,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance recommendations");
            return StatusCode(500, new { error = "Failed to generate recommendations" });
        }
    }

    private List<string> GetCacheRecommendations(int totalEntries, double hitRate)
    {
        var recommendations = new List<string>();
        
        if (hitRate < 0.5)
        {
            recommendations.Add("Cache hit rate is low (<50%). Consider increasing cache duration or reviewing access patterns.");
        }
        
        if (totalEntries > 10000)
        {
            recommendations.Add("High number of cache entries detected. Consider implementing cache size limits or TTL cleanup.");
        }
        
        if (totalEntries < 10)
        {
            recommendations.Add("Very few cache entries. Cache might not be effectively utilized or TTL is too short.");
        }
        
        if (hitRate > 0.9)
        {
            recommendations.Add("Excellent cache performance! Consider monitoring for cache invalidation efficiency.");
        }
        
        if (recommendations.Count == 0)
        {
            recommendations.Add("Cache performance is within normal parameters.");
        }
        
        return recommendations;
    }
}