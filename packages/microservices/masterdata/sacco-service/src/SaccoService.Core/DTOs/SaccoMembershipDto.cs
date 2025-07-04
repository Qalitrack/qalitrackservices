namespace SaccoService.Core.DTOs;

public class SaccoMembershipDto
{
    public string Id { get; set; } = string.Empty;
    public string SaccoId { get; set; } = string.Empty;
    public string SaccoMemberId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal RegistrationFee { get; set; }
    public decimal AnnualFee { get; set; }
    public bool IsActive { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal OutstandingFees { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}