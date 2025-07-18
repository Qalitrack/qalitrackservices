namespace TransporterService.Core.Entities;

public enum TransporterStatus
{
    Active,
    Inactive,
    Suspended,
    UnderReview
}

public enum TransporterType
{
    Individual,
    Company,
    Cooperative,
    Government
}

public class Transporter : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public TransporterType TransporterType { get; set; }
    public int FleetSize { get; set; }
    public string? OperatingLicense { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public TransporterStatus Status { get; set; } = TransporterStatus.Active;
    public decimal? Rating { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    
    // Dual-role support (transporter can also be a customer)
    public bool IsCustomer { get; set; } = false;
    public string? CustomerServiceReference { get; set; }
    public DateTime? CustomerRegistrationDate { get; set; }
    public string? CustomerNotes { get; set; }
    
    // Navigation properties
    public virtual ICollection<TransporterContact> Contacts { get; set; } = new List<TransporterContact>();
    public virtual ICollection<TransporterFleet> Fleet { get; set; } = new List<TransporterFleet>();
    public virtual ICollection<TransporterDriver> Drivers { get; set; } = new List<TransporterDriver>();
    public virtual ICollection<TransporterLicense> Licenses { get; set; } = new List<TransporterLicense>();
    public virtual ICollection<TransporterInsurance> Insurance { get; set; } = new List<TransporterInsurance>();
    public virtual ICollection<TransporterContract> Contracts { get; set; } = new List<TransporterContract>();
    public virtual ICollection<TransporterPerformance> PerformanceRecords { get; set; } = new List<TransporterPerformance>();
}