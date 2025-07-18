using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ArchiveService.Core.Entities;

public class Compliance : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ArchiveId { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? RetentionPolicyId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string ComplianceName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public ComplianceType ComplianceType { get; set; }
    
    public ComplianceStatus Status { get; set; } = ComplianceStatus.Active;
    
    // Regulatory Framework Information
    [Required]
    [StringLength(100)]
    public string RegulatoryFramework { get; set; } = string.Empty; // GDPR, CCPA, SOX, HIPAA, etc.
    
    [StringLength(100)]
    public string? FrameworkVersion { get; set; }
    
    [StringLength(100)]
    public string? JurisdictionCode { get; set; } // US, EU, UK, etc.
    
    [StringLength(1000)]
    public string? ApplicableRegulations { get; set; } // JSON array of specific regulations
    
    // Compliance Requirements
    [StringLength(2000)]
    public string? ComplianceRequirements { get; set; } // JSON array of specific requirements
    
    [StringLength(1000)]
    public string? MandatoryRetentionPeriod { get; set; } // JSON object with retention periods
    
    [StringLength(1000)]
    public string? DataProtectionRequirements { get; set; } // JSON array of protection requirements
    
    [StringLength(1000)]
    public string? AccessControlRequirements { get; set; } // JSON array of access control rules
    
    public bool RequiresEncryption { get; set; } = false;
    public bool RequiresAuditTrail { get; set; } = true;
    public bool RequiresDataMasking { get; set; } = false;
    public bool RequiresGeographicRestrictions { get; set; } = false;
    
    // Legal Hold Management
    public bool IsSubjectToLegalHold { get; set; } = false;
    
    [StringLength(100)]
    public string? LegalHoldId { get; set; }
    
    [StringLength(1000)]
    public string? LegalHoldReason { get; set; }
    
    [StringLength(50)]
    public string? LegalHoldOrderedBy { get; set; }
    
    public DateTime? LegalHoldStartDate { get; set; }
    public DateTime? LegalHoldEndDate { get; set; }
    
    [StringLength(1000)]
    public string? LegalHoldScope { get; set; } // JSON object defining scope
    
    public LegalHoldStatus LegalHoldStatus { get; set; } = LegalHoldStatus.NotApplicable;
    
    // Privacy and Data Subject Rights
    public bool SupportsRightToErasure { get; set; } = false; // GDPR Article 17
    public bool SupportsRightOfAccess { get; set; } = false; // GDPR Article 15
    public bool SupportsDataPortability { get; set; } = false; // GDPR Article 20
    public bool SupportsRectification { get; set; } = false; // GDPR Article 16
    
    [StringLength(1000)]
    public string? PersonalDataCategories { get; set; } // JSON array of personal data types
    
    [StringLength(1000)]
    public string? SpecialCategoryData { get; set; } // JSON array of special category data
    
    public bool ContainsSensitiveData { get; set; } = false;
    
    // Audit and Monitoring
    public bool EnableContinuousMonitoring { get; set; } = true;
    
    [StringLength(1000)]
    public string? MonitoringMetrics { get; set; } // JSON array of metrics to monitor
    
    [StringLength(1000)]
    public string? ComplianceChecks { get; set; } // JSON array of automated checks
    
    public DateTime? LastComplianceCheck { get; set; }
    public DateTime? NextComplianceCheck { get; set; }
    
    public int ComplianceCheckFrequencyDays { get; set; } = 30;
    
    // Assessment and Scoring
    public decimal? ComplianceScore { get; set; } // 0-100 compliance score
    
    [StringLength(1000)]
    public string? ComplianceGaps { get; set; } // JSON array of identified gaps
    
    [StringLength(1000)]
    public string? RemediationActions { get; set; } // JSON array of required actions
    
    public ComplianceRiskLevel RiskLevel { get; set; } = ComplianceRiskLevel.Low;
    
    // Reporting and Documentation
    public bool RequiresRegularReporting { get; set; } = false;
    
    [StringLength(100)]
    public string? ReportingFrequency { get; set; } // Monthly, Quarterly, Annually
    
