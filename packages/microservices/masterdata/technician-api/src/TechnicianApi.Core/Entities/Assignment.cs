using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class Assignment : BaseEntity
{  
    [Required]
    public string ManagerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public AssignmentPriority Priority { get; set; } = AssignmentPriority.Normal;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
    public DateTime? Deadline { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Location details
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }

    // Technician IDs from the other microservice
    public ICollection<string> TechnicianIds { get; set; } = new List<string>();

    public virtual CheckIn? CheckIn { get; set; }
    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
    public virtual ServiceReport? ServiceReport { get; set; }
    public virtual ICollection<Requisition> Requisitions { get; set; } = new List<Requisition>();

    // Financial forms
    public virtual ICollection<PettyCashAdvanceForm> PettyCashAdvanceForms { get; set; } = new List<PettyCashAdvanceForm>();
    public virtual ICollection<AdvanceReturnForm> AdvanceReturnForms { get; set; } = new List<AdvanceReturnForm>();
    public virtual ICollection<PerDiemReturnForm> PerDiemReturnForms { get; set; } = new List<PerDiemReturnForm>();
    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    // Balance tracking
    public virtual AssignmentBalanceSummary? BalanceSummary { get; set; }
}

public enum AssignmentStatus
{
    Pending,
    Accepted,
    Declined,
    InProgress,
    Completed,
    Cancelled
}

public enum AssignmentPriority
{
    Low,
    Normal,
    High,
    Urgent
}
