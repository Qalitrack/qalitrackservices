namespace TechnicianApi.Core.DTOs.Refund;

public class CreateRefundDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReferenceNumber { get; set; }
}
