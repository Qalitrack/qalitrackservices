namespace SupplierService.Core.Entities;

public class SupplierLocation : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; } = false;
    public string? OperatingHours { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
}

public enum LocationType
{
    Headquarters,
    Branch,
    Warehouse,
    Manufacturing,
    Office,
    ServiceCenter,
    Distribution
}