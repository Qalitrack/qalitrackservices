namespace QaliTrack.DataManager.Core.Modules.WeightData.DTOs;

public class WeightMeasurementDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public decimal Weight { get; set; }
    public DateTime MeasurementTime { get; set; }
    public string MeasurementType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public string CalibrationReference { get; set; } = string.Empty;
    public bool IsCalibrated { get; set; }
    public string? Notes { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWeightMeasurementDto
{
    public Guid TransactionId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public decimal Weight { get; set; }
    public string MeasurementType { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public string? Notes { get; set; }
    public Guid OrganizationId { get; set; }
}

public class CalibrationRecordDto
{
    public Guid Id { get; set; }
    public Guid WeighbridgeId { get; set; }
    public DateTime CalibrationDate { get; set; }
    public string CalibrationType { get; set; } = string.Empty;
    public decimal AccuracyValue { get; set; }
    public string CertificationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ToleranceLevel { get; set; }
    public string CalibrationStandard { get; set; } = string.Empty;
    public DateTime NextCalibrationDate { get; set; }
    public string CalibratedBy { get; set; } = string.Empty;
    public string? CalibrationNotes { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCalibrationRecordDto
{
    public Guid WeighbridgeId { get; set; }
    public string CalibrationType { get; set; } = string.Empty;
    public decimal AccuracyValue { get; set; }
    public string CertificationNumber { get; set; } = string.Empty;
    public decimal ToleranceLevel { get; set; }
    public string CalibrationStandard { get; set; } = string.Empty;
    public DateTime NextCalibrationDate { get; set; }
    public string CalibratedBy { get; set; } = string.Empty;
    public string? CalibrationNotes { get; set; }
    public Guid OrganizationId { get; set; }
}