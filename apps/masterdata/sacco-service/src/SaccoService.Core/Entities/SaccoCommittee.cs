namespace SaccoService.Core.Entities;

public class SaccoCommittee : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public CommitteePosition Position { get; set; }
    public DateTime AppointmentDate { get; set; } = DateTime.UtcNow;
    public DateTime? TermEndDate { get; set; }
    public CommitteeStatus Status { get; set; } = CommitteeStatus.Active;
    public string? Responsibilities { get; set; }
    public bool IsElected { get; set; } = true;
    public decimal? Allowance { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
}

public enum CommitteePosition
{
    Chairman,
    ViceChairman,
    Secretary,
    Treasurer,
    Member,
    Auditor,
    CreditOfficer,
    MarketingOfficer
}

public enum CommitteeStatus
{
    Active,
    Inactive,
    Suspended,
    Terminated
}