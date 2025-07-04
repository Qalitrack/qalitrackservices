using CustomerService.Core.Entities;

namespace CustomerService.Core.DTOs;

public class CustomerBillingDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string BillingContactName { get; set; } = string.Empty;
    public string BillingEmail { get; set; } = string.Empty;
    public string? BillingPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? TaxExemptNumber { get; set; }
    public bool IsTaxExempt { get; set; }
    public string? Currency { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public bool AutomaticBilling { get; set; }
    public int? BillingCycleDay { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateCustomerBillingRequest
{
    public string BillingContactName { get; set; } = string.Empty;
    public string BillingEmail { get; set; } = string.Empty;
    public string? BillingPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? TaxExemptNumber { get; set; }
    public bool IsTaxExempt { get; set; }
    public string? Currency { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public bool AutomaticBilling { get; set; }
    public int? BillingCycleDay { get; set; }
}