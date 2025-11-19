namespace TechnicianApi.Core.Entities;

public class Requisition : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public RequisitionType Type { get; set; } = RequisitionType.MaterialRequisition;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ItemsList { get; set; }
    public string? Justification { get; set; }

    // Workflow status
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;

    // TM Review
    public DateTime? TmReviewedAt { get; set; }
    public string? TmReviewedBy { get; set; }
    public string? TmComments { get; set; }

    // CFO Processing
    public DateTime? CfoReviewedAt { get; set; }
    public string? CfoReviewedBy { get; set; }
    public string? CfoComments { get; set; }
    public string? VoucherNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? PaidAt { get; set; }

    // Rejection
    public string? RejectionReason { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
}

public enum RequisitionType
{
    MaterialRequisition,
    CashAdvance,
    CashReturn,
    PerDiemReturn
}

public enum RequisitionStatus
{
    Pending,
    TmApproved,
    TmRejected,
    CfoProcessing,
    CfoApproved,
    Paid,
    CfoRejected
}
