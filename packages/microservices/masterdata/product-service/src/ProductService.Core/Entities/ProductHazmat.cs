namespace ProductService.Core.Entities;

public class ProductHazmat : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string HazmatClass { get; set; } = string.Empty;
    public string? PackingGroup { get; set; }
    public string? UnNumber { get; set; }
    public string? ProperShippingName { get; set; }
    public string? HazardLabels { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? TransportRequirements { get; set; }
    public string? StorageRequirements { get; set; }
    public string? HandlingInstructions { get; set; }
    public string? EmergencyProcedures { get; set; }
    public bool RequiresPermit { get; set; }
    public DateTime? CertificationExpiry { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
}