    [StringLength(1000)]
    public string? ReportingRequirements { get; set; } // JSON array of reporting requirements
    
    [StringLength(1000)]
    public string? DocumentationRequirements { get; set; } // JSON array of required documentation
    
    public DateTime? LastReportSubmitted { get; set; }
    public DateTime? NextReportDue { get; set; }
    
    // Breach and Incident Management
    public bool RequiresBreachNotification { get; set; } = false;
    
    [StringLength(100)]
    public string? BreachNotificationTimeframe { get; set; } // 72 hours for GDPR
    
    [StringLength(1000)]
    public string? BreachNotificationAuthorities { get; set; } // JSON array of authorities to notify
    
    [StringLength(1000)]
    public string? IncidentResponse { get; set; } // JSON object with incident response procedures
    
    // Cross-Border Transfer
    public bool AllowsCrossBorderTransfer { get; set; } = true;
    
    [StringLength(1000)]
    public string? ApprovedJurisdictions { get; set; } // JSON array of approved countries/regions
    
    [StringLength(1000)]
    public string? TransferMechanisms { get; set; } // JSON array of legal transfer mechanisms
    
    public bool RequiresAdequacyDecision { get; set; } = false;
    public bool RequiresStandardContractualClauses { get; set; } = false;
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? ComplianceOfficer { get; set; }
    
    [StringLength(100)]
    public string? DataProtectionOfficer { get; set; }
    
    [StringLength(100)]
    public string? LegalCounsel { get; set; }
    
    // Review and Approval
    public bool RequiresLegalReview { get; set; } = false;
    
    [StringLength(50)]
    public string? ReviewedBy { get; set; }
    
    public DateTime? ReviewDate { get; set; }
    
    [StringLength(1000)]
    public string? ReviewComments { get; set; }
    
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.NotRequired;
    
    // Certification and Standards
    [StringLength(1000)]
    public string? CertificationStandards { get; set; } // JSON array of applicable standards (ISO 27001, SOC 2, etc.)
    
    [StringLength(1000)]
    public string? CertificationDetails { get; set; } // JSON object with certification information
    
    public DateTime? CertificationExpiryDate { get; set; }
    
    // Penalties and Enforcement
    [StringLength(1000)]
    public string? PotentialPenalties { get; set; } // JSON object describing potential penalties
    
    [StringLength(1000)]
    public string? EnforcementActions { get; set; } // JSON array of possible enforcement actions
    
    public decimal? MaxFinancialPenalty { get; set; }
    
    // Training and Awareness
    public bool RequiresStaffTraining { get; set; } = false;
    
    [StringLength(1000)]
    public string? TrainingRequirements { get; set; } // JSON array of training requirements
    
    public DateTime? LastTrainingDate { get; set; }
    public DateTime? NextTrainingDue { get; set; }
    
    // Vendor and Third-Party Management
    [StringLength(1000)]
    public string? ThirdPartyRequirements { get; set; } // JSON array of third-party compliance requirements
    
    [StringLength(1000)]
    public string? VendorAssessments { get; set; } // JSON array of vendor assessment requirements
    
    public bool RequiresDataProcessingAgreements { get; set; } = false;
    
    // Technology and Security Requirements
    [StringLength(1000)]
    public string? TechnicalSafeguards { get; set; } // JSON array of required technical safeguards
    
    [StringLength(1000)]
    public string? SecurityRequirements { get; set; } // JSON array of security requirements
    
    public bool RequiresPrivacyByDesign { get; set; } = false;
    public bool RequiresDataMinimization { get; set; } = false;
    
