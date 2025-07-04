namespace SaccoService.Core.Entities;

public class SaccoShare : BaseEntity
{
    public string SaccoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string ShareCertificateNumber { get; set; } = string.Empty;
    public int NumberOfShares { get; set; }
    public decimal ShareValue { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public ShareStatus Status { get; set; } = ShareStatus.Active;
    public string? TransferToMemberId { get; set; }
    public DateTime? TransferDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sacco Sacco { get; set; } = null!;
    public virtual SaccoMember Member { get; set; } = null!;
}

public enum ShareStatus
{
    Active,
    Transferred,
    Redeemed,
    Suspended
}