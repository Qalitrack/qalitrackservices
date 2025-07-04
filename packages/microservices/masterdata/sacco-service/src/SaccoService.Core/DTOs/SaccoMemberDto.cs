using SaccoService.Core.Entities;

namespace SaccoService.Core.DTOs;

public class SaccoMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string SaccoId { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public DateTime JoinDate { get; set; }
    public MembershipType MembershipType { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public decimal SharesOwned { get; set; }
    public decimal CurrentSavings { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaccoMemberDetailDto : SaccoMemberDto
{
    public List<SaccoShareDto> Shares { get; set; } = new();
    public List<SaccoLoanDto> Loans { get; set; } = new();
    public List<SaccoMembershipDto> Memberships { get; set; } = new();
}

public class AddMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public MembershipType MembershipType { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}

public class UpdateMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public MembershipType MembershipType { get; set; }
    public MembershipStatus Status { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}