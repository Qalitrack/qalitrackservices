using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterFleetDto
{
    public string Id { get; set; } = string.Empty;
    public string TransporterId { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public decimal? LoadCapacity { get; set; }
    public decimal? FuelCapacity { get; set; }
    public VehicleStatus Status { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? CurrentDriverId { get; set; }
    public string? CurrentLocation { get; set; }
    public decimal? Mileage { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AddFleetVehicleRequest
{
    public string TransporterId { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public decimal? LoadCapacity { get; set; }
    public decimal? FuelCapacity { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? Notes { get; set; }
}