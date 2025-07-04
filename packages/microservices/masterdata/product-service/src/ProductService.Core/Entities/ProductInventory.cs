namespace ProductService.Core.Entities;

public class ProductInventory : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public string? Location { get; set; }
    public string? Warehouse { get; set; }
    public DateTime? LastStockCheck { get; set; }
    public bool TrackingEnabled { get; set; } = true;

    // Navigation properties
    public virtual Product? Product { get; set; }
}