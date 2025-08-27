using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Relationships.Entities;

/// <summary>
/// Driver-SACCO membership relationship
/// Addresses the missing relationship between drivers and SACCO organizations
/// </summary>
public class DriverSaccoMembership : BaseEntity
{
    public Guid DriverId { get; set; }
    public Guid SaccoId { get; set; }
    public DateTime MembershipDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string MembershipNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal ShareContribution { get; set; }
    public decimal? MonthlyContribution { get; set; }
    public string? MembershipType { get; set; }
    public string? Benefits { get; set; } // JSON array of membership benefits
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Driver Driver { get; set; } = null!;
    // public virtual Sacco Sacco { get; set; } = null!;
}

/// <summary>
/// Vehicle-Transporter ownership relationship
/// Addresses the missing relationship for fleet management
/// </summary>
public class VehicleTransporterOwnership : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Guid TransporterProfileId { get; set; } // References BusinessEntity with TransporterProfile
    public DateTime OwnershipStartDate { get; set; }
    public DateTime? OwnershipEndDate { get; set; }
    public string OwnershipType { get; set; } = "Active";
    public decimal? PurchasePrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? FinancingDetails { get; set; }
    public string? InsuranceDetails { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Vehicle Vehicle { get; set; } = null!;
    // public virtual TransporterProfile TransporterProfile { get; set; } = null!;
}

/// <summary>
/// Driver-Vehicle assignment relationship
/// Addresses the missing relationship for current and historical vehicle assignments
/// </summary>
public class DriverVehicleAssignment : BaseEntity
{
    public Guid DriverId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? UnassignedDate { get; set; }
    public bool IsPrimary { get; set; } = false; // Primary driver for the vehicle
    public bool IsActive { get; set; } = true;
    public string AssignmentType { get; set; } = "Active";
    public string? Reason { get; set; } // Reason for assignment/unassignment
    public string AssignedBy { get; set; } = string.Empty;
    public string? UnassignedBy { get; set; }
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Driver Driver { get; set; } = null!;
    // public virtual Vehicle Vehicle { get; set; } = null!;
}

/// <summary>
/// Driver-Transporter employment relationship
/// Addresses the missing relationship for employment tracking
/// </summary>
public class DriverTransporterEmployment : BaseEntity
{
    public Guid DriverId { get; set; }
    public Guid TransporterProfileId { get; set; } // References BusinessEntity with TransporterProfile
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string EmploymentType { get; set; } = "Active";
    public string Status { get; set; } = "Active";
    public decimal? Salary { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string? ReportsTo { get; set; }
    public string? TerminationReason { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Driver Driver { get; set; } = null!;
    // public virtual TransporterProfile TransporterProfile { get; set; } = null!;
}

/// <summary>
/// Product-Supplier catalog relationship
/// Addresses the missing many-to-many relationship between products and suppliers
/// </summary>
public class ProductSupplierCatalog : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid SupplierProfileId { get; set; } // References BusinessEntity with SupplierProfile
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinOrderQuantity { get; set; } = 1;
    public int? MaxOrderQuantity { get; set; }
    public int LeadTimeDays { get; set; } = 7;
    public bool IsPreferred { get; set; } = false;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? ProductCode { get; set; } // Supplier's product code
    public string? Description { get; set; } // Supplier's product description
    public string? QualityRating { get; set; }
    public decimal? Discount { get; set; }
    public string? PaymentTerms { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Product Product { get; set; } = null!;
    // public virtual SupplierProfile SupplierProfile { get; set; } = null!;
}

/// <summary>
/// Route-Weighbridge association relationship
/// Addresses the missing relationship between routes and weighbridges
/// </summary>
public class RouteWeighbridgeAssociation : BaseEntity
{
    public Guid RouteId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public string AssociationType { get; set; } = "Active";
    public int SequenceOrder { get; set; } = 1; // Order in the route
    public bool IsMandatory { get; set; } = true; // Must stop at this weighbridge
    public bool IsActive { get; set; } = true;
    public string? Instructions { get; set; } // Special instructions for this stop
    public int? EstimatedDurationMinutes { get; set; }
    public decimal? Distance { get; set; } // Distance from previous point
    public string? Conditions { get; set; } // JSON array of conditions/restrictions
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Route Route { get; set; } = null!;
    // public virtual Weighbridge Weighbridge { get; set; } = null!;
}

/// <summary>
/// Organization-Weighbridge ownership relationship
/// Addresses the missing relationship for asset management
/// </summary>
public class OrganizationWeighbridgeOwnership : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public DateTime OwnershipStartDate { get; set; }
    public DateTime? OwnershipEndDate { get; set; }
    public string OwnershipType { get; set; } = "Active";
    public decimal? PurchasePrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? MaintenanceContract { get; set; }
    public string? InsuranceDetails { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Organization Organization { get; set; } = null!;
    // public virtual Weighbridge Weighbridge { get; set; } = null!;
}

/// <summary>
/// Vehicle-SACCO registration relationship
/// Addresses the missing relationship for regulatory compliance
/// </summary>
public class VehicleSaccoRegistration : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Guid SaccoId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal RegistrationFee { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Conditions { get; set; } // JSON array of registration conditions
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual Vehicle Vehicle { get; set; } = null!;
    // public virtual Sacco Sacco { get; set; } = null!;
}

/// <summary>
/// User-Organization-Role matrix
/// Addresses the missing relationship for comprehensive access control
/// </summary>
public class UserOrganizationRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string AssignedBy { get; set; } = string.Empty;
    public string? Scope { get; set; } // JSON array of scope limitations
    public string? Permissions { get; set; } // JSON array of specific permissions
    public string? Notes { get; set; }

    // Navigation properties will be resolved via the DbContext configuration
    // public virtual User User { get; set; } = null!;
    // public virtual Organization Organization { get; set; } = null!;
    // public virtual Role Role { get; set; } = null!;
}

// Enumerations for relationship entities
public enum MembershipStatus
{
    Active,
    Inactive,
    Suspended,
    Expired,
    Cancelled
}

public enum OwnershipType
{
    Owned,
    Leased,
    Rented,
    Financed,
    Contracted
}

public enum AssignmentType
{
    Regular,
    Temporary,
    Emergency,
    Training,
    Backup
}

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Temporary,
    Consultant
}

public enum EmploymentStatus
{
    Active,
    Inactive,
    OnLeave,
    Suspended,
    Terminated
}

public enum AssociationType
{
    Entry,
    Exit,
    Intermediate,
    Optional,
    Emergency
}

public enum RegistrationStatus
{
    Active,
    Expired,
    Suspended,
    Cancelled,
    Pending
}