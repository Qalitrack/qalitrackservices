namespace OrganizationService.Core.Entities;

public class OrganizationBilling : BaseEntity
{
    public string OrganizationId { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingEmail { get; set; } = string.Empty;
    public string? BillingPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    public string? VatNumber { get; set; }
    public string? TaxId { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public BillingStatus Status { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
}

public enum BillingCycle
{
    Monthly = 0,
    Quarterly = 1,
    Annually = 2,
    PayAsYouGo = 3
}

public enum BillingStatus
{
    Active = 0,
    Suspended = 1,
    Overdue = 2,
    Cancelled = 3
}