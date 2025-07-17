namespace WeightDataService.Core.Entities;

public class WeightMeasurement : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public string? DriverId { get; set; }
    public decimal Weight { get; set; }
    public MeasurementType Type { get; set; }
    public MeasurementStatus Status { get; set; } = MeasurementStatus.Pending;
    public DateTime MeasurementDateTime { get; set; } = DateTime.UtcNow;
    public string? TicketReference { get; set; }
    public string? Notes { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string? ProductType { get; set; }
    public string? CustomerReference { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Real-time streaming fields
    public string? StreamingSessionId { get; set; }
    public DateTime? StreamingStartTime { get; set; }
    public DateTime? StreamingEndTime { get; set; }
    public bool IsStreaming { get; set; } = false;
    
    // Calibration fields
    public string? CalibrationId { get; set; }
    public DateTime? CalibrationDate { get; set; }
    public decimal? CalibrationDrift { get; set; }
    public bool CalibrationRequired { get; set; } = false;
    
    // Historical analysis fields
    public decimal? WeightTrend { get; set; }
    public decimal? WeightVariance { get; set; }
    public string? TrendCategory { get; set; }
    public DateTime? LastAnalysisDate { get; set; }
    
    public ICollection<WeightCorrection> Corrections { get; set; } = new List<WeightCorrection>();
}