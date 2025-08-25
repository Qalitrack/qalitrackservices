namespace SupplierService.Core.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public SupplierType SupplierType { get; set; }
    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public string Email { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    public SupplierType Type { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Industry { get; set; }

    // Navigation Properties
    public virtual ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();
    public virtual ICollection<SupplierContract> Contracts { get; set; } = new List<SupplierContract>();
    public virtual ICollection<SupplierProduct> Products { get; set; } = new List<SupplierProduct>();
    public virtual ICollection<SupplierLocation> Locations { get; set; } = new List<SupplierLocation>();
    public virtual ICollection<SupplierDocument> Documents { get; set; } = new List<SupplierDocument>();
    public virtual ICollection<Procurement> Procurements { get; set; } = new List<Procurement>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<SupplierPerformance> PerformanceMetrics { get; set; } = new List<SupplierPerformance>();
    public virtual SupplierPerformance? Performance { get; set; }
    public virtual SupplierFinancial? Financial { get; set; }
}

public enum SupplierType
{
    Manufacturer,
    Distributor,
    Retailer,
    ServiceProvider,
    Contractor,
    Consultant
}

public enum SupplierStatus
{
    Active,
    Inactive,
    Pending,
    Suspended,
    Blacklisted
}