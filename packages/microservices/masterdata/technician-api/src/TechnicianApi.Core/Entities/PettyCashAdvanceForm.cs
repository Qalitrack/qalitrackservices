namespace TechnicianApi.Core.Entities;

public class PettyCashAdvanceForm : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public decimal Sum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PreparedBy { get; set; } = string.Empty; // Technician name

    // Status tracking
    public PettyCashStatus Status { get; set; } = PettyCashStatus.Pending;

    // Approval workflow
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComments { get; set; }

    // Rejection
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Disbursement
    public DateTime? DisbursedAt { get; set; }
    public string? DisbursedBy { get; set; }
    public string? VoucherNumber { get; set; }
    public string? ReferenceNumber { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
}

public enum PettyCashStatus
{
    Pending,
    Approved,
    Rejected,
    Disbursed
}
