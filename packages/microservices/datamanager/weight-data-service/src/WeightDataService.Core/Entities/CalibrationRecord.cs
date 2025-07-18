namespace WeightDataService.Core.Entities;

public class CalibrationRecord : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string CalibrationId { get; set; } = string.Empty;
    public DateTime CalibrationDate { get; set; } = DateTime.UtcNow;
    public string TechnicianId { get; set; } = string.Empty;
    public decimal ReferenceWeight { get; set; }
    public decimal MeasuredWeight { get; set; }
    public decimal Drift { get; set; }
    public decimal DriftPercentage { get; set; }
    public CalibrationStatus Status { get; set; } = CalibrationStatus.Valid;
    public DateTime NextCalibrationDue { get; set; }
    public string? Notes { get; set; }
    public string? CertificateNumber { get; set; }
    public bool IsAutomatic { get; set; } = false;
    public string OrganizationId { get; set; } = string.Empty;
}

public enum CalibrationStatus
{
    Valid,
    RequiresAttention,
    Failed,
    Expired,
    Pending
}