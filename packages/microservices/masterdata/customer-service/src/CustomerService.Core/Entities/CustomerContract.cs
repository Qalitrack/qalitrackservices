namespace CustomerService.Core.Entities;

public class CustomerContract : BaseEntity
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
    public string? Terms { get; set; }
    public string? SignedByCustomer { get; set; }
    public string? SignedByCompany { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}

public enum ContractStatus
{
    Draft,
    Pending,
    Active,
    Expired,
    Terminated,
    Suspended
}

public enum ContractType
{
    Service,
    Product,
    Maintenance,
    Subscription,
    OnTime,
    Framework
}