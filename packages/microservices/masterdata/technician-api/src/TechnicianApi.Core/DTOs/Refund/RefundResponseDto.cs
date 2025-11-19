namespace TechnicianApi.Core.DTOs.Refund;

public class RefundResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;

    // Refund details
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReferenceNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    // Manager Review
    public DateTime? ManagerReviewedAt { get; set; }
    public string? ManagerReviewedBy { get; set; }
    public string? ManagerComments { get; set; }

    // CFO Review
    public DateTime? CfoReviewedAt { get; set; }
    public string? CfoReviewedBy { get; set; }
    public string? CfoComments { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? ReceivedBy { get; set; }

    // Rejection
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
