using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Vehicle.Entities;

public class Vehicle : BaseEntity
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty; // Vehicle Identification Number
    public string EngineNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty; // Diesel, Petrol, Electric, Hybrid
    public decimal MaxWeight { get; set; }
    public decimal TareWeight { get; set; }
    public decimal PayloadCapacity { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public decimal? CurrentMileage { get; set; }
    public Guid OrganizationId { get; set; }
    public string? Notes { get; set; }

    // Foreign keys
    public Guid VehicleTypeId { get; set; }

    // Navigation properties
    public virtual VehicleType VehicleType { get; set; } = null!;
    public virtual VehicleRegistration? Registration { get; set; }
    public virtual VehicleSpecification? Specification { get; set; }
    public virtual ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public virtual ICollection<VehicleInspection> Inspections { get; set; } = new List<VehicleInspection>();
    public virtual ICollection<VehicleInsurance> InsurancePolicies { get; set; } = new List<VehicleInsurance>();
    public virtual ICollection<VehicleMaintenance> MaintenanceRecords { get; set; } = new List<VehicleMaintenance>();

    // Cross-module relationships (handled via DbContext configuration)
    // public virtual ICollection<VehicleTransporterOwnership> TransporterOwnerships { get; set; } = new List<VehicleTransporterOwnership>();
    // public virtual ICollection<DriverVehicleAssignment> DriverAssignments { get; set; } = new List<DriverVehicleAssignment>();
    // public virtual ICollection<VehicleSaccoRegistration> SaccoRegistrations { get; set; } = new List<VehicleSaccoRegistration>();
}

public class VehicleType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? MaxWeightCapacity { get; set; }
    public decimal? MaxVolumeCapacity { get; set; }
    public string? FuelTypes { get; set; } // JSON array of supported fuel types
    public string? RequiredLicenseClass { get; set; }
    public bool RequiresSpecialLicense { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

public class VehicleRegistration : BaseEntity
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal? RegistrationFee { get; set; }
    public DateTime? LastRenewalDate { get; set; }
    public string? RenewalStatus { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}

public class VehicleSpecification : BaseEntity
{
    public Guid VehicleId { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Wheelbase { get; set; }
    public decimal GroundClearance { get; set; }
    public string TransmissionType { get; set; } = string.Empty;
    public int NumberOfGears { get; set; }
    public string DriveType { get; set; } = string.Empty; // 2WD, 4WD, AWD
    public decimal EngineCapacity { get; set; }
    public int EnginePower { get; set; }
    public int EngineTorque { get; set; }
    public decimal FuelTankCapacity { get; set; }
    public decimal FuelConsumption { get; set; }
    public string EmissionStandard { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public string? SpecialFeatures { get; set; } // JSON array of special features
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}

public class VehicleDocument : BaseEntity
{
    public Guid VehicleId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string Category { get; set; } = "Active";
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}

public class VehicleInspection : BaseEntity
{
    public Guid VehicleId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string InspectionType { get; set; } = "Active";
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionCenter { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal? Mileage { get; set; }
    public string? DefectsFound { get; set; } // JSON array of defects found
    public string? RepairsRequired { get; set; } // JSON array of required repairs
    public string? RepairsCompleted { get; set; } // JSON array of completed repairs
    public decimal? InspectionFee { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}

public class VehicleInsurance : BaseEntity
{
    public Guid VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string PolicyType { get; set; } = "Active";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Deductible { get; set; }
    public string Status { get; set; } = "Active";
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? CoverageDetails { get; set; } // JSON object with coverage details
    public string? Claims { get; set; } // JSON array of claims made
    public DateTime? LastClaimDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}

public class VehicleMaintenance : BaseEntity
{
    public Guid VehicleId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = "Active";
    public string ServiceProvider { get; set; } = string.Empty;
    public decimal? Mileage { get; set; }
    public string? ServicesPerformed { get; set; } // JSON array of services performed
    public string? PartsReplaced { get; set; } // JSON array of parts replaced
    public decimal ServiceCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? NextServiceDate { get; set; }
    public decimal? NextServiceMileage { get; set; }
    public string? Warranty { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}

// Enumerations
public enum VehicleStatus
{
    Active,
    Inactive,
    InMaintenance,
    OutOfService,
    Retired,
    Sold
}

public enum RegistrationStatus
{
    Valid,
    Expired,
    Suspended,
    Cancelled,
    Pending
}

public enum DocumentCategory
{
    General,
    Registration,
    Insurance,
    Inspection,
    Maintenance,
    Purchase,
    Legal
}

public enum InspectionType
{
    Annual,
    PreTrip,
    PostTrip,
    Safety,
    Emissions,
    Special
}

public enum InspectionStatus
{
    Passed,
    Failed,
    Conditional,
    Pending,
    Expired
}

public enum InsuranceType
{
    Liability,
    Comprehensive,
    Collision,
    Commercial,
    Fleet
}

public enum InsuranceStatus
{
    Active,
    Expired,
    Cancelled,
    Suspended,
    Pending
}

public enum MaintenanceType
{
    Routine,
    Preventive,
    Corrective,
    Emergency,
    Seasonal,
    Warranty
}

public enum MaintenanceStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Overdue
}