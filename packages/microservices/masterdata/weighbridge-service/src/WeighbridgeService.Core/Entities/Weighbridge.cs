namespace WeighbridgeService.Core.Entities;

public enum WeighbridgeStatus
{
    Active,
    Inactive,
    Maintenance,
    Calibrating,
    OutOfService
}

public class Weighbridge : BaseEntity
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
    public DateTime LastCalibrationDate { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public WeighbridgeStatus Status { get; set; } = WeighbridgeStatus.Active;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual WeighbridgeLocation? WeighbridgeLocation { get; set; }
    public virtual WeighbridgeConfiguration? Configuration { get; set; }
    public virtual ICollection<WeighbridgeCalibration> CalibrationHistory { get; set; } = new List<WeighbridgeCalibration>();
    public virtual ICollection<WeighbridgeMaintenance> MaintenanceSchedules { get; set; } = new List<WeighbridgeMaintenance>();
    public virtual ICollection<WeighbridgeOperator> Operators { get; set; } = new List<WeighbridgeOperator>();
    public virtual WeighbridgeCapacity? CurrentCapacity { get; set; }
}