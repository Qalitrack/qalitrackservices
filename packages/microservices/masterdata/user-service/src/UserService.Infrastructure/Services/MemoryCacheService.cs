using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Services;

namespace UserService.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    : ICacheService
{
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lockObject = new();

    public async Task<T?> GetAsync<T>(string key)
    {
        return await Task.Run(() => 
        {
            try
            {
                if (memoryCache.TryGetValue(key, out var cachedValue))
                {
                    if (cachedValue is T directValue)
                    {
                        return directValue;
                    }

                    if (cachedValue is string jsonValue)
                    {
                        var deserializedValue = JsonSerializer.Deserialize<T>(jsonValue);
                        return deserializedValue;
                    }
                }

                return default(T);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
                return default(T);
            }
        });
    }


    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        await Task.Run(() => 
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions();
            
                if (expiration.HasValue)
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                cacheOptions.Priority = CacheItemPriority.Normal;
                cacheOptions.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    lock (_lockObject)
                    {
                        _cacheKeys.Remove(evictedKey.ToString()!);
                    }
                    logger.LogDebug("Cache entry evicted. Key: {Key}, Reason: {Reason}", evictedKey, reason);
                });

                var serializedValue = JsonSerializer.Serialize(value);
                memoryCache.Set(key, serializedValue, cacheOptions);

                lock (_lockObject)
                {
                    _cacheKeys.Add(key);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        });
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            memoryCache.Remove(key);
            lock (_lockObject)
            {
                _cacheKeys.Remove(key);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
        return Task.CompletedTask;
    }
    public async Task RemovePatternAsync(string pattern)
    {
        try
        {
            List<string> keysToRemove;
            lock (_lockObject)
            {
                keysToRemove = _cacheKeys
                    .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
    }

    public async Task ReleaseLockAsync(string lockKey)
    {
        await Task.Run(() =>
        {
            try
            {
                memoryCache.Remove(lockKey);
                lock (_lockObject)
                {
                    _cacheKeys.Remove(lockKey);
                }
                logger.LogDebug("Lock released: {LockKey}", lockKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error releasing lock: {LockKey}", lockKey);
            }
        });
    }

    public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan timeout)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Try to add the lock to the cache - if it's not there, we get the lock
                var lockAcquired = memoryCache.TryGetValue(lockKey, out _) == false;
                
                if (lockAcquired)
                {
                    // Set lock with the specified timeout
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = timeout,
                        Priority = CacheItemPriority.High
                    };
                    
                    memoryCache.Set(lockKey, true, cacheOptions);
                    lock (_lockObject)
                    {
                        _cacheKeys.Add(lockKey);
                    }
                    logger.LogDebug("Lock acquired: {LockKey}", lockKey);
                }
                else
                {
                    logger.LogDebug("Failed to acquire lock: {LockKey}", lockKey);
                }
                
                return lockAcquired;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error acquiring lock: {LockKey}", lockKey);
                return false;
            }
        });
    }
}