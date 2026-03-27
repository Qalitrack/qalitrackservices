namespace TechnicianApi.Core.DTOs.Trip;

public class CreateReceiptDto
{
    public string ExpenseId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Note { get; set; }
}
