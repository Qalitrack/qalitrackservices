using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.DTOs.Base;

namespace TechnicianApi.Core.DTOs.Claim;

public class ClaimResponseDto : BaseResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;

    // Claim details
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? SupportingDocuments { get; set; }
    public string? Justification { get; set; }

    public string Status { get; set; } = string.Empty;

    // Manager Review
    public DateTime? ManagerReviewedAt { get; set; }
    public string? ManagerReviewedBy { get; set; }
    public string? ManagerComments { get; set; }

    // CFO Review
    public DateTime? CfoReviewedAt { get; set; }
    public string? CfoReviewedBy { get; set; }
    public string? CfoComments { get; set; }

    // Disbursement
    public DateTime? DisbursedAt { get; set; }
    public string? DisbursedBy { get; set; }
    public string? VoucherNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }

    // Rejection
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

}
