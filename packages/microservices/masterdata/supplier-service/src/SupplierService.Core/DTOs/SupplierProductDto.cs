namespace SupplierService.Core.DTOs;

public class SupplierProductDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty; // From Product Service
    public string SupplierSKU { get; set; } = string.Empty;
    public int LeadTime { get; set; } // In days
    public int MinOrderQuantity { get; set; }
    public bool IsPreferred { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public int CurrentStock { get; set; }
    public DateTime? LastRestockDate { get; set; }
    public DateTime? NextDeliveryDate { get; set; }
    public decimal? CurrentPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}