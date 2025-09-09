using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Services;
using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache distributedCache, 
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.Preserve
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default(T);
        }

        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }

            var deserializedValue = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return deserializedValue;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for cache key: {Key}", key);
            // Remove corrupted cache entry
            await RemoveAsync(key);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (value is null)
        {
            return;
        }

        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default expiration of 15 minutes
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            }

            // Add sliding expiration to keep frequently accessed items fresh
            options.SlidingExpiration = TimeSpan.FromMinutes(5);

            await _distributedCache.SetStringAsync(key, serializedValue, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization error for cache key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        try
        {
            await _distributedCache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            _logger.LogWarning("Cache pattern is null or empty");
            return;
        }

        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            
            var keys = new List<RedisKey>();
            foreach (var key in server.Keys(pattern: pattern, pageSize: 1000))
            {
                keys.Add(key);
                _logger.LogDebug("Found key matching pattern {Pattern}: {Key}", pattern, key);
            }

            if (keys.Any())
            {
                await database.KeyDeleteAsync(keys.ToArray());
                _logger.LogInformation("Deleted {Count} keys matching pattern: {Pattern}", keys.Count, pattern);
                
                // Verify deletion
                foreach (var key in keys)
                {
                    var exists = await database.KeyExistsAsync(key);
                    if (exists)
                    {
                        _logger.LogWarning("Key {Key} still exists after deletion for pattern: {Pattern}", key, pattern);
                    }
                }
            }
            else
            {
                _logger.LogDebug("No cache entries found matching pattern: {Pattern}", pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            _logger.LogWarning("Invalid lock key provided");
            return false;
        }

        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            bool acquired = await database.StringSetAsync(lockKey, "locked", lockTimeout, When.NotExists);
            _logger.LogDebug(acquired ? "Acquired lock for key: {LockKey}" : "Failed to acquire lock for key: {LockKey}", lockKey);
            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for key: {LockKey}", lockKey);
            return false;
        }
    }

    public async Task ReleaseLockAsync(string lockKey)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            await database.KeyDeleteAsync(lockKey);
            _logger.LogDebug("Released lock for key: {LockKey}", lockKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for key: {LockKey}", lockKey);
        }
    }
}