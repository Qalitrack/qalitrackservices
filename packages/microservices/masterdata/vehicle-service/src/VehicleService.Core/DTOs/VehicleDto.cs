using VehicleService.Core.Entities;

namespace VehicleService.Core.DTOs;

public class VehicleDto
{
    public string Id { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal TareWeight { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerContactInfo { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public decimal? CurrentMileage { get; set; }
    public string? Notes { get; set; }
    public string VehicleTypeId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Related entities
    public VehicleTypeDto? VehicleType { get; set; }
    public VehicleRegistrationDto? Registration { get; set; }
    public VehicleSpecificationDto? Specification { get; set; }
    public List<VehicleDocumentDto> Documents { get; set; } = new List<VehicleDocumentDto>();
    public List<VehicleInspectionDto> Inspections { get; set; } = new List<VehicleInspectionDto>();
    public List<VehicleInsuranceDto> InsurancePolicies { get; set; } = new List<VehicleInsuranceDto>();
}

public class RegisterVehicleRequest
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal TareWeight { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerContactInfo { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public decimal? CurrentMileage { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVehicleRequest
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal TareWeight { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerContactInfo { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; }
    public decimal? CurrentMileage { get; set; }
    public string? Notes { get; set; }
}