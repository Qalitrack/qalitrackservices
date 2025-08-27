namespace QaliTrack.MasterData.Core.Modules.Driver.DTOs;

/// <summary>
/// Simplified DTO for creating a driver - only essential fields
/// </summary>
public class CreateDriverDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public Guid OrganizationId { get; set; }
    
    // Optional basic fields
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? EmploymentType { get; set; } = "FullTime";
}

/// <summary>
/// DTO for updating basic driver information
/// </summary>
public class UpdateDriverDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    
    // Optional fields
    public string? MiddleName { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? Salary { get; set; }
    public string? Department { get; set; }
    public string? Supervisor { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Simplified DTO for driver list view
/// </summary>
public class DriverSummaryDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public bool HasLicense { get; set; }
    public bool BiometricEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for single driver view - without child collections
/// </summary>
public class DriverDetailDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
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
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Supervisor { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public DateTime? BiometricRegistrationDate { get; set; }
    public bool BiometricEnabled { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Status flags
    public bool HasLicense { get; set; }
    public bool HasProfile { get; set; }
}

// Driver License DTOs
public class CreateDriverLicenseDto
{
    public Guid DriverId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
}

public class UpdateDriverLicenseDto
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class PatchDriverLicenseDto
{
    public string? LicenseNumber { get; set; }
    public string? LicenseClass { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? Restrictions { get; set; }
    public string? Status { get; set; }
}

public class DriverLicenseDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseClass { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsExpired { get; set; }
    public int DaysUntilExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Driver Training DTOs
public class CreateDriverTrainingDto
{
    public Guid DriverId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
}

public class UpdateDriverTrainingDto
{
    public string TrainingName { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
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
}

public class PatchDriverTrainingDto
{
    public string? TrainingName { get; set; }
    public string? TrainingType { get; set; }
    public string? TrainingProvider { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public decimal? Score { get; set; }
    public bool? IsCertified { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
}

public class DriverTrainingDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public bool IsCertified { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Driver Medical DTOs
public class CreateDriverMedicalDto
{
    public Guid DriverId { get; set; }
    public DateTime ExaminationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string IssuingDoctor { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class UpdateDriverMedicalDto
{
    public DateTime ExaminationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string IssuingDoctor { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Restrictions { get; set; }
    public string? Conditions { get; set; }
    public bool RequiresGlasses { get; set; } = false;
    public bool RequiresHearingAid { get; set; } = false;
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? Medications { get; set; }
    public string? Notes { get; set; }
}

public class PatchDriverMedicalDto
{
    public DateTime? ExaminationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CertificateNumber { get; set; }
    public string? IssuingDoctor { get; set; }
    public string? MedicalFacility { get; set; }
    public string? Status { get; set; }
    public string? Restrictions { get; set; }
    public string? Conditions { get; set; }
    public bool? RequiresGlasses { get; set; }
    public bool? RequiresHearingAid { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? Medications { get; set; }
    public string? Notes { get; set; }
}

public class DriverMedicalDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public DateTime ExaminationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string IssuingDoctor { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Restrictions { get; set; }
    public string? Conditions { get; set; }
    public bool RequiresGlasses { get; set; }
    public bool RequiresHearingAid { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? Medications { get; set; }
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Driver Document DTOs
public class DriverDocumentDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsExpired { get; set; }
}