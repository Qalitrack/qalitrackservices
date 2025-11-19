namespace TechnicianApi.Core.DTOs.Refund;

public class RejectRefundDto
{
    public string RejectedBy { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
}
