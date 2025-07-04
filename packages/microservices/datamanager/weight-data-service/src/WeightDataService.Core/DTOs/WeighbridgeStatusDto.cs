using WeightDataService.Core.Entities;

namespace WeightDataService.Core.DTOs;

public class WeighbridgeStatusDto
{
    public Guid Id { get; set; }
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public MaintenanceStatus Status { get; set; }
    public decimal MaxCapacity { get; set; }
    public decimal MinCapacity { get; set; }
    public decimal CurrentWeight { get; set; }
    public DateTime LastCalibrationDate { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public string? MaintenanceNotes { get; set; }
    public bool IsOnline { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public decimal AccuracyTolerance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWeighbridgeStatusDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public decimal MinCapacity { get; set; } = 0;
    public DateTime LastCalibrationDate { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public decimal AccuracyTolerance { get; set; } = 0.1m;
}

public class UpdateWeighbridgeStatusDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public MaintenanceStatus? Status { get; set; }
    public decimal? CurrentWeight { get; set; }
    public string? MaintenanceNotes { get; set; }
    public bool? IsOnline { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public decimal? AccuracyTolerance { get; set; }
}