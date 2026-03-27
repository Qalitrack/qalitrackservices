using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicianApi.Core.Entities;

public class MaterialCost : BaseEntity
{
    [Required]
    public string MaterialId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public string? UserId { get; set; } // References user service
    public bool Synced { get; set; } = false;

    // Navigation properties
    public virtual Material Material { get; set; } = null!;
}
