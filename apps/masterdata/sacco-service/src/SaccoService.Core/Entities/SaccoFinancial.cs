namespace SaccoService.Core.Entities;

public class SaccoFinancial : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal ShareCapital { get; set; }
    public decimal ReservesFunds { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalLoansOutstanding { get; set; }
    public decimal CashAtBank { get; set; }
    public decimal CashAtHand { get; set; }
    public decimal AnnualIncome { get; set; }
    public decimal AnnualExpenses { get; set; }
    public decimal NetSurplus { get; set; }
    public DateTime FinancialYearStart { get; set; }
    public DateTime FinancialYearEnd { get; set; }
    public bool IsAudited { get; set; }
    public DateTime? LastAuditDate { get; set; }
    public string? AuditorName { get; set; }
    public decimal InterestRateOnLoans { get; set; }
    public decimal InterestRateOnSavings { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
}