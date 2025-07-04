namespace SaccoService.Core.Entities;

public class SaccoMember : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public MembershipType MembershipType { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public DateTime? DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public decimal SharesOwned { get; set; }
    public decimal CurrentSavings { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual ICollection<SaccoMembership> Memberships { get; set; } = new List<SaccoMembership>();
    public virtual ICollection<SaccoShare> Shares { get; set; } = new List<SaccoShare>();
    public virtual ICollection<SaccoLoan> Loans { get; set; } = new List<SaccoLoan>();
}

public enum MembershipType
{
    Ordinary,
    Associate,
    Junior,
    Corporate,
    Honorary
}

public enum MembershipStatus
{
    Active,
    Inactive,
    Suspended,
    Terminated,
    Deceased
}