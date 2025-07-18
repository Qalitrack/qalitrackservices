namespace CustomerService.Core.DTOs;

public class ContractReadDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string? PricingTerms { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PricingModel { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string? PaymentTerms { get; set; }
    public int PaymentDueDays { get; set; }
    public string? ComplianceRequirements { get; set; }
    public string? RegulatoryStandards { get; set; }
    public string? QualityStandards { get; set; }
    public string? SafetyRequirements { get; set; }
    public string? EnvironmentalRequirements { get; set; }
    public string? Terms { get; set; }
    public string? SignedByCustomer { get; set; }
    public string? SignedByCompany { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public string? RenewalNotificationEmail { get; set; }
    public string? PerformanceMetrics { get; set; }
    public string? PenaltyClause { get; set; }
    public decimal? LatePaymentPenalty { get; set; }
    public string? TerminationClause { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateContractDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string ContractType { get; set; } = "Service";
    public string? PricingTerms { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PricingModel { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string? PaymentTerms { get; set; }
    public int PaymentDueDays { get; set; } = 30;
    public string? ComplianceRequirements { get; set; }
    public string? RegulatoryStandards { get; set; }
    public string? QualityStandards { get; set; }
    public string? SafetyRequirements { get; set; }
    public string? EnvironmentalRequirements { get; set; }
    public string? Terms { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public string? RenewalNotificationEmail { get; set; }
    public string? PerformanceMetrics { get; set; }
    public string? PenaltyClause { get; set; }
    public decimal? LatePaymentPenalty { get; set; }
    public string? TerminationClause { get; set; }
}

public class UpdateContractDto
{
    public string? ContractNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public string? PricingTerms { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PricingModel { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string? PaymentTerms { get; set; }
    public int PaymentDueDays { get; set; }
    public string? ComplianceRequirements { get; set; }
    public string? RegulatoryStandards { get; set; }
    public string? QualityStandards { get; set; }
    public string? SafetyRequirements { get; set; }
    public string? EnvironmentalRequirements { get; set; }
    public string? Terms { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public string? RenewalNotificationEmail { get; set; }
    public string? PerformanceMetrics { get; set; }
    public string? PenaltyClause { get; set; }
    public decimal? LatePaymentPenalty { get; set; }
    public string? TerminationClause { get; set; }
}

public class SuspendContractDto
{
    public string? Reason { get; set; }
}

public class TerminateContractDto
{
    public string? Reason { get; set; }
}

public class RenewContractDto
{
    public DateTime NewEndDate { get; set; }
    public decimal? NewContractValue { get; set; }
    public string? RenewalTerms { get; set; }
    public string? RenewedBy { get; set; }
    public string? Notes { get; set; }
}

public class ContractRenewalReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public DateTime RenewalDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public decimal? NewContractValue { get; set; }
    public string? RenewalTerms { get; set; }
    public string? RenewedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}