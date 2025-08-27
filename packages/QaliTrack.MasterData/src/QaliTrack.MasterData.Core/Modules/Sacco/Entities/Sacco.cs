using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Sacco.Entities;

public class Sacco : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string SaccoType { get; set; } = "Active";
    public int MembershipCapacity { get; set; }
    public int CurrentMemberCount { get; set; } = 0;
    public string Status { get; set; } = "Active";
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal ShareCapitalMinimum { get; set; }
    public decimal ShareValue { get; set; }
    public string? LogoUrl { get; set; }
    public string? RegulatoryAuthority { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<SaccoMember> Members { get; set; } = new List<SaccoMember>();
    public virtual ICollection<SaccoCommittee> Committees { get; set; } = new List<SaccoCommittee>();
    public virtual ICollection<SaccoMeeting> Meetings { get; set; } = new List<SaccoMeeting>();
    public virtual SaccoFinancial? Financial { get; set; }
    public virtual ICollection<SaccoShare> Shares { get; set; } = new List<SaccoShare>();
    public virtual ICollection<SaccoLoan> Loans { get; set; } = new List<SaccoLoan>();
    public virtual ICollection<SaccoService> Services { get; set; } = new List<SaccoService>();

    // Cross-module relationships (handled via DbContext configuration)
    // public virtual ICollection<DriverSaccoMembership> DriverMemberships { get; set; } = new List<DriverSaccoMembership>();
    // public virtual ICollection<VehicleSaccoRegistration> VehicleRegistrations { get; set; } = new List<VehicleSaccoRegistration>();
}

public class SaccoMember : BaseEntity
{
    public Guid SaccoId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public string MembershipType { get; set; } = "Active";
    public string Status { get; set; } = "Active";
    public decimal SharesOwned { get; set; }
    public decimal ShareValue { get; set; }
    public decimal TotalContributions { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Notes { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
}

public class SaccoCommittee : BaseEntity
{
    public Guid SaccoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CommitteeType { get; set; } = "Active";
    public DateTime EstablishedDate { get; set; }
    public string Status { get; set; } = "Active";
    public int MaxMembers { get; set; } = 5;
    public string? Responsibilities { get; set; } // JSON array of responsibilities
    public string? MeetingSchedule { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual ICollection<SaccoCommitteeMember> CommitteeMembers { get; set; } = new List<SaccoCommitteeMember>();
}

public class SaccoCommitteeMember : BaseEntity
{
    public Guid SaccoCommitteeId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime AppointedDate { get; set; }
    public DateTime? TermEndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Responsibilities { get; set; } // JSON array of specific responsibilities
    public string? Notes { get; set; }

    // Navigation properties
    public virtual SaccoCommittee Committee { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
}

public class SaccoMeeting : BaseEntity
{
    public Guid SaccoId { get; set; }
    public Guid? SaccoCommitteeId { get; set; } // Optional - general meeting if null
    public string Title { get; set; } = string.Empty;
    public DateTime MeetingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string MeetingType { get; set; } = "Active";
    public string Status { get; set; } = "Active";
    public string? Agenda { get; set; } // JSON array of agenda items
    public string? Minutes { get; set; }
    public string? Resolutions { get; set; } // JSON array of resolutions made
    public int AttendeesCount { get; set; }
    public string? Documents { get; set; } // JSON array of meeting documents
    public string OrganizedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoCommittee? Committee { get; set; }
}

public class SaccoFinancial : BaseEntity
{
    public Guid SaccoId { get; set; }
    public decimal TotalShares { get; set; }
    public decimal ShareCapital { get; set; }
    public decimal ReservesFund { get; set; }
    public decimal LoansFund { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal OutstandingLoans { get; set; }
    public decimal BadDebts { get; set; }
    public decimal AnnualIncome { get; set; }
    public decimal OperatingExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public DateTime LastAuditDate { get; set; }
    public string? AuditorName { get; set; }
    public string FinancialStatus { get; set; } = "Active";
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
}

public class SaccoShare : BaseEntity
{
    public Guid SaccoId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string ShareCertificateNumber { get; set; } = string.Empty;
    public int NumberOfShares { get; set; }
    public decimal ShareValue { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime? TransferDate { get; set; }
    public string? TransferTo { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
}

public class SaccoLoan : BaseEntity
{
    public Guid SaccoId { get; set; }
    public Guid SaccoMemberId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string LoanType { get; set; } = "Active";
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int RepaymentPeriodMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public string Status { get; set; } = "Active";
    public string Purpose { get; set; } = string.Empty;
    public string? Collateral { get; set; } // JSON array of collateral items
    public string? Guarantors { get; set; } // JSON array of guarantors
    public string ApprovedBy { get; set; } = string.Empty;
    public string? RepaymentHistory { get; set; } // JSON array of payment history
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
}

public class SaccoService : BaseEntity
{
    public Guid SaccoId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Active";
    public decimal? ServiceFee { get; set; }
    public string? Eligibility { get; set; } // JSON array of eligibility criteria
    public string? Requirements { get; set; } // JSON array of requirements
    public string? ProcessingTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LaunchDate { get; set; }
    public string? ResponsibleCommittee { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
}

// Enumerations
public enum SaccoType
{
    Community,
    Employee,
    Faith,
    Agricultural,
    Transport,
    Business,
    Mixed,
    Housing,
    Teachers
}

public enum SaccoStatus
{
    Active,
    Inactive,
    Suspended,
    Pending,
    Dissolved,
    UnderReview
}

public enum MembershipType
{
    Regular,
    Associate,
    Honorary,
    Corporate,
    Youth,
    Senior
}

public enum MembershipStatus
{
    Active,
    Inactive,
    Suspended,
    Expired,
    Terminated,
    Deceased
}

public enum CommitteeType
{
    Management,
    Audit,
    Credit,
    Education,
    Supervisory,
    Investment,
    Disciplinary
}

public enum CommitteeStatus
{
    Active,
    Inactive,
    Dissolved,
    Suspended
}

public enum AppointmentStatus
{
    Active,
    Expired,
    Resigned,
    Removed,
    Suspended
}

public enum MeetingType
{
    Regular,
    Special,
    Annual,
    Emergency,
    Committee
}

public enum MeetingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed
}

public enum FinancialStatus
{
    Stable,
    Growing,
    Declining,
    Critical,
    Recovering
}

public enum ShareStatus
{
    Active,
    Transferred,
    Redeemed,
    Suspended
}

public enum LoanType
{
    Development,
    Emergency,
    Business,
    Education,
    Agriculture,
    Housing,
    Asset,
    Refinancing
}

public enum LoanStatus
{
    Applied,
    UnderReview,
    Approved,
    Rejected,
    Disbursed,
    Active,
    Completed,
    Defaulted,
    WrittenOff
}

public enum ServiceType
{
    Financial,
    Insurance,
    Investment,
    Advisory,
    Training,
    Transport,
    Social
}