namespace WeightDataService.Core.Entities;

public class WeightCorrection : BaseEntity
{
    public Guid WeightMeasurementId { get; set; }
    public decimal OriginalWeight { get; set; }
    public decimal CorrectedWeight { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string AuthorizedBy { get; set; } = string.Empty;
    public DateTime CorrectionDateTime { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDateTime { get; set; }
    public string? ApprovalNotes { get; set; }
    
    public WeightMeasurement WeightMeasurement { get; set; } = null!;
}