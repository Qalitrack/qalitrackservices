namespace TechnicianApi.Core.DTOs.Claim;

public class UpdateClaimDto
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public string? SupportingDocuments { get; set; }
    public string? Justification { get; set; }
}
