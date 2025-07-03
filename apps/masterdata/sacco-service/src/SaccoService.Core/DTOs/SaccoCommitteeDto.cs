using SaccoService.Core.Entities;

namespace SaccoService.Core.DTOs;

public class SaccoCommitteeDto
{
    public string Id { get; set; } = string.Empty;
    public string SaccoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public CommitteePosition Position { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime? TermEndDate { get; set; }
    public CommitteeStatus Status { get; set; }
    public string? Responsibilities { get; set; }
    public bool IsElected { get; set; }
    public decimal? Allowance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AddCommitteeMemberRequest
{
    public string MemberId { get; set; } = string.Empty;
    public CommitteePosition Position { get; set; }
    public DateTime? TermEndDate { get; set; }
    public string? Responsibilities { get; set; }
    public bool IsElected { get; set; } = true;
    public decimal? Allowance { get; set; }
}