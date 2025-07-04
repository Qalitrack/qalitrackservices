namespace SaccoService.Core.Entities;

public class SaccoLoan : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string LoanNumber { get; set; } = string.Empty;
    public LoanType LoanType { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal MonthlyInstallment { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovalDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Applied;
    public string Purpose { get; set; } = string.Empty;
    public string? CollateralDescription { get; set; }
    public decimal? CollateralValue { get; set; }
    public string? GuarantorIds { get; set; } // JSON array of member IDs
    public string? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
    public virtual SaccoMember? ApprovedBy { get; set; }
}

public enum LoanType
{
    Personal,
    Business,
    Emergency,
    Education,
    Agriculture,
    Housing,
    Asset,
    Development
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
    WriteOff
}