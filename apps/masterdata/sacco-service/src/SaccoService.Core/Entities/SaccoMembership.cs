namespace SaccoService.Core.Entities;

public class SaccoMembership : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public string SaccoMemberId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public decimal RegistrationFee { get; set; }
    public decimal AnnualFee { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TerminationReason { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal OutstandingFees { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
}