namespace TechnicianApi.Core.DTOs.Trip;

public class ExpenseResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string TripId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReceiptPhotoUrl { get; set; }
    public string? UserId { get; set; }
    public bool Synced { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
