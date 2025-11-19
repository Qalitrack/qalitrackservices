using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.DTOs.Claim;

public class CreateClaimDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? SupportingDocuments { get; set; }
    public string? Justification { get; set; }
    public virtual Entities.Attachment? Attachment { get; set; }
}
