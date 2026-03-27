namespace TechnicianApi.Core.DTOs.Trip;

public class ReceiptResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ExpenseId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? ReceiptDetailsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
