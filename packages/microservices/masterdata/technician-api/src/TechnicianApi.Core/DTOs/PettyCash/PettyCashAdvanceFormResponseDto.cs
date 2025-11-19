namespace TechnicianApi.Core.DTOs.PettyCash;

public class PettyCashAdvanceFormResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public decimal Sum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PreparedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Approval details
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComments { get; set; }

    // Rejection details
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Disbursement details
    public DateTime? DisbursedAt { get; set; }
    public string? DisbursedBy { get; set; }
    public string? VoucherNumber { get; set; }
    public string? ReferenceNumber { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
