using Microsoft.Extensions.Caching.Memory;

namespace QaliTrackGateway.Services;

/// <summary>
/// Service for managing authorization cache operations
/// </summary>
public interface IAuthorizationCacheService
{
    void ClearUserCache(string userId);
    (int TotalEntries, double EstimatedHitRate) GetCacheStats();
    void SetAuthorizationDecision(string userId, string path, bool decision, TimeSpan? expiry = null);
    bool? GetAuthorizationDecision(string userId, string path);
    void SetUserPermissions(string userId, List<string> permissions, TimeSpan? expiry = null);
    List<string>? GetUserPermissions(string userId);
}

public class AuthorizationCacheService : IAuthorizationCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationCacheService> _logger;
    private int _hitCount = 0;
    private int _missCount = 0;

    public AuthorizationCacheService(IMemoryCache cache, ILogger<AuthorizationCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void ClearUserCache(string userId)
    {
        var keysToRemove = new List<string>();
        
        // Note: In production, consider using a cache with tagging support
        // This is a simplified implementation
        try
        {
            var cacheField = _cache.GetType().GetField("_cache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cacheDict = cacheField?.GetValue(_cache) as IDictionary<object, object>;
            
            if (cacheDict != null)
            {
                foreach (var item in cacheDict)
                {
                    if (item.Key.ToString()?.Contains($"_{userId}_") == true)
                    {
                        keysToRemove.Add(item.Key.ToString()!);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enumerating cache entries for user {UserId}", userId);
        }
        
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
        
        _logger.LogInformation("Cleared {Count} cached entries for user {UserId}", keysToRemove.Count, userId);
    }

    public (int TotalEntries, double EstimatedHitRate) GetCacheStats()
    {
        try
        {
            var cacheField = _cache.GetType().GetField("_cache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cacheDict = cacheField?.GetValue(_cache) as IDictionary<object, object>;
            var totalEntries = cacheDict?.Count ?? 0;
            
            // Calculate hit rate based on actual hits/misses
            var totalRequests = _hitCount + _missCount;
            var hitRate = totalRequests > 0 ? (double)_hitCount / totalRequests : 0.0;
            
            return (totalEntries, hitRate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cache statistics");
            return (0, 0.0);
        }
    }

    public void SetAuthorizationDecision(string userId, string path, bool decision, TimeSpan? expiry = null)
    {
        var cacheKey = $"auth_decision_{userId}_{path}";
        var expiryTime = expiry ?? TimeSpan.FromMinutes(5);
        
        _cache.Set(cacheKey, decision, expiryTime);
        
        _logger.LogDebug("Cached authorization decision for user {UserId} on {Path}: {Decision}", 
            userId, path, decision);
    }

    public bool? GetAuthorizationDecision(string userId, string path)
    {
        var cacheKey = $"auth_decision_{userId}_{path}";
        
        if (_cache.TryGetValue(cacheKey, out bool cachedDecision))
        {
            Interlocked.Increment(ref _hitCount);
            _logger.LogDebug("Authorization decision cache hit for user {UserId} on {Path}", userId, path);
            return cachedDecision;
        }
        
        Interlocked.Increment(ref _missCount);
        return null;
    }

    public void SetUserPermissions(string userId, List<string> permissions, TimeSpan? expiry = null)
    {
        var cacheKey = $"user_permissions_{userId}";
        var expiryTime = expiry ?? TimeSpan.FromMinutes(10);
        
        _cache.Set(cacheKey, permissions, expiryTime);
        
        _logger.LogDebug("Cached permissions for user {UserId}: {PermissionCount} permissions", 
            userId, permissions.Count);
    }

    public List<string>? GetUserPermissions(string userId)
    {
        var cacheKey = $"user_permissions_{userId}";
        
        if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions))
        {
            Interlocked.Increment(ref _hitCount);
            return cachedPermissions;
        }
        
        Interlocked.Increment(ref _missCount);
        return null;
    }
}