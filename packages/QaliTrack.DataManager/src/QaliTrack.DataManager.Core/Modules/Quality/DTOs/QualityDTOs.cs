namespace QaliTrack.DataManager.Core.Modules.Quality.DTOs;

// Quality Test Result DTOs
public class QualityTestResultDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string TestCategory { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string TestedBy { get; set; } = string.Empty;
    public string TestResult { get; set; } = string.Empty;
    public decimal? TestValue { get; set; }
    public string? TestUnit { get; set; }
    public string? TestMethod { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateQualityTestResultDto
{
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string TestCategory { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string TestedBy { get; set; } = string.Empty;
    public string TestResult { get; set; } = string.Empty;
    public decimal? TestValue { get; set; }
    public string? TestUnit { get; set; }
    public string? TestMethod { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdateQualityTestResultDto
{
    public string TestType { get; set; } = string.Empty;
    public string TestCategory { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string TestedBy { get; set; } = string.Empty;
    public string TestResult { get; set; } = string.Empty;
    public decimal? TestValue { get; set; }
    public string? TestUnit { get; set; }
    public string? TestMethod { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Notes { get; set; }
}

// Seal Record DTOs
public class SealRecordDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid VehicleId { get; set; }
    public string SealNumber { get; set; } = string.Empty;
    public string SealType { get; set; } = string.Empty;
    public Guid SealedBy { get; set; }
    public DateTime SealedAt { get; set; }
    public Guid? InspectedBy { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? InspectionResult { get; set; }
    public string? InspectionNotes { get; set; }
    public bool RequiresInvestigation { get; set; }
    public string? PhotoEvidencePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSealRecordDto
{
    public Guid TransactionId { get; set; }
    public Guid VehicleId { get; set; }
    public string SealNumber { get; set; } = string.Empty;
    public string SealType { get; set; } = string.Empty;
    public Guid SealedBy { get; set; }
    public DateTime SealedAt { get; set; }
}

public class UpdateSealRecordDto
{
    public string SealNumber { get; set; } = string.Empty;
    public string SealType { get; set; } = string.Empty;
    public Guid? InspectedBy { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? InspectionResult { get; set; }
    public string? InspectionNotes { get; set; }
    public bool RequiresInvestigation { get; set; }
    public string? PhotoEvidencePath { get; set; }
}

// Vehicle Incident DTOs
public class VehicleIncidentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? SealRecordId { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ReportedBy { get; set; }
    public DateTime IncidentTime { get; set; }
    public DateTime ReportedTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool BlocksVehicle { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateVehicleIncidentDto
{
    public Guid VehicleId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? SealRecordId { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ReportedBy { get; set; }
    public DateTime IncidentTime { get; set; }
    public DateTime ReportedTime { get; set; }
    public string Status { get; set; } = "Open";
    public bool BlocksVehicle { get; set; }
}

public class UpdateVehicleIncidentDto
{
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool BlocksVehicle { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedTime { get; set; }
}