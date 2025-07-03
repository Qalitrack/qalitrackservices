namespace SupplierService.Core.Entities;

public class SupplierFinancial : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public decimal? AnnualRevenue { get; set; }
    public string? Currency { get; set; } = "USD";
    public CreditRating? CreditRating { get; set; }
    public decimal? CreditLimit { get; set; }
    public PaymentTerms PreferredPaymentTerms { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string? BankAddress { get; set; }
    public string? SwiftCode { get; set; }
    public string? TaxId { get; set; }
    public string? VatNumber { get; set; }
    public bool TaxExempt { get; set; } = false;
    public string? TaxExemptionCertificate { get; set; }
    public decimal? OutstandingBalance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public int? AveragePaymentDays { get; set; }
    public FinancialStatus Status { get; set; } = FinancialStatus.Good;
    public DateTime? LastFinancialReview { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
}

public enum CreditRating
{
    Excellent,
    Good,
    Fair,
    Poor,
    NotRated
}

public enum FinancialStatus
{
    Excellent,
    Good,
    Fair,
    Poor,
    UnderReview,
    Suspended
}