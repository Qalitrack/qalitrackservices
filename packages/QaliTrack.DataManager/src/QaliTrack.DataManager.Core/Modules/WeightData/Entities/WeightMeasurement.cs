using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.WeightData.Entities;

/// <summary>
/// Enhanced weight measurements for cement weighbridge operations
/// </summary>
public class WeightMeasurement : BaseEntity
{
    public Guid TransactionId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public decimal Weight { get; set; }
    public DateTime MeasurementTime { get; set; }
    public string MeasurementType { get; set; } = string.Empty; // Tare, Gross
    public string Status { get; set; } = "Valid";
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public bool IsCalibrated { get; set; } = true;
    public bool IsStable { get; set; } = true;
    public decimal? StabilityVariance { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Simplified calibration records for weighbridge maintenance
/// </summary>
public class CalibrationRecord : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public DateTime CalibrationDate { get; set; }
    public string CalibrationType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Annual
    public decimal AccuracyValue { get; set; }
    public string Status { get; set; } = "Valid";
    public decimal ToleranceLevel { get; set; }
    public DateTime NextCalibrationDate { get; set; }
    public string CalibratedBy { get; set; } = string.Empty;
    public string? CalibrationNotes { get; set; }
}