namespace SupplierService.Core.Entities;

public class SupplierContract : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public decimal? ContractValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public PaymentTerms PaymentTerms { get; set; }
    public int? PaymentDays { get; set; }
    public string? Terms { get; set; }
    public string? Conditions { get; set; }
    public bool AutoRenewal { get; set; } = false;
    public int? RenewalPeriodMonths { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
}

public enum ContractType
{
    Service,
    Supply,
    Maintenance,
    Consulting,
    Framework,
    OneTime
}

public enum ContractStatus
{
    Draft,
    UnderReview,
    Approved,
    Active,
    Expired,
    Terminated,
    Cancelled
}

public enum PaymentTerms
{
    Net15,
    Net30,
    Net45,
    Net60,
    Net90,
    Immediate,
    COD,
    AdvancePayment
}