namespace CustomerService.Core.Entities;

public class CustomerBilling : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string BillingContactName { get; set; } = string.Empty;
    public string BillingEmail { get; set; } = string.Empty;
    public string? BillingPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;
    public string? PreferredPaymentMethod { get; set; }
    public string? TaxExemptNumber { get; set; }
    public bool IsTaxExempt { get; set; }
    public string? Currency { get; set; } = "USD";
    public decimal? DiscountPercentage { get; set; }
    public bool AutomaticBilling { get; set; }
    public int? BillingCycleDay { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}

public enum PaymentTerms
{
    Immediate,
    Net15,
    Net30,
    Net45,
    Net60,
    Net90,
    COD,
    PrepaymentRequired
}