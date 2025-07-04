namespace CustomerService.Core.Entities;

public class CustomerCredit : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal UsedCredit { get; set; }
    public CreditStatus CreditStatus { get; set; } = CreditStatus.Good;
    public int CreditScore { get; set; }
    public DateTime? LastCreditCheck { get; set; }
    public DateTime? NextCreditReview { get; set; }
    public string? CreditTerms { get; set; }
    public PaymentHistory PaymentHistory { get; set; } = new();
    public bool RequiresApproval { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public string? CreditReference1 { get; set; }
    public string? CreditReference2 { get; set; }
    public string? CreditReference3 { get; set; }
    public string? Notes { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}

public enum CreditStatus
{
    Excellent,
    Good,
    Fair,
    Poor,
    NoCredit,
    Suspended,
    UnderReview
}

public class PaymentHistory
{
    public int TotalInvoices { get; set; }
    public int OnTimePayments { get; set; }
    public int LatePayments { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public int AverageDaysToPayment { get; set; }
    public DateTime? OldestOutstandingInvoice { get; set; }
}