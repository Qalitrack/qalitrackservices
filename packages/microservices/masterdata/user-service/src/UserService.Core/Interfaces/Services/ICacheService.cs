namespace UserService.Core.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
    Task ReleaseLockAsync(string lockKey); // New
    Task<bool> AcquireLockAsync(string lockKey, TimeSpan fromSeconds);
}