namespace WeighbridgeService.Core.Entities;

public enum CalibrationType
{
    Initial,
    Routine,
    AfterMaintenance,
    Emergency,
    Regulatory
}

public enum CalibrationStatus
{
    Scheduled,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public class WeighbridgeCalibration : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public CalibrationType Type { get; set; }
    public CalibrationStatus Status { get; set; } = CalibrationStatus.Scheduled;
    public string? CalibratedBy { get; set; }
    public string? CertificationNumber { get; set; }
    public string? CalibrationCompany { get; set; }
    public decimal? TestWeight1 { get; set; }
    public decimal? ActualReading1 { get; set; }
    public decimal? TestWeight2 { get; set; }
    public decimal? ActualReading2 { get; set; }
    public decimal? TestWeight3 { get; set; }
    public decimal? ActualReading3 { get; set; }
    public decimal? Accuracy { get; set; }
    public decimal? LinearityError { get; set; }
    public decimal? RepeatabilityError { get; set; }
    public bool IsPassed { get; set; } = false;
    public string? FailureReason { get; set; }
    public string? CorrectionApplied { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public string? Notes { get; set; }
    public string? CertificateFilePath { get; set; }
    public decimal? Cost { get; set; }
    
    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}