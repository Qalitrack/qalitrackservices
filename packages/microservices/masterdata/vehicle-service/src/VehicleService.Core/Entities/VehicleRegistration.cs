namespace VehicleService.Core.Entities;

public class VehicleRegistration : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string RegistrationCertificateNumber { get; set; } = string.Empty;
    public string RegisteredOwnerName { get; set; } = string.Empty;
    public string RegisteredOwnerAddress { get; set; } = string.Empty;
    public string RegisteredOwnerContact { get; set; } = string.Empty;
    public decimal RegistrationFee { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}