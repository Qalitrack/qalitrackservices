using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicianApi.Core.Entities;

public class Expense : BaseEntity
{
    [Required]
    public string TripId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public string? ReceiptPhotoUrl { get; set; }

    public string? UserId { get; set; } // References user service
    public bool Synced { get; set; } = false;

    // Navigation properties
    public virtual Trip Trip { get; set; } = null!;
    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
