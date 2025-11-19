namespace TechnicianApi.Core.DTOs.ServiceReport;

public class UpdateServiceReportDto
{
    public string? WorkPerformed { get; set; }
    public string? MaterialsUsed { get; set; }
    public string? Observations { get; set; }
    public string? Recommendations { get; set; }
    public string? CustomerFeedback { get; set; }
    public string? SignatureData { get; set; }
    public string? Status { get; set; }
    public string? RejectionReason { get; set; }
}
