namespace TechnicianApi.Core.DTOs.Refund;

public class ApproveRefundDto
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
