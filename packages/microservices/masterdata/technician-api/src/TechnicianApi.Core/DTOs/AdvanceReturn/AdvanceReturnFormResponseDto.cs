using TechnicianApi.Core.DTOs.Base;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.DTOs.AdvanceReturn;

public class AdvanceReturnFormResponseDto : BaseResponseDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public List<AdvanceReturnLineItemDto> LineItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;

    // Approval details
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComments { get; set; }
  
    // Rejection details
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

}
