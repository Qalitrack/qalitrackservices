using SaccoService.Core.Entities;

namespace SaccoService.Core.DTOs;

public class SaccoLoanDto
{
    public string Id { get; set; } = string.Empty;
    public string SaccoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string LoanNumber { get; set; } = string.Empty;
    public LoanType LoanType { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal MonthlyInstallment { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public LoanStatus Status { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? CollateralDescription { get; set; }
    public decimal? CollateralValue { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}