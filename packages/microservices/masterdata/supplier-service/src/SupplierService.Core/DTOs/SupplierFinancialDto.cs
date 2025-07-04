using SupplierService.Core.Entities;

namespace SupplierService.Core.DTOs;

public class SupplierFinancialDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public decimal? AnnualRevenue { get; set; }
    public string? Currency { get; set; }
    public CreditRating? CreditRating { get; set; }
    public decimal? CreditLimit { get; set; }
    public PaymentTerms PreferredPaymentTerms { get; set; }
    public string? BankName { get; set; }
    public string? TaxId { get; set; }
    public string? VatNumber { get; set; }
    public bool TaxExempt { get; set; }
    public decimal? OutstandingBalance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public int? AveragePaymentDays { get; set; }
    public FinancialStatus Status { get; set; }
    public DateTime? LastFinancialReview { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateSupplierFinancialRequest
{
    public decimal? AnnualRevenue { get; set; }
    public string? Currency { get; set; } = "USD";
    public CreditRating? CreditRating { get; set; }
    public decimal? CreditLimit { get; set; }
    public PaymentTerms PreferredPaymentTerms { get; set; }
    public string? BankName { get; set; }
    public string? TaxId { get; set; }
    public string? VatNumber { get; set; }
    public bool TaxExempt { get; set; } = false;
    public string? Notes { get; set; }
}