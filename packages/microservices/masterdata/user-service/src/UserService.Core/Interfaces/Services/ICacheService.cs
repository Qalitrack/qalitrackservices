namespace UserService.Core.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
    Task ReleaseLockAsync(string lockKey); // New
    Task<bool> AcquireLockAsync(string lockKey, TimeSpan fromSeconds);

    // Atomically increments a counter and returns the new value, creating it
    // at 1 with the given expiration if it doesn't exist yet. Needed for
    // rate-limit/attempt counters — a plain GetAsync-then-SetAsync round trip
    // lets concurrent callers all read the same starting value and race.
    Task<long> IncrementAsync(string key, TimeSpan? expiration = null);
}