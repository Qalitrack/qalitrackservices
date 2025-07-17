using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class ConfigDto
{
    public string Id { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public ConfigType ConfigType { get; set; }
    public ConfigScope Scope { get; set; }
    public string? ScopeId { get; set; }
    public string? Description { get; set; }
    public string Environment { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public bool IsReadOnly { get; set; }
    public bool RequiresRestart { get; set; }
    public ConfigStatus Status { get; set; }
    public int Version { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CreateConfigRequest
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public ConfigType ConfigType { get; set; }
    public ConfigScope Scope { get; set; } = ConfigScope.Global;
    public string? ScopeId { get; set; }
    public string? Description { get; set; }
    public string Environment { get; set; } = "Production";
    public bool IsSecret { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public bool RequiresRestart { get; set; } = false;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdateConfigRequest
{
    public string? ConfigValue { get; set; }
    public string? Description { get; set; }
    public bool? IsReadOnly { get; set; }
    public bool? RequiresRestart { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ChangeReason { get; set; }
}

public class ConfigValidationDto
{
    public string ConfigKey { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
}

public class ConfigComparisonDto
{
    public string ConfigKey { get; set; } = string.Empty;
    public int Version1 { get; set; }
    public int Version2 { get; set; }
    public string? Value1 { get; set; }
    public string? Value2 { get; set; }
    public bool HasChanged { get; set; }
    public List<string> Changes { get; set; } = new List<string>();
}

public class ConfigSyncResultDto
{
    public string SourceEnvironment { get; set; } = string.Empty;
    public string TargetEnvironment { get; set; } = string.Empty;
    public int TotalConfigs { get; set; }
    public int SyncedConfigs { get; set; }
    public int SkippedConfigs { get; set; }
    public int FailedConfigs { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class ConfigTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ConfigDto> Configs { get; set; } = new List<ConfigDto>();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreateConfigTemplateRequest
{
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateConfigRequest> Configs { get; set; } = new List<CreateConfigRequest>();
}

public class ConfigAuditDto
{
    public string Id { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Reason { get; set; }
}

public class ConfigUsageReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalConfigs { get; set; }
    public int ChangedConfigs { get; set; }
    public int AccessedConfigs { get; set; }
    public List<string> MostChangedConfigs { get; set; } = new List<string>();
    public List<string> MostAccessedConfigs { get; set; } = new List<string>();
}