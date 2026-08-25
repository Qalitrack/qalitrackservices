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
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            // IMPORTANT: Removed ReferenceHandler.Preserve to avoid breaking plain strings
            // If you really need Preserve for complex cyclic objects → handle them separately
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default;
        }

        try
        {
            var cachedString = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedString))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                // Direct return for strings - no JSON parsing needed
                return (T)(object)cachedString;
            }

            var value = JsonSerializer.Deserialize<T>(cachedString, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key} (type: {Type})", key, typeof(T).Name);
            return value;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for key: {Key}", key);
            await RemoveAsync(key); // clean up corrupted entry
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key) || value == null)
        {
            return;
        }

        try
        {
            string serialized;

            if (typeof(T) == typeof(string))
            {
                // Store strings directly - no JSON wrapper
                serialized = value.ToString()!;
            }
            else
            {
                serialized = JsonSerializer.Serialize(value, _jsonOptions);
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5) // keep hot items alive
            };

            await _distributedCache.SetStringAsync(key, serialized, options);
            _logger.LogDebug("Cache set successful for key: {Key} (type: {Type}, size: {Size} chars)",
                key, typeof(T).Name, serialized.Length);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization failed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache key: {Key}", key);
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
            var db = _connectionMultiplexer.GetDatabase();
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

            var keys = new List<RedisKey>();
            // Note: using sync Keys here (like your old version) - fine for dev/small prod
            // For large-scale prod consider switching to async SCAN + lua script
            foreach (var key in server.Keys(pattern: pattern, pageSize: 1000))
            {
                keys.Add(key);
            }

            if (keys.Count > 0)
            {
                await db.KeyDeleteAsync(keys.ToArray());
                _logger.LogInformation("Removed {Count} keys matching pattern: {Pattern}", keys.Count, pattern);

                // Optional: verify (can remove in production)
                foreach (var key in keys)
                {
                    if (await db.KeyExistsAsync(key))
                    {
                        _logger.LogWarning("Key still exists after delete: {Key}", key);
                    }
                }
            }
            else
            {
                _logger.LogDebug("No keys found for pattern: {Pattern}", pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout)
    {
        if (string.IsNullOrWhiteSpace(lockKey)) return false;

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            bool acquired = await db.StringSetAsync(lockKey, "locked", lockTimeout, When.NotExists);
            _logger.LogDebug(acquired ? "Lock acquired: {LockKey}" : "Lock already held: {LockKey}", lockKey);
            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire lock: {LockKey}", lockKey);
            return false;
        }
    }

    public async Task ReleaseLockAsync(string lockKey)
    {
        if (string.IsNullOrWhiteSpace(lockKey)) return;

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(lockKey);
            _logger.LogDebug("Lock released: {LockKey}", lockKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to release lock (will expire naturally): {LockKey}", lockKey);
        }
    }

    public async Task<long> IncrementAsync(string key, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must not be empty.", nameof(key));
        }

        var db = _connectionMultiplexer.GetDatabase();
        // Redis INCR is atomic — concurrent callers each get a distinct,
        // correctly-ordered value instead of racing on a read-then-write.
        var newValue = await db.StringIncrementAsync(key);
        if (newValue == 1 && expiration.HasValue)
        {
            // Only set TTL on first creation, so this is a fixed window
            // (e.g. "5 attempts per 10 minutes"), not a sliding one that
            // resets on every increment.
            await db.KeyExpireAsync(key, expiration.Value);
        }

        return newValue;
    }
}