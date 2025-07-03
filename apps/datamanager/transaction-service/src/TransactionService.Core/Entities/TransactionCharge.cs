namespace TransactionService.Core.Entities;

public class TransactionCharge : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public ChargeType ChargeType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsApproved { get; set; } = false;
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation Properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
}