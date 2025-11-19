namespace TechnicianApi.Core.Entities;

public class Refund : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;

    // Refund details
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; } // Cash, Bank Transfer, Cheque, etc.
    public string? ReceiptNumber { get; set; }
    public string? ReferenceNumber { get; set; }

    // Status
    public RefundStatus Status { get; set; } = RefundStatus.Pending;

    // Manager Review
    public DateTime? ManagerReviewedAt { get; set; }
    public string? ManagerReviewedBy { get; set; }
    public string? ManagerComments { get; set; }

    // CFO Review (to confirm receipt of money)
    public DateTime? CfoReviewedAt { get; set; }
    public string? CfoReviewedBy { get; set; }
    public string? CfoComments { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? ReceivedBy { get; set; }

    // Rejection
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
    public virtual  Attachment? Attachment { get; set; } = null!;
}

public enum RefundStatus
{
    Pending,
    ManagerApproved,
    ManagerRejected,
    CfoReceived,      // CFO confirms money received
    CfoRejected
}
