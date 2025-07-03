namespace VehicleService.Core.Entities;

public enum VehicleStatus
{
    Active,
    Inactive,
    InMaintenance,
    OutOfService,
    Retired
}

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
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerContactInfo { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public decimal? CurrentMileage { get; set; }
    public string? Notes { get; set; }

    // Foreign keys
    public string VehicleTypeId { get; set; } = string.Empty;

    // Navigation properties
    public virtual VehicleType VehicleType { get; set; } = null!;
    public virtual VehicleRegistration? Registration { get; set; }
    public virtual VehicleSpecification? Specification { get; set; }
    public virtual ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public virtual ICollection<VehicleInspection> Inspections { get; set; } = new List<VehicleInspection>();
    public virtual ICollection<VehicleInsurance> InsurancePolicies { get; set; } = new List<VehicleInsurance>();
}