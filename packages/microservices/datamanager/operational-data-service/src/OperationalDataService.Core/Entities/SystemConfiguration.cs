using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class SystemConfiguration : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string ConfigurationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Category { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Key { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public required string Value { get; set; }
    
    [Required]
    public ConfigurationType Type { get; set; }
    
    [Required]
    public ConfigurationScope Scope { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public bool IsEncrypted { get; set; } = false;
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsReadOnly { get; set; } = false;
    
    [MaxLength(2000)]
    public string? DefaultValue { get; set; }
    
    [MaxLength(1000)]
    public string? ValidationRules { get; set; }
    
    [MaxLength(500)]
    public string? DisplayName { get; set; }
    
    [MaxLength(1000)]
    public string? HelpText { get; set; }
    
    public List<string> AllowedValues { get; set; } = new();
    
    public ConfigurationConstraints? Constraints { get; set; }
    
    public List<ConfigurationDependency> Dependencies { get; set; } = new();
    
    public List<ConfigurationHistory> History { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public List<string> Tags { get; set; } = new();
    
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string? ModifiedBy { get; set; }
    
    public int Version { get; set; } = 1;
    
    public bool RequiresRestart { get; set; } = false;
    
    public DateTime? EffectiveFrom { get; set; }
    
    public DateTime? EffectiveTo { get; set; }
    
    [MaxLength(500)]
    public string? ChangeReason { get; set; }
}

public class ConfigurationConstraints
{
    public string? MinValue { get; set; }
    
    public string? MaxValue { get; set; }
    
    public int? MinLength { get; set; }
    
    public int? MaxLength { get; set; }
    
    [MaxLength(500)]
    public string? RegexPattern { get; set; }
    
    public bool? IsRequired { get; set; }
    
    public List<string> AllowedValues { get; set; } = new();
    
    public List<string> ExcludedValues { get; set; } = new();
    
    [MaxLength(1000)]
    public string? CustomValidation { get; set; }
}

public class ConfigurationDependency
{
    [Required]
    [MaxLength(100)]
    public required string DependsOn { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Condition { get; set; }
    
    [MaxLength(1000)]
    public string? ExpectedValue { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsRequired { get; set; } = true;
}

public class ConfigurationHistory
{
    [Required]
    public DateTime ChangedAt { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string ChangedBy { get; set; }
    
    [MaxLength(2000)]
    public string? OldValue { get; set; }
    
    [MaxLength(2000)]
    public string? NewValue { get; set; }
    
    [MaxLength(500)]
    public string? ChangeReason { get; set; }
    
    [MaxLength(100)]
    public string? ChangeType { get; set; }
    
    public int Version { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum ConfigurationType
{
    String,
    Integer,
    Decimal,
    Boolean,
    DateTime,
    Json,
    Xml,
    Url,
    Email,
    Password,
    File,
    Directory,
    Color,
    Custom
}

public enum ConfigurationScope
{
    Global,
    Organization,
    User,
    Session,
    Application,
    Service,
    Environment,
    Deployment
}