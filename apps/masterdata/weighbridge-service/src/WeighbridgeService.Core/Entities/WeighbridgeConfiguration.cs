namespace WeighbridgeService.Core.Entities;

public class WeighbridgeConfiguration : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string ConfigurationName { get; set; } = string.Empty;
    public int AutoPrintTickets { get; set; } = 0; // 0 = No, 1 = Yes
    public int RequireDriverId { get; set; } = 1; // 0 = No, 1 = Yes
    public int RequireVehicleId { get; set; } = 1; // 0 = No, 1 = Yes
    public int EnableTareWeight { get; set; } = 1; // 0 = No, 1 = Yes
    public decimal MinimumWeight { get; set; } = 0;
    public decimal MaximumWeight { get; set; }
    public int StabilizationTime { get; set; } = 3; // seconds
    public int AutoZeroRange { get; set; } = 5; // kg
    public string WeightUnit { get; set; } = "kg"; // kg, tons, lbs
    public string DisplayFormat { get; set; } = "0.00";
    public int BackupFrequency { get; set; } = 24; // hours
    public string? BackupLocation { get; set; }
    public int AlertThreshold { get; set; } = 90; // percentage of max capacity
    public string? AlertEmail { get; set; }
    public int MaintenanceInterval { get; set; } = 30; // days
    public int CalibrationInterval { get; set; } = 365; // days
    public string? CustomSettings { get; set; } // JSON format for additional settings
    
    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}