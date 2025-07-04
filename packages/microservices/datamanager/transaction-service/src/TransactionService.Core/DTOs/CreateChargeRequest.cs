using TransactionService.Core.Entities;

namespace TransactionService.Core.DTOs;

public class CreateChargeRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public ChargeType ChargeType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? TaxRate { get; set; }
    public string Currency { get; set; } = "USD";
}