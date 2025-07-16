using ProductService.Core.Entities;

namespace ProductService.Core.DTOs;

public class SpecificationReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    
    // Specification Details
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Value Information
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? DataType { get; set; }
    
    // Numeric Values
    public decimal? NumericValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // Quality Standards
    public string? QualityStandard { get; set; }
    public string? TestMethod { get; set; }
    public string? TestConditions { get; set; }
    public DateTime? LastTested { get; set; }
    public DateTime? NextTestDue { get; set; }
    
    // Compliance Information
    public bool IsComplianceRequired { get; set; }
    public string? ComplianceStandard { get; set; }
    public string? CertificationNumber { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty;
    
    // Regulatory Information
    public string? RegulatoryBody { get; set; }
    public string? RegulationReference { get; set; }
    public bool IsMandatory { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    
    // Documentation
    public string? DocumentUrl { get; set; }
    public string? CertificateUrl { get; set; }
    public string? TestReportUrl { get; set; }
    
    // Validation Rules
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; }
    public bool IsEditable { get; set; }
    public int DisplayOrder { get; set; }
    
    // Approval Workflow
    public bool RequiresApproval { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Helper Properties
    public bool IsCompliant { get; set; }
    public bool IsCertificationExpired { get; set; }
    public bool IsTestDue { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSpecificationDto
{
    public string ProductId { get; set; } = string.Empty;
    
    // Specification Details
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpecificationType Type { get; set; } = SpecificationType.Technical;
    public string Category { get; set; } = string.Empty;
    
    // Value Information
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? DataType { get; set; } = "string";
    
    // Numeric Values
    public decimal? NumericValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // Quality Standards
    public string? QualityStandard { get; set; }
    public string? TestMethod { get; set; }
    public string? TestConditions { get; set; }
    public DateTime? LastTested { get; set; }
    public DateTime? NextTestDue { get; set; }
    
    // Compliance Information
    public bool IsComplianceRequired { get; set; } = false;
    public string? ComplianceStandard { get; set; }
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
    public SpecificationStatus Status { get; set; } = SpecificationStatus.Active;
}

public class UpdateSpecificationDto
{
    // Specification Details
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SpecificationType Type { get; set; } = SpecificationType.Technical;
    public string Category { get; set; } = string.Empty;
    
    // Value Information
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? DataType { get; set; } = "string";
    
    // Numeric Values
    public decimal? NumericValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // Quality Standards
    public string? QualityStandard { get; set; }
    public string? TestMethod { get; set; }
    public string? TestConditions { get; set; }
    public DateTime? LastTested { get; set; }
    public DateTime? NextTestDue { get; set; }
    
    // Compliance Information
    public bool IsComplianceRequired { get; set; } = false;
    public string? ComplianceStandard { get; set; }
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
    public SpecificationStatus Status { get; set; } = SpecificationStatus.Active;
}public
 class SpecificationReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; }
    public string Status { get; set; } = string.Empty;
}