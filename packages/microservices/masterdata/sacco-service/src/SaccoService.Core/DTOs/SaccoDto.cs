using SaccoService.Core.Entities;

namespace SaccoService.Core.DTOs;

public class SaccoDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    public SaccoType SaccoType { get; set; }
    public int MembershipCapacity { get; set; }
    public SaccoStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MemberCount { get; set; }
}

public class SaccoDetailDto : SaccoDto
{
    public List<SaccoMemberDto> Members { get; set; } = new();
    public List<SaccoCommitteeDto> Committees { get; set; } = new();
    public List<SaccoMeetingDto> RecentMeetings { get; set; } = new();
    public SaccoFinancialDto? Financial { get; set; }
}

public class RegisterSaccoRequest
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    public SaccoType SaccoType { get; set; }
    public int MembershipCapacity { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
}

public class UpdateSaccoRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public SaccoType SaccoType { get; set; }
    public int MembershipCapacity { get; set; }
    public SaccoStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
}