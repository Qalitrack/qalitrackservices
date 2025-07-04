using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class ProductCatalog : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string ProductId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Category { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string Unit { get; set; }
    
    [Required]
    public decimal CurrentPrice { get; set; }
    
    [Required]
    public decimal AvailableQuantity { get; set; }
    
    [Required]
    public decimal ReorderLevel { get; set; }
    
    [Required]
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    
    [Required]
    public DateTime LastSyncDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiryDate { get; set; }
    
    [MaxLength(1000)]
    public string? ComplianceRequirements { get; set; }
    
    [MaxLength(1000)]
    public string? QualitySpecifications { get; set; }
    
    [MaxLength(200)]
    public string? SupplierIds { get; set; }
    
    [MaxLength(100)]
    public string? MasterDataVersion { get; set; }
    
    public bool RequiresSpecialHandling { get; set; } = false;
    
    public decimal MinOrderQuantity { get; set; } = 0;
    
    public decimal MaxOrderQuantity { get; set; } = 0;
    
    public List<string> AllowedVehicleTypes { get; set; } = new();
    
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

public enum ProductStatus
{
    Active,
    Inactive,
    Discontinued,
    OutOfStock,
    Restricted
}