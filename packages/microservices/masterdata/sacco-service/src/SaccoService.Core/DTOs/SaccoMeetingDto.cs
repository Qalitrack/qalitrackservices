using SaccoService.Core.Entities;

namespace SaccoService.Core.DTOs;

public class SaccoMeetingDto
{
    public string Id { get; set; } = string.Empty;
    public string SaccoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public MeetingType MeetingType { get; set; }
    public MeetingStatus Status { get; set; }
    public string? Agenda { get; set; }
    public string? Minutes { get; set; }
    public int AttendanceCount { get; set; }
    public int QuorumRequired { get; set; }
    public bool QuorumMet { get; set; }
    public string? ChairpersonName { get; set; }
    public string? SecretaryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ScheduleMeetingRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public MeetingType MeetingType { get; set; }
    public string? Agenda { get; set; }
    public int QuorumRequired { get; set; }
    public string? ChairpersonId { get; set; }
    public string? SecretaryId { get; set; }
}

public class UpdateMeetingRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public MeetingStatus Status { get; set; }
    public string? Agenda { get; set; }
    public string? Minutes { get; set; }
    public int AttendanceCount { get; set; }
    public bool QuorumMet { get; set; }
}