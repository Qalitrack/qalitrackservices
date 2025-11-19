namespace TechnicianApi.Core.DTOs.Claim;

public class RejectClaimDto
{
    public string RejectedBy { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
}
