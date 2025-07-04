using CustomerService.Core.Entities;

namespace CustomerService.Core.DTOs;

public class CustomerCreditDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal UsedCredit { get; set; }
    public CreditStatus CreditStatus { get; set; }
    public int CreditScore { get; set; }
    public DateTime? LastCreditCheck { get; set; }
    public DateTime? NextCreditReview { get; set; }
    public string? CreditTerms { get; set; }
    public PaymentHistoryDto PaymentHistory { get; set; } = new();
    public bool RequiresApproval { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public string? CreditReference1 { get; set; }
    public string? CreditReference2 { get; set; }
    public string? CreditReference3 { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PaymentHistoryDto
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

public class UpdateCustomerCreditRequest
{
    public decimal CreditLimit { get; set; }
    public CreditStatus CreditStatus { get; set; }
    public int CreditScore { get; set; }
    public DateTime? NextCreditReview { get; set; }
    public string? CreditTerms { get; set; }
    public bool RequiresApproval { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public string? CreditReference1 { get; set; }
    public string? CreditReference2 { get; set; }
    public string? CreditReference3 { get; set; }
    public string? Notes { get; set; }
}