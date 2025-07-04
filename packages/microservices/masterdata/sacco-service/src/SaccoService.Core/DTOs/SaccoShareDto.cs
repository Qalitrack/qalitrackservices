using SaccoService.Core.Entities;

namespace SaccoService.Core.DTOs;

public class SaccoShareDto
{
    public string Id { get; set; } = string.Empty;
    public string SaccoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string ShareCertificateNumber { get; set; } = string.Empty;
    public int NumberOfShares { get; set; }
    public decimal ShareValue { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public ShareStatus Status { get; set; }
    public string? TransferToMemberId { get; set; }
    public DateTime? TransferDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}