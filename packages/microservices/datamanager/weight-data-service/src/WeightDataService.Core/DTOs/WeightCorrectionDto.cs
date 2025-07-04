namespace WeightDataService.Core.DTOs;

public class WeightCorrectionDto
{
    public Guid Id { get; set; }
    public Guid WeightMeasurementId { get; set; }
    public decimal OriginalWeight { get; set; }
    public decimal CorrectedWeight { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string AuthorizedBy { get; set; } = string.Empty;
    public DateTime CorrectionDateTime { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDateTime { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWeightCorrectionDto
{
    public Guid WeightMeasurementId { get; set; }
    public decimal CorrectedWeight { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ApproveWeightCorrectionDto
{
    public bool IsApproved { get; set; }
    public string? ApprovalNotes { get; set; }
}