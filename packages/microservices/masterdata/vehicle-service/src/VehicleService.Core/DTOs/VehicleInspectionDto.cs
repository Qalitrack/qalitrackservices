using VehicleService.Core.Entities;

namespace VehicleService.Core.DTOs;

public class VehicleInspectionDto
{
    public string Id { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public InspectionType InspectionType { get; set; }
    public InspectionResult Result { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionCenter { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public decimal InspectionFee { get; set; }
    public int CurrentMileage { get; set; }
    public string? Issues { get; set; }
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RecordVehicleInspectionRequest
{
    public string VehicleId { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public InspectionType InspectionType { get; set; }
    public InspectionResult Result { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionCenter { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public decimal InspectionFee { get; set; }
    public int CurrentMileage { get; set; }
    public string? Issues { get; set; }
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVehicleInspectionRequest
{
    public DateTime? NextInspectionDue { get; set; }
    public InspectionResult Result { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionCenter { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public decimal InspectionFee { get; set; }
    public int CurrentMileage { get; set; }
    public string? Issues { get; set; }
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }
    public bool IsValid { get; set; }
}