using DriverService.Core.Entities;

namespace DriverService.Core.DTOs;

public class DriverTrainingDto
{
    public string Id { get; set; } = string.Empty;
    public string TrainingName { get; set; } = string.Empty;
    public TrainingType TrainingType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public TrainingStatus Status { get; set; }
    public decimal? Score { get; set; }
    public decimal? PassingScore { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal Duration { get; set; }
    public decimal Cost { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDriverTrainingDto
{
    public string TrainingName { get; set; } = string.Empty;
    public TrainingType TrainingType { get; set; } = TrainingType.Other;
    public string Description { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public decimal? PassingScore { get; set; }
    public decimal Duration { get; set; } = 0;
    public decimal Cost { get; set; } = 0;
    public string Location { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
}