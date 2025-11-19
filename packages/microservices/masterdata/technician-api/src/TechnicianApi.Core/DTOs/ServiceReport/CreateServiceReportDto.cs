namespace TechnicianApi.Core.DTOs.ServiceReport;

public class CreateServiceReportDto
{
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
    public string? SignatureData { get; set; }
}
