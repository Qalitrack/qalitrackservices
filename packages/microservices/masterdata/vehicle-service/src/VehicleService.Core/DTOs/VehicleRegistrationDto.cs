namespace VehicleService.Core.DTOs;

public class VehicleRegistrationDto
{
    public string Id { get; set; } = string.Empty;
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
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateVehicleRegistrationRequest
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
    public string? Notes { get; set; }
}

public class UpdateVehicleRegistrationRequest
{
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string RegistrationCertificateNumber { get; set; } = string.Empty;
    public string RegisteredOwnerName { get; set; } = string.Empty;
    public string RegisteredOwnerAddress { get; set; } = string.Empty;
    public string RegisteredOwnerContact { get; set; } = string.Empty;
    public decimal RegistrationFee { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}