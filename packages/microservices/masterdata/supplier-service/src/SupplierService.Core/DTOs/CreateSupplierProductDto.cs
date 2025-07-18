using System.ComponentModel.DataAnnotations;

namespace SupplierService.Core.DTOs;

public class CreateSupplierProductDto
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [StringLength(100)]
    public string SupplierSKU { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int LeadTime { get; set; } = 0; // In days

    [Range(1, int.MaxValue)]
    public int MinOrderQuantity { get; set; } = 1;

    public bool IsPreferred { get; set; } = false;

    public string Status { get; set; } = "Available";

    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int CurrentStock { get; set; } = 0;

    public DateTime? NextDeliveryDate { get; set; }
}