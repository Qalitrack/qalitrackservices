namespace WeightDataService.Core.DTOs;

public class CalibrationRecordDto
{
    public Guid Id { get; set; }
    public string CalibrationId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime CalibrationDate { get; set; }
    public string TechnicianId { get; set; } = string.Empty;
    public decimal ReferenceWeight { get; set; }
    public decimal MeasuredWeight { get; set; }
    public decimal Drift { get; set; }
    public decimal DriftPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime NextCalibrationDue { get; set; }
    public string? Notes { get; set; }
    public string? CertificateNumber { get; set; }
    public bool IsAutomatic { get; set; }
}

public class CreateCalibrationRecordDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public decimal ReferenceWeight { get; set; }
    public decimal MeasuredWeight { get; set; }
    public string? Notes { get; set; }
    public bool IsAutomatic { get; set; } = false;
}

public class CalibrationStatusDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastCalibrationDate { get; set; }
    public DateTime NextCalibrationDue { get; set; }
    public decimal LastDrift { get; set; }
    public decimal LastDriftPercentage { get; set; }
    public bool RequiresCalibration { get; set; }
    public int DaysUntilDue { get; set; }
}

public class CalibrationDriftDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public decimal CurrentDrift { get; set; }
    public decimal DriftPercentage { get; set; }
    public bool DriftDetected { get; set; }
    public string? RecommendedAction { get; set; }
    public DateTime DetectionTime { get; set; }
}