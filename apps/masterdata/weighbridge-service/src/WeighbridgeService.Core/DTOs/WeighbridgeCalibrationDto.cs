using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeCalibrationDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public CalibrationType Type { get; set; }
    public CalibrationStatus Status { get; set; }
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
    public bool IsPassed { get; set; }
    public string? FailureReason { get; set; }
    public string? CorrectionApplied { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public string? Notes { get; set; }
    public string? CertificateFilePath { get; set; }
    public decimal? Cost { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ScheduleCalibrationRequest
{
    public DateTime ScheduledDate { get; set; }
    public CalibrationType Type { get; set; }
    public string? CalibrationCompany { get; set; }
    public string? Notes { get; set; }
}

public class RecordCalibrationRequest
{
    public DateTime ActualDate { get; set; }
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
    public bool IsPassed { get; set; }
    public string? FailureReason { get; set; }
    public string? CorrectionApplied { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public string? Notes { get; set; }
    public decimal? Cost { get; set; }
}