    // Lifecycle Management
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ApplicableRegulationsList
    {
        get => string.IsNullOrEmpty(ApplicableRegulations) ? null : JsonConvert.DeserializeObject<List<string>>(ApplicableRegulations);
        set => ApplicableRegulations = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ComplianceRequirementsList
    {
        get => string.IsNullOrEmpty(ComplianceRequirements) ? null : JsonConvert.DeserializeObject<List<object>>(ComplianceRequirements);
        set => ComplianceRequirements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? MandatoryRetentionPeriodObject
    {
        get => string.IsNullOrEmpty(MandatoryRetentionPeriod) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MandatoryRetentionPeriod);
        set => MandatoryRetentionPeriod = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? DataProtectionRequirementsList
    {
        get => string.IsNullOrEmpty(DataProtectionRequirements) ? null : JsonConvert.DeserializeObject<List<object>>(DataProtectionRequirements);
        set => DataProtectionRequirements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AccessControlRequirementsList
    {
        get => string.IsNullOrEmpty(AccessControlRequirements) ? null : JsonConvert.DeserializeObject<List<object>>(AccessControlRequirements);
        set => AccessControlRequirements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? LegalHoldScopeObject
    {
        get => string.IsNullOrEmpty(LegalHoldScope) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(LegalHoldScope);
        set => LegalHoldScope = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? PersonalDataCategoriesList
    {
        get => string.IsNullOrEmpty(PersonalDataCategories) ? null : JsonConvert.DeserializeObject<List<string>>(PersonalDataCategories);
        set => PersonalDataCategories = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? SpecialCategoryDataList
    {
        get => string.IsNullOrEmpty(SpecialCategoryData) ? null : JsonConvert.DeserializeObject<List<string>>(SpecialCategoryData);
        set => SpecialCategoryData = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? MonitoringMetricsList
    {
        get => string.IsNullOrEmpty(MonitoringMetrics) ? null : JsonConvert.DeserializeObject<List<string>>(MonitoringMetrics);
        set => MonitoringMetrics = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ComplianceChecksList
    {
        get => string.IsNullOrEmpty(ComplianceChecks) ? null : JsonConvert.DeserializeObject<List<object>>(ComplianceChecks);
        set => ComplianceChecks = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ComplianceGapsList
    {
        get => string.IsNullOrEmpty(ComplianceGaps) ? null : JsonConvert.DeserializeObject<List<object>>(ComplianceGaps);
        set => ComplianceGaps = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? RemediationActionsList
    {
        get => string.IsNullOrEmpty(RemediationActions) ? null : JsonConvert.DeserializeObject<List<object>>(RemediationActions);
        set => RemediationActions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsCompliant()
    {
        return ComplianceScore >= 95m && Status == ComplianceStatus.Active;
    }
    
    public bool RequiresAttention()
    {
        return RiskLevel >= ComplianceRiskLevel.High ||
               ComplianceScore < 80m ||
               (NextComplianceCheck.HasValue && NextComplianceCheck < DateTime.UtcNow) ||
               (NextReportDue.HasValue && NextReportDue < DateTime.UtcNow.AddDays(7));
    }
    
    public bool IsExpired()
    {
        return ExpirationDate.HasValue && ExpirationDate < DateTime.UtcNow;
    }
    
    public TimeSpan? GetTimeSinceLastCheck()
    {
        return LastComplianceCheck.HasValue ? DateTime.UtcNow - LastComplianceCheck.Value : null;
    }
    
    public bool IsOverdueForCheck()
    {
        return NextComplianceCheck.HasValue && NextComplianceCheck < DateTime.UtcNow;
    }
    
    public bool IsOverdueForReporting()
    {
        return NextReportDue.HasValue && NextReportDue < DateTime.UtcNow;
    }
    
    // Navigation Properties
    public virtual Archive Archive { get; set; } = null!;
    public virtual RetentionPolicy? RetentionPolicy { get; set; }
}

public enum ComplianceType
{
    DataProtection = 1,
    Privacy = 2,
    Financial = 3,
    Healthcare = 4,
    Industry = 5,
    Government = 6,
    International = 7,
    Custom = 8
}

public enum ComplianceStatus
{
    Active = 1,
    UnderReview = 2,
    NonCompliant = 3,
    Remediation = 4,
    Suspended = 5,
    Expired = 6
}

public enum LegalHoldStatus
{
    NotApplicable = 1,
    Active = 2,
    Released = 3,
    PartiallyReleased = 4,
    Challenged = 5,
    Expired = 6
}

public enum ComplianceRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ReviewStatus
{
    NotRequired = 1,
    Pending = 2,
    InProgress = 3,
    Completed = 4,
    RequiresUpdate = 5
}