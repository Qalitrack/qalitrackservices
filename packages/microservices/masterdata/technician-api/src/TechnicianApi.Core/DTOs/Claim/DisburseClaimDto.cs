namespace TechnicianApi.Core.DTOs.Claim;

public class DisburseClaimDto
{
    public string DisbursedBy { get; set; } = string.Empty;
    public string? VoucherNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }
}
