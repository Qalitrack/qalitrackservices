namespace TechnicianApi.Core.DTOs.Trip;

public class CreateExpenseDto
{
    public string TripId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReceiptPhotoUrl { get; set; }
}
