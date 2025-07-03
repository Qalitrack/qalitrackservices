using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterDriverDto
{
    public string Id { get; set; } = string.Empty;
    public string TransporterId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public DriverStatus Status { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Address { get; set; }
    public decimal? Salary { get; set; }
    public string? AssignedVehicleId { get; set; }
    public DateTime? LastMedicalCheckDate { get; set; }
    public DateTime? NextMedicalCheckDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssignDriverRequest
{
    public string TransporterId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Address { get; set; }
    public decimal? Salary { get; set; }
    public string? AssignedVehicleId { get; set; }
}