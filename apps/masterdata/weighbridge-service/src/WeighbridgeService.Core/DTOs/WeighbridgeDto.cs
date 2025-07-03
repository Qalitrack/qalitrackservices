using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public decimal MinCapacity { get; set; }
    public decimal Accuracy { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime InstallationDate { get; set; }
    public DateTime LastCalibrationDate { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public WeighbridgeStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related data
    public WeighbridgeLocationDto? WeighbridgeLocation { get; set; }
    public WeighbridgeConfigurationDto? Configuration { get; set; }
    public WeighbridgeCapacityDto? CurrentCapacity { get; set; }
    public List<WeighbridgeOperatorDto> Operators { get; set; } = new();
    public List<WeighbridgeCalibrationDto> RecentCalibrations { get; set; } = new();
    public List<WeighbridgeMaintenanceDto> UpcomingMaintenance { get; set; } = new();
}

public class RegisterWeighbridgeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public decimal MinCapacity { get; set; } = 0;
    public decimal Accuracy { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime InstallationDate { get; set; }
    public DateTime CalibrationDate { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class UpdateWeighbridgeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public decimal MinCapacity { get; set; }
    public decimal Accuracy { get; set; }
    public WeighbridgeStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}