namespace CustomerService.Core.Entities;

public class Contract : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public ContractType ContractType { get; set; }
    
    // Pricing terms
    public string? PricingTerms { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PricingModel { get; set; } // Fixed, Variable, Tiered, etc.
    public decimal? DiscountPercentage { get; set; }
    public string? PaymentTerms { get; set; }
    public int PaymentDueDays { get; set; } = 30;
    
    // Compliance requirements
    public string? ComplianceRequirements { get; set; }
    public string? RegulatoryStandards { get; set; }
    public string? QualityStandards { get; set; }
    public string? SafetyRequirements { get; set; }
    public string? EnvironmentalRequirements { get; set; }
    
    // Contract management
    public string? Terms { get; set; }
    public string? SignedByCustomer { get; set; }
    public string? SignedByCompany { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public string? RenewalNotificationEmail { get; set; }
    
    // Performance and penalties
    public string? PerformanceMetrics { get; set; }
    public string? PenaltyClause { get; set; }
    public decimal? LatePaymentPenalty { get; set; }
    public string? TerminationClause { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<ContractRenewal> Renewals { get; set; } = new List<ContractRenewal>();
}

public class ContractRenewal : BaseEntity
{
    public string ContractId { get; set; } = string.Empty;
    public DateTime RenewalDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public decimal? NewContractValue { get; set; }
    public string? RenewalTerms { get; set; }
    public string? RenewedBy { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Contract Contract { get; set; } = null!;
}

public enum ContractStatus
{
    Draft,
    Pending,
    Active,
    Expired,
    Terminated,
    Suspended,
    UnderReview
}

public enum ContractType
{
    Service,
    Product,
    Maintenance,
    Subscription,
    OneTime,
    Framework,
    Supply,
    Transportation
}