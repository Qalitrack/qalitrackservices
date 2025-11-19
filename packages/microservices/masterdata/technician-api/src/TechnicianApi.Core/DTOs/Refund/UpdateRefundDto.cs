namespace TechnicianApi.Core.DTOs.Refund;

public class UpdateRefundDto
{
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReferenceNumber { get; set; }
}
