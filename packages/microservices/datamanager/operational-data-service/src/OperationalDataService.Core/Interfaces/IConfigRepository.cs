using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IConfigRepository : IRepository<Config>
{
    Task<Config?> GetByKeyAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<List<Config>> GetByEnvironmentAsync(string environment);
    Task<List<Config>> GetByScopeAsync(ConfigScope scope, string? scopeId = null);
    Task<List<Config>> GetByTypeAsync(ConfigType configType);
    Task<List<Config>> GetActiveConfigsAsync();
    Task<List<Config>> GetSecretConfigsAsync();
    Task<List<Config>> GetConfigVersionsAsync(string configKey);
    Task<Config?> GetConfigVersionAsync(string configKey, int version);
    Task<List<Config>> GetExpiredConfigsAsync();
    Task<List<Config>> GetConfigsRequiringRestartAsync();
    Task<Dictionary<string, string>> GetAllConfigValuesAsync(string environment, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<List<Config>> GetModifiedConfigsAsync(DateTime fromDate, DateTime toDate);
    Task<bool> ConfigExistsAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
}