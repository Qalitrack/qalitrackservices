namespace SupplierService.Core.Entities;

public class Address : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public AddressType Type { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? AdditionalInfo { get; set; }
    
    // Navigation property
    public virtual Supplier? Supplier { get; set; }
}

public enum AddressType
{
    Billing,
    Shipping,
    Office,
    Warehouse
}