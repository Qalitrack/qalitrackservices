using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicianApi.Core.Entities;

public class TripMaterial : BaseEntity
{
    [Required]
    public string TripId { get; set; } = string.Empty;

    [Required]
    public string MaterialId { get; set; } = string.Empty;

    public string? MaterialVariantId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public string? AddedByUserId { get; set; } // References user service

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Trip Trip { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;
    public virtual MaterialVariant? MaterialVariant { get; set; }
}
