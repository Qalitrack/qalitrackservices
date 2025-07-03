namespace TransporterService.Core.Entities;

public enum ContractType
{
    Service,
    Lease,
    Maintenance,
    Supply,
    Partnership,
    Exclusive,
    NonExclusive
}

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    Terminated,
    Suspended,
    UnderReview,
    Renewed
}

public class TransporterContract : BaseEntity
{
    public string TransporterId { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public ContractType ContractType { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientContactPerson { get; set; }
    public string? ClientPhone { get; set; }
    public string? ClientEmail { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string? PaymentTerms { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public string? ServiceDescription { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? PerformanceMetrics { get; set; }
    public string? PenaltyClause { get; set; }
    public bool AutoRenewal { get; set; } = false;
    public int? RenewalPeriodMonths { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Transporter Transporter { get; set; } = null!;
}