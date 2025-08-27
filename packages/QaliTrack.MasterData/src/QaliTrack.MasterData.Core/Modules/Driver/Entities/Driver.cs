using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Driver.Entities;

public class Driver : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string EmergencyContactRelationship { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public string Status { get; set; } = "Active";
    public decimal? Salary { get; set; }
    public string? Department { get; set; }
    public string? Supervisor { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? FingerprintData { get; set; }
    public string? FaceRecognitionData { get; set; }
    public DateTime? BiometricRegistrationDate { get; set; }
    public bool BiometricEnabled { get; set; } = false;
    public Guid OrganizationId { get; set; }
    public string? Notes { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    // Navigation properties
    public virtual DriverLicense? License { get; set; }
    public virtual DriverProfile? Profile { get; set; }
    public virtual ICollection<DriverDocument> Documents { get; set; } = new List<DriverDocument>();
    public virtual ICollection<DriverTraining> Trainings { get; set; } = new List<DriverTraining>();
    public virtual ICollection<DriverMedical> MedicalRecords { get; set; } = new List<DriverMedical>();
    public virtual ICollection<DriverViolation> Violations { get; set; } = new List<DriverViolation>();
    public virtual ICollection<DriverPerformance> PerformanceRecords { get; set; } = new List<DriverPerformance>();

    // Cross-module relationships (handled via DbContext configuration)
    // public virtual ICollection<DriverSaccoMembership> SaccoMemberships { get; set; } = new List<DriverSaccoMembership>();
    // public virtual ICollection<DriverVehicleAssignment> VehicleAssignments { get; set; } = new List<DriverVehicleAssignment>();
    // public virtual ICollection<DriverTransporterEmployment> TransporterEmployments { get; set; } = new List<DriverTransporterEmployment>();
}

public class DriverLicense : BaseEntity
{
    public Guid DriverId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Restrictions { get; set; } // JSON array of restrictions
    public string? Endorsements { get; set; } // JSON array of endorsements
    public int ViolationPoints { get; set; } = 0;
    public DateTime? LastRenewalDate { get; set; }
    public string? RenewalStatus { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public class DriverProfile : BaseEntity
{
    public Guid DriverId { get; set; }
    public decimal? Rating { get; set; }
    public int TotalTrips { get; set; } = 0;
    public decimal TotalDistance { get; set; } = 0;
    public int SafetyScore { get; set; } = 100;
    public int PunctualityScore { get; set; } = 100;
    public string? SpecialSkills { get; set; } // JSON array of special skills
    public string? Languages { get; set; } // JSON array of languages spoken
    public bool IsAvailable { get; set; } = true;
    public DateTime? LastTripDate { get; set; }
    public string? PreferredRoutes { get; set; } // JSON array of preferred route IDs
    public string? PreferredVehicleTypes { get; set; } // JSON array of preferred vehicle types
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public class DriverDocument : BaseEntity
{
    public Guid DriverId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string Category { get; set; } = "Active";
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public class DriverTraining : BaseEntity
{
    public Guid DriverId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string TrainingType { get; set; } = "Active";
    public string? TrainingProvider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public decimal? Score { get; set; }
    public bool IsCertified { get; set; } = false;
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public class DriverMedical : BaseEntity
{
    public Guid DriverId { get; set; }
    public DateTime ExaminationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string IssuingDoctor { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Restrictions { get; set; } // JSON array of medical restrictions
    public string? Conditions { get; set; } // JSON array of medical conditions
    public bool RequiresGlasses { get; set; } = false;
    public bool RequiresHearingAid { get; set; } = false;
    public string? BloodType { get; set; }
    public string? Allergies { get; set; } // JSON array of allergies
    public string? Medications { get; set; } // JSON array of current medications
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public class DriverViolation : BaseEntity
{
    public Guid DriverId { get; set; }
    public DateTime ViolationDate { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? FineAmount { get; set; }
    public int PointsAssessed { get; set; } = 0;
    public string Status { get; set; } = "Active";
    public DateTime? CourtDate { get; set; }
    public string? CourtDecision { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public class DriverPerformance : BaseEntity
{
    public Guid DriverId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal SafetyScore { get; set; }
    public decimal PunctualityScore { get; set; }
    public decimal FuelEfficiencyScore { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }
    public decimal OverallScore { get; set; }
    public int TripsCompleted { get; set; }
    public decimal DistanceCovered { get; set; }
    public int IncidentsReported { get; set; }
    public int ViolationsReceived { get; set; }
    public string? Strengths { get; set; } // JSON array of strengths
    public string? AreasForImprovement { get; set; } // JSON array of improvement areas
    public string? ActionItems { get; set; } // JSON array of action items
    public string EvaluatedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

// Enumerations
public enum DriverStatus
{
    Active,
    Inactive,
    Suspended,
    OnLeave,
    Terminated
}

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Temporary
}

public enum LicenseStatus
{
    Valid,
    Expired,
    Suspended,
    Revoked,
    Pending
}

public enum DocumentCategory
{
    General,
    License,
    Medical,
    Training,
    Insurance,
    Personal,
    Legal
}

public enum TrainingType
{
    Safety,
    Technical,
    Defensive,
    Hazmat,
    FirstAid,
    Equipment,
    Compliance
}

public enum TrainingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public enum MedicalStatus
{
    Fit,
    Unfit,
    Conditional,
    Expired,
    Pending
}

public enum ViolationStatus
{
    Pending,
    Resolved,
    Contested,
    Dismissed,
    Paid
}