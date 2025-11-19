namespace TechnicianApi.Core.DTOs.ServiceReport;

public class ServiceReportResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string WorkPerformed { get; set; } = string.Empty;
    public string? MaterialsUsed { get; set; }
    public string? Observations { get; set; }
    public string? Recommendations { get; set; }
    public string? CustomerFeedback { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? SignatureData { get; set; }
    public DateTime? SignedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
