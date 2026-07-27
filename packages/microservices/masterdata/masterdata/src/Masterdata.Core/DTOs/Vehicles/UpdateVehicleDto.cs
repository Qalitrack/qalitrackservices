using System.ComponentModel.DataAnnotations;
using Masterdata.Core.Enums;

namespace Masterdata.Core.DTOs.Vehicles;

public class UpdateVehicleDto
{
    // Identification
    [Required(ErrorMessage = "Registration number is required")]
    [MaxLength(20, ErrorMessage = "Registration number cannot be longer than 20 characters")]
    public string RegistrationNumber { get; set; } = null!;

    [Required(ErrorMessage = "Vehicle type is required")]
    [MaxLength(50, ErrorMessage = "Type cannot be longer than 50 characters")]
    public string Type { get; set; } = null!;

    [MaxLength(50, ErrorMessage = "Make cannot be longer than 50 characters")]
    public string? Make { get; set; }

    [MaxLength(50, ErrorMessage = "Model cannot be longer than 50 characters")]
    public string? Model { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year of manufacture must be between 1900 and 2100")]
    public int? YearOfManufacture { get; set; }

    [MaxLength(50, ErrorMessage = "Color cannot be longer than 50 characters")]
    public string? Color { get; set; }

    [MaxLength(50, ErrorMessage = "Chassis number cannot be longer than 50 characters")]
    public string? ChassisNumber { get; set; }

    [MaxLength(50, ErrorMessage = "Engine number cannot be longer than 50 characters")]
    public string? EngineNumber { get; set; }

    // Status and Classification
    [MaxLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
    public string? Status { get; set; }

    [MaxLength(50, ErrorMessage = "Vehicle class cannot be longer than 50 characters")]
    public string? VehicleClass { get; set; }

    [MaxLength(50, ErrorMessage = "Body type cannot be longer than 50 characters")]
    public string? BodyType { get; set; }

    // Technical Specifications
    [Range(0, double.MaxValue, ErrorMessage = "Gross weight must be a positive number")]
    public decimal? GrossWeight { get; set; } // in kg

    [Range(0, double.MaxValue, ErrorMessage = "Tare weight must be a positive number")]
    public decimal? TareWeight { get; set; } // in kg

    [Range(0, double.MaxValue, ErrorMessage = "Net weight capacity must be a positive number")]
    public decimal? NetWeightCapacity { get; set; } // in kg

    [Range(0, int.MaxValue, ErrorMessage = "Seating capacity must be a positive number")]
    public int? SeatingCapacity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Fuel tank capacity must be a positive number")]
    public decimal? FuelTankCapacity { get; set; } // in liters

    // Insurance and Registration
    [MaxLength(50, ErrorMessage = "Insurance policy number cannot be longer than 50 characters")]
    public string? InsurancePolicyNumber { get; set; }

    public DateTime? InsuranceExpiryDate { get; set; }

    [MaxLength(50, ErrorMessage = "Road worthiness number cannot be longer than 50 characters")]
    public string? RoadWorthinessNumber { get; set; }

    public DateTime? RoadWorthinessExpiryDate { get; set; }

    // References
    public string? SupplierId { get; set; }
    public string? TransporterId { get; set; }

    // Not [Required]: Vehicle.OwnerId is nullable on the entity (a vehicle
    // can be unassigned from an owner via /Owners/{id}/vehicles/remove),
    // so the update endpoint must accept saving an already-ownerless
    // vehicle. Creation still requires an owner (CreateVehicleDto).
    public string? OwnerId { get; set; }

    [Required(ErrorMessage = "Axle configuration ID is required")]
    public string AxleConfigurationId { get; set; } = null!;
     
    // Driver assignments
    public List<string>? DriverIds { get; set; } = new();
    
    public string? RfiDcode { get; set; }
}
