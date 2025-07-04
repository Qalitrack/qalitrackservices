namespace VehicleService.Core.Entities;

public enum InspectionResult
{
    Pass,
    Fail,
    ConditionalPass,
    Pending
}

public enum InspectionType
{
    AnnualInspection,
    PrePurchaseInspection,
    AccidentInspection,
    MaintenanceInspection,
    ComplianceInspection,
    SafetyInspection,
    EmissionTest,
    Other
}

public class VehicleInspection : BaseEntity
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
    public string? Issues { get; set; } // JSON or text describing any issues found
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }
    public bool IsValid { get; set; } = true;

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}