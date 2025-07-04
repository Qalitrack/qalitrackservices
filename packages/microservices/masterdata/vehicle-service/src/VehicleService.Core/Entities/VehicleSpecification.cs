namespace VehicleService.Core.Entities;

public class VehicleSpecification : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public string EngineCapacity { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty; // Manual, Automatic, CVT
    public int NumberOfCylinders { get; set; }
    public int SeatingCapacity { get; set; }
    public string DriveType { get; set; } = string.Empty; // FWD, RWD, AWD, 4WD
    public decimal Length { get; set; } // in meters
    public decimal Width { get; set; } // in meters
    public decimal Height { get; set; } // in meters
    public decimal Wheelbase { get; set; } // in meters
    public decimal FuelTankCapacity { get; set; } // in liters
    public string TireSize { get; set; } = string.Empty;
    public decimal LoadCapacity { get; set; } // in kg
    public decimal TowingCapacity { get; set; } // in kg
    public string SafetyFeatures { get; set; } = string.Empty; // JSON or comma-separated
    public string TechnologyFeatures { get; set; } = string.Empty; // JSON or comma-separated
    public string? AdditionalSpecifications { get; set; } // JSON for flexible additional specs

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}