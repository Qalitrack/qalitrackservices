namespace ProductService.Core.Entities;

public class Specification : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public virtual Product? Product { get; set; }
    
    // Specification Details
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpecificationType Type { get; set; } = SpecificationType.Technical;
    public string Category { get; set; } = string.Empty; // e.g., "Physical", "Performance", "Safety"
    
    // Value Information
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? DataType { get; set; } = "string"; // string, number, boolean, date, etc.
    
    // Numeric Values (for calculations and comparisons)
    public decimal? NumericValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // Quality Standards
    public string? QualityStandard { get; set; } // e.g., "ISO 9001", "ASTM D1234"
    public string? TestMethod { get; set; }
    public string? TestConditions { get; set; }
    public DateTime? LastTested { get; set; }
    public DateTime? NextTestDue { get; set; }
    
    // Compliance Information
    public bool IsComplianceRequired { get; set; } = false;
    public string? ComplianceStandard { get; set; } // e.g., "FDA", "CE", "UL"
    public string? CertificationNumber { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public ComplianceStatus ComplianceStatus { get; set; } = ComplianceStatus.NotRequired;
    
    // Regulatory Information
    public string? RegulatoryBody { get; set; }
    public string? RegulationReference { get; set; }
    public bool IsMandatory { get; set; } = false;
    public string? Country { get; set; }
    public string? Region { get; set; }
    
    // Documentation
    public string? DocumentUrl { get; set; }
    public string? CertificateUrl { get; set; }
    public string? TestReportUrl { get; set; }
    
    // Validation Rules
    public bool IsRequired { get; set; } = false;
    public bool IsVisible { get; set; } = true;
    public bool IsEditable { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    // Approval Workflow
    public bool RequiresApproval { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public SpecificationStatus Status { get; set; } = SpecificationStatus.Active;
    
    // Helper Properties
    public bool IsCompliant => ComplianceStatus == ComplianceStatus.Compliant;
    public bool IsCertificationExpired => CertificationExpiry.HasValue && CertificationExpiry.Value < DateTime.UtcNow;
    public bool IsTestDue => NextTestDue.HasValue && NextTestDue.Value <= DateTime.UtcNow;
}

public enum SpecificationType
{
    Technical,
    Physical,
    Performance,
    Safety,
    Environmental,
    Quality,
    Compliance,
    Regulatory,
    Certification,
    Material,
    Chemical,
    Electrical,
    Mechanical
}

public enum ComplianceStatus
{
    NotRequired,
    Pending,
    InProgress,
    Compliant,
    NonCompliant,
    Expired,
    UnderReview
}

public enum SpecificationStatus
{
    Active,
    Inactive,
    Draft,
    UnderReview,
    Approved,
    Rejected,
    Archived
}