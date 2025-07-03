namespace DriverService.Core.Entities;

public enum TrainingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public enum TrainingType
{
    DefensiveDriving,
    HazmatHandling,
    FirstAid,
    SafetyProtocols,
    VehicleInspection,
    CustomerService,
    Navigation,
    Regulatory,
    Other
}

public class DriverTraining : BaseEntity
{
    public string TrainingName { get; set; } = string.Empty;
    public TrainingType TrainingType { get; set; } = TrainingType.Other;
    public string Description { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public TrainingStatus Status { get; set; } = TrainingStatus.Scheduled;
    public decimal? Score { get; set; }
    public decimal? PassingScore { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal Duration { get; set; } = 0; // In hours
    public decimal Cost { get; set; } = 0;
    public string Location { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}