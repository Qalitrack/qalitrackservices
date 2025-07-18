using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OperationalDataService.Core.Entities;

public class Config : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ConfigKey { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string ConfigValue { get; set; } = string.Empty;
    
    [Required]
    public ConfigType ConfigType { get; set; }
    
    [Required]
    public ConfigScope Scope { get; set; }
    
    [StringLength(50)]
    public string? ScopeId { get; set; } // Organization, User, or Process specific
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string Environment { get; set; } = "Production";
    
    public bool IsSecret { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public bool RequiresRestart { get; set; } = false;
    
    public ConfigStatus Status { get; set; } = ConfigStatus.Active;
    
    // Versioning
    public int Version { get; set; } = 1;
    
    [StringLength(50)]
    public string? PreviousVersionId { get; set; }
    
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    
    // Change tracking
    [StringLength(50)]
    public string? ModifiedBy { get; set; }
    
    [StringLength(500)]
    public string? ChangeReason { get; set; }
    
    public DateTime? LastApplied { get; set; }
    
    [StringLength(50)]
    public string? AppliedBy { get; set; }
    
    // Validation
    [StringLength(1000)]
    public string? ValidationRules { get; set; }
    
    public string? DefaultValue { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    
    [StringLength(200)]
    public string? AllowedValues { get; set; } // Comma-separated for enum-like configs
    
    // JSON properties for complex configurations
    public string? MetadataJson { get; set; }
    public string? DependenciesJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? Dependencies
    {
        get => string.IsNullOrEmpty(DependenciesJson) ? null : JsonConvert.DeserializeObject<List<string>>(DependenciesJson);
        set => DependenciesJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Typed value accessors
    public T GetValue<T>()
    {
        if (typeof(T) == typeof(string))
            return (T)(object)ConfigValue;
        
        if (typeof(T) == typeof(bool))
            return (T)(object)bool.Parse(ConfigValue);
        
        if (typeof(T) == typeof(int))
            return (T)(object)int.Parse(ConfigValue);
        
        if (typeof(T) == typeof(decimal))
            return (T)(object)decimal.Parse(ConfigValue);
        
        if (typeof(T) == typeof(DateTime))
            return (T)(object)DateTime.Parse(ConfigValue);
        
        // For complex types, try JSON deserialization
        return JsonConvert.DeserializeObject<T>(ConfigValue)!;
    }
    
    public void SetValue<T>(T value)
    {
        if (value is string stringValue)
            ConfigValue = stringValue;
        else if (value is bool || value is int || value is decimal || value is DateTime)
            ConfigValue = value.ToString()!;
        else
            ConfigValue = JsonConvert.SerializeObject(value);
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Navigation Properties
    public virtual Config? PreviousVersion { get; set; }
    public virtual ICollection<Config> NextVersions { get; set; } = new List<Config>();
}

public enum ConfigType
{
    String = 1,
    Integer = 2,
    Decimal = 3,
    Boolean = 4,
    DateTime = 5,
    Json = 6,
    Array = 7,
    Secret = 8,
    ConnectionString = 9,
    Url = 10,
    Email = 11,
    FilePath = 12
}

public enum ConfigScope
{
    Global = 1,
    Organization = 2,
    User = 3,
    Process = 4,
    Environment = 5,
    Service = 6,
    Operation = 7
}

public enum ConfigStatus
{
    Draft = 1,
    Active = 2,
    Pending = 3,
    Deprecated = 4,
    Archived = 5,
    Failed = 6
}