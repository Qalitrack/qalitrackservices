using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public TransporterType TransporterType { get; set; }
    public int FleetSize { get; set; }
    public string? OperatingLicense { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public TransporterStatus Status { get; set; }
    public decimal? Rating { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RegisterTransporterRequest
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public TransporterType TransporterType { get; set; }
    public int FleetSize { get; set; }
    public string? OperatingLicense { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
}

public class UpdateTransporterRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public int FleetSize { get; set; }
    public string? OperatingLicense { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public TransporterStatus Status { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
}