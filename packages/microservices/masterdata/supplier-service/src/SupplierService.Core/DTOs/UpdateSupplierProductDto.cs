using System.ComponentModel.DataAnnotations;

namespace SupplierService.Core.DTOs;

public class UpdateSupplierProductDto
{
    [StringLength(100)]
    public string? SupplierSKU { get; set; }

    [Range(0, int.MaxValue)]
    public int? LeadTime { get; set; }

    [Range(1, int.MaxValue)]
    public int? MinOrderQuantity { get; set; }

    public bool? IsPreferred { get; set; }

    public string? Status { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Range(0, int.MaxValue)]
    public int? ReorderLevel { get; set; }

    [Range(0, int.MaxValue)]
    public int? CurrentStock { get; set; }

    public DateTime? NextDeliveryDate { get; set; }
}