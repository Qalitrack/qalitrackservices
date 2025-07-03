namespace SaccoService.Core.Entities;

public class SaccoMeeting : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public MeetingType MeetingType { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    public string? Agenda { get; set; }
    public string? Minutes { get; set; }
    public int AttendanceCount { get; set; }
    public int QuorumRequired { get; set; }
    public bool QuorumMet { get; set; }
    public string? ChairpersonId { get; set; }
    public string? SecretaryId { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember? Chairperson { get; set; }
    public virtual SaccoMember? Secretary { get; set; }
}

public enum MeetingType
{
    General,
    Committee,
    Emergency,
    Annual,
    Special,
    Training
}

public enum MeetingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed
}