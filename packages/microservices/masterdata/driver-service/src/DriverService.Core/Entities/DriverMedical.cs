namespace DriverService.Core.Entities;

public enum MedicalStatus
{
    Fit,
    Restricted,
    Unfit,
    PendingReview
}

public enum MedicalTestType
{
    PhysicalExam,
    VisionTest,
    HearingTest,
    DrugTest,
    AlcoholTest,
    BloodTest,
    UrineTest,
    Psychological,
    Other
}

public class DriverMedical : BaseEntity
{
    public MedicalTestType TestType { get; set; } = MedicalTestType.PhysicalExam;
    public DateTime ExamDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public MedicalStatus Status { get; set; } = MedicalStatus.Fit;
    public string ExaminingPhysician { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty; // JSON array
    public string Results { get; set; } = string.Empty; // JSON object
    public string Recommendations { get; set; } = string.Empty;
    public DateTime? NextExamDue { get; set; }
    public decimal Cost { get; set; } = 0;
    public bool IsCompliant { get; set; } = true;
    public string Notes { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}