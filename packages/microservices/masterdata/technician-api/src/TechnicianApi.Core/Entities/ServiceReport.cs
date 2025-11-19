namespace TechnicianApi.Core.Entities;

public class ServiceReport : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;

    // Customer & Location Information (auto-filled from assignment)
    public string CustomerName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;

    // Contact Person Details
    public string ContactPerson { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }

    // Visit Information
    public string? VehicleNo { get; set; }
    public NatureOfVisit NatureOfVisit { get; set; } = NatureOfVisit.NormalCustomerVisit;

    // Machine & Service Details
    public string? MachineDetails { get; set; }
    public string? FaultReported { get; set; }
    public string? Findings { get; set; }
    public string? Correction { get; set; }
    public string? FinalResult { get; set; }
    public string? PartsOffered { get; set; }

    // Customer Feedback
    public string? CustomerComments { get; set; }

    // Field Job Time (auto-populated from check-ins/check-outs)
    public DateTime? StartDay { get; set; }
    public DateTime? EndDay { get; set; }
    public int TotalFieldJobMinutes { get; set; } // Auto-calculated from all check-ins

    // Digital signature
    public string? SignatureData { get; set; }
    public DateTime? SignedAt { get; set; }

    // Status & Approval
    public ServiceReportStatus Status { get; set; } = ServiceReportStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
}

public enum ServiceReportStatus
{
    Draft,
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Revised
}

public enum NatureOfVisit
{
    PlannedMaintenance,
    Service,
    Repairs,
    NormalCustomerVisit
}
