using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterLicenseDto
{
    public string Id { get; set; } = string.Empty;
    public string TransporterId { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Restrictions { get; set; }
    public decimal? Fee { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}