namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeConfigurationDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string ConfigurationName { get; set; } = string.Empty;
    public int AutoPrintTickets { get; set; }
    public int RequireDriverId { get; set; }
    public int RequireVehicleId { get; set; }
    public int EnableTareWeight { get; set; }
    public decimal MinimumWeight { get; set; }
    public decimal MaximumWeight { get; set; }
    public int StabilizationTime { get; set; }
    public int AutoZeroRange { get; set; }
    public string WeightUnit { get; set; } = string.Empty;
    public string DisplayFormat { get; set; } = string.Empty;
    public int BackupFrequency { get; set; }
    public string? BackupLocation { get; set; }
    public int AlertThreshold { get; set; }
    public string? AlertEmail { get; set; }
    public int MaintenanceInterval { get; set; }
    public int CalibrationInterval { get; set; }
    public string? CustomSettings { get; set; }
}

public class UpdateWeighbridgeConfigurationRequest
{
    public string ConfigurationName { get; set; } = string.Empty;
    public int AutoPrintTickets { get; set; }
    public int RequireDriverId { get; set; }
    public int RequireVehicleId { get; set; }
    public int EnableTareWeight { get; set; }
    public decimal MinimumWeight { get; set; }
    public decimal MaximumWeight { get; set; }
    public int StabilizationTime { get; set; }
    public int AutoZeroRange { get; set; }
    public string WeightUnit { get; set; } = string.Empty;
    public string DisplayFormat { get; set; } = string.Empty;
    public int BackupFrequency { get; set; }
    public string? BackupLocation { get; set; }
    public int AlertThreshold { get; set; }
    public string? AlertEmail { get; set; }
    public int MaintenanceInterval { get; set; }
    public int CalibrationInterval { get; set; }
    public string? CustomSettings { get; set; }
}