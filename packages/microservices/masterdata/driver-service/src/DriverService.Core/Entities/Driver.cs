namespace DriverService.Core.Entities;

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

public class Driver : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
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
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public DriverStatus Status { get; set; } = DriverStatus.Active;
    public decimal? Salary { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Supervisor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public string? FingerprintData { get; set; }
    public string? FaceRecognitionData { get; set; }
    public DateTime? BiometricRegistrationDate { get; set; }
    public bool BiometricEnabled { get; set; } = false;

    // Navigation properties
    public virtual DriverLicense? License { get; set; }
    public virtual DriverProfile? Profile { get; set; }
    public virtual ICollection<DriverDocument> Documents { get; set; } = new List<DriverDocument>();
    public virtual ICollection<DriverTraining> Trainings { get; set; } = new List<DriverTraining>();
    public virtual ICollection<DriverMedical> MedicalRecords { get; set; } = new List<DriverMedical>();
    public virtual ICollection<DriverViolation> Violations { get; set; } = new List<DriverViolation>();
    public virtual ICollection<DriverPerformance> PerformanceRecords { get; set; } = new List<DriverPerformance>();
}