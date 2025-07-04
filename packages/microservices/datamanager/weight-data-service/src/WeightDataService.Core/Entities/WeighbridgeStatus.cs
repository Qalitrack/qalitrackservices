namespace WeightDataService.Core.Entities;

public class WeighbridgeStatus : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Active;
    public decimal MaxCapacity { get; set; }
    public decimal MinCapacity { get; set; } = 0;
    public decimal CurrentWeight { get; set; } = 0;
    public DateTime LastCalibrationDate { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public string? MaintenanceNotes { get; set; }
    public bool IsOnline { get; set; } = true;
    public string OrganizationId { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public decimal AccuracyTolerance { get; set; } = 0.1m;
}