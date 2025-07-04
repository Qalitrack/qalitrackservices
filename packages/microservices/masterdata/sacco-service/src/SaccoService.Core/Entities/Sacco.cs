namespace SaccoService.Core.Entities;

public class Sacco : BaseEntity
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
    public SaccoStatus Status { get; set; } = SaccoStatus.Active;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
    public string? LogoUrl { get; set; }

    // Navigation properties
    public virtual ICollection<SaccoMember> Members { get; set; } = new List<SaccoMember>();
    public virtual ICollection<SaccoCommittee> Committees { get; set; } = new List<SaccoCommittee>();
    public virtual ICollection<SaccoMeeting> Meetings { get; set; } = new List<SaccoMeeting>();
    public virtual SaccoFinancial? Financial { get; set; }
    public virtual ICollection<SaccoShare> Shares { get; set; } = new List<SaccoShare>();
    public virtual ICollection<SaccoLoan> Loans { get; set; } = new List<SaccoLoan>();
}

public enum SaccoType
{
    Community,
    Employee,
    Faith,
    Agricultural,
    Transport,
    Business,
    Mixed
}

public enum SaccoStatus
{
    Active,
    Inactive,
    Suspended,
    Pending,
    Dissolved
}