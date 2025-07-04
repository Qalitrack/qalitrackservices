namespace VehicleService.Core.DTOs;

public class VehicleSpecificationDto
{
    public string Id { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public string EngineCapacity { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int NumberOfCylinders { get; set; }
    public int SeatingCapacity { get; set; }
    public string DriveType { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Wheelbase { get; set; }
    public decimal FuelTankCapacity { get; set; }
    public string TireSize { get; set; } = string.Empty;
    public decimal LoadCapacity { get; set; }
    public decimal TowingCapacity { get; set; }
    public string SafetyFeatures { get; set; } = string.Empty;
    public string TechnologyFeatures { get; set; } = string.Empty;
    public string? AdditionalSpecifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateVehicleSpecificationRequest
{
    public string VehicleId { get; set; } = string.Empty;
    public string EngineCapacity { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int NumberOfCylinders { get; set; }
    public int SeatingCapacity { get; set; }
    public string DriveType { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Wheelbase { get; set; }
    public decimal FuelTankCapacity { get; set; }
    public string TireSize { get; set; } = string.Empty;
    public decimal LoadCapacity { get; set; }
    public decimal TowingCapacity { get; set; }
    public string SafetyFeatures { get; set; } = string.Empty;
    public string TechnologyFeatures { get; set; } = string.Empty;
    public string? AdditionalSpecifications { get; set; }
}

public class UpdateVehicleSpecificationRequest
{
    public string EngineCapacity { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int NumberOfCylinders { get; set; }
    public int SeatingCapacity { get; set; }
    public string DriveType { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Wheelbase { get; set; }
    public decimal FuelTankCapacity { get; set; }
    public string TireSize { get; set; } = string.Empty;
    public decimal LoadCapacity { get; set; }
    public decimal TowingCapacity { get; set; }
    public string SafetyFeatures { get; set; } = string.Empty;
    public string TechnologyFeatures { get; set; } = string.Empty;
    public string? AdditionalSpecifications { get; set; }
}