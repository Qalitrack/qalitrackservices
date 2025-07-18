using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ComplianceService.Core.Entities;

public class Reporting : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ReportName { get; set; } = string.Empty;
    
    [Required]
    public ReportType ReportType { get; set; }
    
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? RegulatoryId { get; set; }
    
    [StringLength(100)]
    public string? RegulatoryBody { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    // Report Periods
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
    
    public DateTime? GeneratedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    [StringLength(50)]
    public string? GeneratedBy { get; set; }
    
    [StringLength(100)]
    public string? GeneratedByName { get; set; }
    
    [StringLength(50)]
    public string? SubmittedBy { get; set; }
    
    [StringLength(100)]
    public string? SubmittedByName { get; set; }
    
    // Report Content
    public int TotalComplianceChecks { get; set; } = 0;
    public int CompliantItems { get; set; } = 0;
    public int NonCompliantItems { get; set; } = 0;
    public int PartiallyCompliantItems { get; set; } = 0;
    
    public decimal CompliancePercentage { get; set; } = 0;
    public decimal RiskScore { get; set; } = 0;
    
    public int TotalViolations { get; set; } = 0;
    public int CriticalViolations { get; set; } = 0;
    public int ResolvedViolations { get; set; } = 0;
    
    // Financial Impact
    public decimal? TotalPenalties { get; set; }
    public decimal? PotentialPenalties { get; set; }
    public decimal? CostOfCompliance { get; set; }
    
    [StringLength(10)]
    public string Currency { get; set; } = "KES";
    
    // Recommendations and Actions
    [StringLength(2000)]
    public string? ExecutiveSummary { get; set; }
    
    [StringLength(2000)]
    public string? KeyFindings { get; set; }
    
    [StringLength(2000)]
    public string? Recommendations { get; set; }
    
    [StringLength(1000)]
    public string? ActionPlan { get; set; }
    
    // External Submission
    public bool RequiresExternalSubmission { get; set; } = false;
    
    [StringLength(200)]
    public string? SubmissionMethod { get; set; } // API, Portal, Email, Manual
    
    [StringLength(100)]
    public string? ExternalReferenceNumber { get; set; }
    
    [StringLength(500)]
    public string? SubmissionUrl { get; set; }
    
    public DateTime? ExternalAcknowledgmentDate { get; set; }
    
    [StringLength(1000)]
    public string? SubmissionNotes { get; set; }
    
    // File Attachments
    [StringLength(500)]
    public string? ReportFilePath { get; set; }
    
    [StringLength(100)]
    public string? ReportFileFormat { get; set; } // PDF, XLSX, XML, JSON
    
    public long? ReportFileSize { get; set; }
    
    [StringLength(64)]
    public string? ReportFileChecksum { get; set; }
    
    // JSON properties for flexible data storage
    public string? ReportDataJson { get; set; }
    public string? MetricsJson { get; set; }
    public string? AttachmentsJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? ReportData
    {
        get => string.IsNullOrEmpty(ReportDataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ReportDataJson);
        set => ReportDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<ComplianceMetric>? Metrics
    {
        get => string.IsNullOrEmpty(MetricsJson) ? null : JsonConvert.DeserializeObject<List<ComplianceMetric>>(MetricsJson);
        set => MetricsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<ReportAttachment>? Attachments
    {
        get => string.IsNullOrEmpty(AttachmentsJson) ? null : JsonConvert.DeserializeObject<List<ReportAttachment>>(AttachmentsJson);
        set => AttachmentsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Regulatory? Regulatory { get; set; }
    public virtual ICollection<Compliance> ComplianceRecords { get; set; } = new List<Compliance>();
}

public class ComplianceMetric
{
    public string MetricName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CalculatedDate { get; set; } = DateTime.UtcNow;
}

public class ReportAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }
}

public enum ReportType
{
    ComplianceAssessment = 1,
    ViolationSummary = 2,
    RegulatorySubmission = 3,
    AuditReport = 4,
    RiskAssessment = 5,
    PerformanceMetrics = 6,
    TrendAnalysis = 7,
    ExecutiveDashboard = 8,
    DetailedAnalysis = 9,
    CustomReport = 10
}

public enum ReportStatus
{
    Draft = 1,
    InProgress = 2,
    Generated = 3,
    UnderReview = 4,
    Approved = 5,
    Submitted = 6,
    Acknowledged = 7,
    Rejected = 8,
    Archived = 9
}