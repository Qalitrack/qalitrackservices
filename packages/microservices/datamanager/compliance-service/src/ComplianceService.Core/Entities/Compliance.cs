using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ComplianceService.Core.Entities;

public class Compliance : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ComplianceName { get; set; } = string.Empty;
    
    [Required]
    public ComplianceType ComplianceType { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty; // Transaction, Weight, Vehicle, Driver, etc.
    
    [Required]
    [StringLength(50)]
    public string EntityId { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public ComplianceStatus Status { get; set; } = ComplianceStatus.Pending;
    
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    [StringLength(50)]
    public string? AssignedUserId { get; set; }
    
    [StringLength(100)]
    public string? AssignedUserName { get; set; }
    
    public CompliancePriority Priority { get; set; } = CompliancePriority.Medium;
    
    // Risk Assessment
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    public decimal RiskScore { get; set; } = 0;
    
    [StringLength(1000)]
    public string? RiskAssessment { get; set; }
    
    // Regulatory Context
    [StringLength(100)]
    public string? RegulatoryFramework { get; set; }
    
    [StringLength(100)]
    public string? RegulatoryBody { get; set; }
    
    [StringLength(50)]
    public string? ComplianceStandardId { get; set; }
    
    // Results and Actions
    public bool IsCompliant { get; set; } = false;
    public decimal ComplianceScore { get; set; } = 0; // 0-100
    
    [StringLength(1000)]
    public string? NonComplianceReason { get; set; }
    
    [StringLength(1000)]
    public string? RecommendedActions { get; set; }
    
    [StringLength(1000)]
    public string? CompletedActions { get; set; }
    
    // Monitoring
    public bool RequiresContinuousMonitoring { get; set; } = false;
    public DateTime? NextCheckDate { get; set; }
    public int? MonitoringFrequencyDays { get; set; }
    
    // JSON properties for flexible data storage
    public string? CheckResultsJson { get; set; }
    public string? MetadataJson { get; set; }
    public string? EvidenceJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? CheckResults
    {
        get => string.IsNullOrEmpty(CheckResultsJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(CheckResultsJson);
        set => CheckResultsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<ComplianceEvidence>? Evidence
    {
        get => string.IsNullOrEmpty(EvidenceJson) ? null : JsonConvert.DeserializeObject<List<ComplianceEvidence>>(EvidenceJson);
        set => EvidenceJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
    public virtual ICollection<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
    public virtual ICollection<ComplianceAudit> AuditTrail { get; set; } = new List<ComplianceAudit>();
    public virtual RegulatoryStandard? RegulatoryStandard { get; set; }
}

public class ComplianceEvidence
{
    public string EvidenceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? Url { get; set; }
    public DateTime CollectedDate { get; set; } = DateTime.UtcNow;
    public string? CollectedBy { get; set; }
}

public enum ComplianceType
{
    WeightCompliance = 1,
    LicenseCompliance = 2,
    RouteCompliance = 3,
    EnvironmentalCompliance = 4,
    SafetyCompliance = 5,
    OperationalCompliance = 6,
    FinancialCompliance = 7,
    DataPrivacyCompliance = 8,
    QualityCompliance = 9,
    CustomCompliance = 10
}

public enum ComplianceStatus
{
    Pending = 1,
    InProgress = 2,
    UnderReview = 3,
    Compliant = 4,
    NonCompliant = 5,
    PartiallyCompliant = 6,
    RequiresAction = 7,
    Escalated = 8,
    Resolved = 9,
    Archived = 10
}

public enum CompliancePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Emergency = 5
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Extreme = 5
}