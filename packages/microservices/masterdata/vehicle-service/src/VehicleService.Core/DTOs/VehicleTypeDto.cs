namespace VehicleService.Core.DTOs;

public class VehicleTypeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? MaxWeightLimit { get; set; }
    public bool RequiresSpecialLicense { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateVehicleTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? MaxWeightLimit { get; set; }
    public bool RequiresSpecialLicense { get; set; } = false;
}

public class UpdateVehicleTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? MaxWeightLimit { get; set; }
    public bool RequiresSpecialLicense { get; set; }
    public bool IsActive { get; set; }
}