using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IConfigService
{
    // Configuration Management
    Task<ConfigDto> CreateConfigAsync(CreateConfigRequest request);
    Task<ConfigDto?> GetConfigAsync(string configId);
    Task<ConfigDto?> GetConfigByKeyAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<ConfigDto> UpdateConfigAsync(string configId, UpdateConfigRequest request);
    Task<bool> DeleteConfigAsync(string configId);
    
    // Configuration Retrieval
    Task<List<ConfigDto>> GetConfigsByEnvironmentAsync(string environment);
    Task<List<ConfigDto>> GetConfigsByScopeAsync(ConfigScope scope, string? scopeId = null);
    Task<List<ConfigDto>> GetConfigsByTypeAsync(ConfigType configType);
    Task<List<ConfigDto>> GetActiveConfigsAsync();
    Task<Dictionary<string, string>> GetAllConfigValuesAsync(string environment, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    
    // Typed Configuration Access
    Task<T?> GetConfigValueAsync<T>(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<bool> SetConfigValueAsync<T>(string configKey, T value, ConfigScope scope = ConfigScope.Global, string? scopeId = null, string? userId = null);
    Task<string?> GetStringConfigAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<int?> GetIntConfigAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<bool?> GetBoolConfigAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    Task<decimal?> GetDecimalConfigAsync(string configKey, ConfigScope scope = ConfigScope.Global, string? scopeId = null);
    
    // Configuration Versioning
    Task<List<ConfigDto>> GetConfigVersionsAsync(string configKey);
    Task<ConfigDto?> GetConfigVersionAsync(string configId, int version);
    Task<bool> RollbackConfigAsync(string configKey, int version, string userId);
    Task<ConfigComparisonDto> CompareConfigVersionsAsync(string configKey, int version1, int version2);
    
    // Configuration Validation
    Task<ConfigValidationDto> ValidateConfigAsync(string configKey, string configValue);
    Task<List<ConfigValidationDto>> ValidateAllConfigsAsync();
    Task<bool> ValidateConfigDependenciesAsync(string configKey);
    
    // Environment Management
    Task<List<string>> GetEnvironmentsAsync();
    Task<bool> CloneEnvironmentConfigsAsync(string sourceEnvironment, string targetEnvironment, string userId);
    Task<ConfigSyncResultDto> SyncConfigBetweenEnvironmentsAsync(string sourceEnvironment, string targetEnvironment, List<string> configKeys, string userId);
    
    // Configuration Templates
    Task<ConfigTemplateDto> CreateConfigTemplateAsync(CreateConfigTemplateRequest request);
    Task<List<ConfigTemplateDto>> GetConfigTemplatesAsync();
    Task<bool> ApplyConfigTemplateAsync(string templateId, string environment, ConfigScope scope, string? scopeId, string userId);
    
    // Dynamic Configuration
    Task<bool> ReloadConfigurationAsync();
    Task<bool> NotifyConfigChangeAsync(string configKey);
    Task<List<string>> GetConfigKeysRequiringRestartAsync();
    
    // Security and Secrets
    Task<bool> EncryptSecretConfigAsync(string configKey);
    Task<bool> DecryptSecretConfigAsync(string configKey);
    Task<List<ConfigDto>> GetSecretConfigsAsync();
    Task<bool> RotateSecretConfigAsync(string configKey, string userId);
    
    // Configuration Audit
    Task<List<ConfigAuditDto>> GetConfigAuditTrailAsync(string configKey);
    Task<List<ConfigAuditDto>> GetConfigChangesByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ConfigUsageReportDto> GetConfigUsageReportAsync(DateTime fromDate, DateTime toDate);
}