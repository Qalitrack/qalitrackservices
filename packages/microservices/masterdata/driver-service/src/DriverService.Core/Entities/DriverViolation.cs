namespace DriverService.Core.Entities;

public enum ViolationType
{
    Speeding,
    RecklessDriving,
    DUI,
    RunningRedLight,
    ImproperParking,
    IllegalTurn,
    FollowingTooClose,
    WeightViolation,
    LogbookViolation,
    SafetyViolation,
    Other
}

public enum ViolationSeverity
{
    Minor,
    Major,
    Serious,
    Critical
}

public class DriverViolation : BaseEntity
{
    public ViolationType ViolationType { get; set; } = ViolationType.Other;
    public ViolationSeverity Severity { get; set; } = ViolationSeverity.Minor;
    public DateTime ViolationDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string IssuingOfficer { get; set; } = string.Empty;
    public string IssuingAgency { get; set; } = string.Empty;
    public decimal FineAmount { get; set; } = 0;
    public int Points { get; set; } = 0;
    public bool IsPaid { get; set; } = false;
    public DateTime? PaymentDate { get; set; }
    public DateTime? CourtDate { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public bool IsContested { get; set; } = false;
    public string Resolution { get; set; } = string.Empty;
    public DateTime? ResolutionDate { get; set; }
    public bool AffectsEmployment { get; set; } = true;
    public string ActionTaken { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}