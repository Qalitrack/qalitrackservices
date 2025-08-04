using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Services;

public class RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    : ICacheService
{
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
            {
                logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }

            var deserializedValue = JsonSerializer.Deserialize<T>(cachedValue);
            logger.LogDebug("Cache hit for key: {Key}", key);
            return deserializedValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default expiration of 15 minutes for 1000+ users (reduced from 30)
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            }

            // Add sliding expiration to keep frequently accessed items
            options.SlidingExpiration = TimeSpan.FromMinutes(5);

            await distributedCache.SetStringAsync(key, serializedValue, options);
            logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await distributedCache.RemoveAsync(key);
            logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public Task RemovePatternAsync(string pattern)
    {
        try
        {
            // Redis pattern-based deletion requires a custom implementation
            // This is a simplified version - in production, consider using Redis SCAN with pattern
            logger.LogWarning("Pattern-based cache removal not fully implemented for Redis. Pattern: {Pattern}", pattern);
        
            // For now, we'll just log the pattern and rely on expiration
            // In production, implement Redis SCAN with LUA script for pattern deletion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
        return Task.CompletedTask;
    }
}