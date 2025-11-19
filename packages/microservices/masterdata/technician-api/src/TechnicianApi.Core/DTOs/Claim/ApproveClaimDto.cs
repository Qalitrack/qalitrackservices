namespace TechnicianApi.Core.DTOs.Claim;

public class ApproveClaimDto
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
