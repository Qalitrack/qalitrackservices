using System.ComponentModel.DataAnnotations;

namespace SupplierService.Core.DTOs;

public class CreateSupplierPricingDto
{
    public string Type { get; set; } = "Standard";

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    [Range(1, int.MaxValue)]
    public int MinQuantity { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int? MaxQuantity { get; set; }

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int Priority { get; set; } = 0;

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? DiscountAmount { get; set; }

    [StringLength(200)]
    public string Notes { get; set; } = string.Empty;
}