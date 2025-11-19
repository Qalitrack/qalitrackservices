using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class AdvanceReturnForm : BaseEntity
{
    
    public virtual Attachment? Attachment { get; set; } = null!;
   [Required]
    public string AssignmentId { get; set; } = string.Empty;
    [Required]
    public string TechnicianId { get; set; } = string.Empty;

    // Line items - particulars with amounts
    public virtual ICollection<AdvanceReturnLineItem> LineItems { get; set; } = new List<AdvanceReturnLineItem>();

    // Calculated total from line items
    public decimal TotalAmount { get; set; }

    // Status tracking
    public AdvanceReturnStatus Status { get; set; } = AdvanceReturnStatus.Pending;

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
}

public class AdvanceReturnLineItem : BaseEntity
{
    public string AdvanceReturnFormId { get; set; } = string.Empty;
    public string Particular { get; set; } = string.Empty; // Name (cement, water, etc.)
    public decimal AmountInKsh { get; set; }

    // Navigation property
    public virtual AdvanceReturnForm AdvanceReturnForm { get; set; } = null!;
}

public enum AdvanceReturnStatus
{
    Pending,
    Approved,
    Rejected
}
