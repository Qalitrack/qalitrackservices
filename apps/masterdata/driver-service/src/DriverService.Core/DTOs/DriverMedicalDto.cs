using DriverService.Core.Entities;

namespace DriverService.Core.DTOs;

public class DriverMedicalDto
{
    public string Id { get; set; } = string.Empty;
    public MedicalTestType TestType { get; set; }
    public DateTime ExamDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public MedicalStatus Status { get; set; }
    public string ExaminingPhysician { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public DateTime? NextExamDue { get; set; }
    public decimal Cost { get; set; }
    public bool IsCompliant { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDriverMedicalDto
{
    public MedicalTestType TestType { get; set; } = MedicalTestType.PhysicalExam;
    public DateTime ExamDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public MedicalStatus Status { get; set; } = MedicalStatus.Fit;
    public string ExaminingPhysician { get; set; } = string.Empty;
    public string MedicalFacility { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public DateTime? NextExamDue { get; set; }
    public decimal Cost { get; set; } = 0;
    public bool IsCompliant { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
}