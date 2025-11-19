namespace TechnicianApi.Core.Entities;

public class PerDiemReturnForm : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty; // Project ID
    public string TechnicianId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;

    // Expense breakdown
    public decimal FaresOrCarExpense { get; set; }
    public decimal Mileage { get; set; }
    public decimal Meals { get; set; }
    public decimal Medical { get; set; }
    public decimal Incidentals { get; set; }

    // Calculated total
    public decimal TotalAmount { get; set; }

    // Status tracking
    public PerDiemReturnStatus Status { get; set; } = PerDiemReturnStatus.Pending;

    // Approval workflow
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComments { get; set; }

    // Rejection
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
    public virtual Attachment? Attachment { get; set; } = null!;
}

public enum PerDiemReturnStatus
{
    Pending,
    Approved,
    Rejected
}
