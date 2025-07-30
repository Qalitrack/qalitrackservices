using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }

            var deserializedValue = JsonSerializer.Deserialize<T>(cachedValue);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return deserializedValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
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

            await _distributedCache.SetStringAsync(key, serializedValue, options);
            _logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern)
    {
        try
        {
            // Redis pattern-based deletion requires a custom implementation
            // This is a simplified version - in production, consider using Redis SCAN with pattern
            _logger.LogWarning("Pattern-based cache removal not fully implemented for Redis. Pattern: {Pattern}", pattern);
            
            // For now, we'll just log the pattern and rely on expiration
            // In production, implement Redis SCAN with LUA script for pattern deletion
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
    }
}