namespace TechnicianApi.Core.Entities;

public class Claim : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;

    // Claim details
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? SupportingDocuments { get; set; } // File paths or URLs
    public string? Justification { get; set; }

    // Status
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

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
    public string? PaymentMethod { get; set; } // Cash, Bank Transfer, etc.

    // Rejection
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
    public virtual Attachment? Attachment { get; set; } = null!;
}

public enum ClaimStatus
{
    Pending,
    ManagerApproved,
    ManagerRejected,
    CfoApproved,
    CfoRejected,
    Disbursed
}
