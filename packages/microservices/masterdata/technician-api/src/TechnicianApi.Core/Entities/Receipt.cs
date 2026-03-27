using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class Receipt : BaseEntity
{
    [Required]
    public string ExpenseId { get; set; } = string.Empty;

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Note { get; set; }

    // OCR extracted data stored as JSON
    public string? ReceiptDetailsJson { get; set; }

    public string? UserId { get; set; } // References user service
    public bool Synced { get; set; } = false;

    // Navigation properties
    public virtual Expense Expense { get; set; } = null!;
}
