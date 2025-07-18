using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ComplianceService.Core.Entities;

public class Regulatory : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string RegulatoryName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string RegulatoryCode { get; set; } = string.Empty;
    
    [Required]
    public RegulatoryType RegulatoryType { get; set; }
    
    [StringLength(100)]
    public string RegulatoryBody { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Jurisdiction { get; set; } = string.Empty; // National, Regional, Local
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public RegulatoryStatus Status { get; set; } = RegulatoryStatus.Active;
    
    // Dates and Versioning
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    
    [StringLength(20)]
    public string Version { get; set; } = "1.0";
    
    [StringLength(50)]
    public string? SupersedesRegulatoryId { get; set; }
    
    // Compliance Requirements
    public bool IsMandatory { get; set; } = true;
    public CompliancePriority Priority { get; set; } = CompliancePriority.Medium;
    
    public int? ComplianceTimelineDays { get; set; }
    public int? ReviewFrequencyDays { get; set; }
    
    // Penalties and Consequences
    public decimal? MinPenaltyAmount { get; set; }
    public decimal? MaxPenaltyAmount { get; set; }
    
    [StringLength(10)]
    public string PenaltyCurrency { get; set; } = "KES";
    
    [StringLength(1000)]
    public string? PenaltyDescription { get; set; }
    
    [StringLength(1000)]
    public string? ConsequencesOfNonCompliance { get; set; }
    
    // External Integration
    [StringLength(500)]
    public string? ExternalApiUrl { get; set; }
    
    [StringLength(100)]
    public string? ExternalSystemId { get; set; }
    
    public bool RequiresExternalValidation { get; set; } = false;
    
    // Monitoring and Reporting
    public bool RequiresContinuousMonitoring { get; set; } = false;
    public bool RequiresPeriodicReporting { get; set; } = false;
    
    public int? ReportingFrequencyDays { get; set; }
    
    [StringLength(200)]
    public string? ReportingTemplate { get; set; }
    
    // JSON properties for flexible data storage
    public string? RequirementsJson { get; set; }
    public string? ValidationRulesJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public List<RegulatoryRequirement>? Requirements
    {
        get => string.IsNullOrEmpty(RequirementsJson) ? null : JsonConvert.DeserializeObject<List<RegulatoryRequirement>>(RequirementsJson);
        set => RequirementsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<ValidationRule>? ValidationRules
    {
        get => string.IsNullOrEmpty(ValidationRulesJson) ? null : JsonConvert.DeserializeObject<List<ValidationRule>>(ValidationRulesJson);
        set => ValidationRulesJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Regulatory? SupersededRegulatory { get; set; }
    public virtual ICollection<Regulatory> SupersedingRegulations { get; set; } = new List<Regulatory>();
    public virtual ICollection<Compliance> ComplianceRecords { get; set; } = new List<Compliance>();
    public virtual ICollection<ComplianceRule> ComplianceRules { get; set; } = new List<ComplianceRule>();
}

public class RegulatoryRequirement
{
    public string RequirementId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; } = true;
    public string? Category { get; set; }
    public List<string>? ApplicableEntityTypes { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ValidationRule
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // MinValue, MaxValue, Pattern, Custom
    public string? Expression { get; set; }
    public object? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
}

public enum RegulatoryType
{
    WeightRegulations = 1,
    TransportLicensing = 2,
    EnvironmentalStandards = 3,
    SafetyRegulations = 4,
    TaxationRules = 5,
    DataProtection = 6,
    QualityStandards = 7,
    InternationalStandards = 8,
    IndustrySpecific = 9,
    CustomRegulations = 10
}

public enum RegulatoryStatus
{
    Draft = 1,
    Active = 2,
    Pending = 3,
    Suspended = 4,
    Superseded = 5,
    Repealed = 6,
    UnderReview = 7
}