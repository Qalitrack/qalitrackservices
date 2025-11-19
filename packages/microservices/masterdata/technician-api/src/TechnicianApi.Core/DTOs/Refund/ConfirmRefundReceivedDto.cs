namespace TechnicianApi.Core.DTOs.Refund;

public class ConfirmRefundReceivedDto
{
    public string ReceivedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
