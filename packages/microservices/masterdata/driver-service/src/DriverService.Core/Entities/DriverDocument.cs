namespace DriverService.Core.Entities;

public enum DocumentType
{
    DriverLicense,
    Passport,
    NationalId,
    MedicalCertificate,
    TrainingCertificate,
    InsuranceDocument,
    EmploymentContract,
    BackgroundCheck,
    DrugTest,
    Other
}

public class DriverDocument : BaseEntity
{
    public string DocumentName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; } = DocumentType.Other;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerificationDate { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}