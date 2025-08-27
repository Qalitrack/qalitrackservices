using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Weighbridge.Entities;

public class Weighbridge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Status { get; set; } = "Active";
    public string Type { get; set; } = "Truck"; // Truck, Rail, Portable, etc.
    public decimal MaxCapacity { get; set; } // Maximum weight capacity in tons
    public decimal MinCapacity { get; set; } // Minimum weight capacity in tons
    public decimal Accuracy { get; set; } = 0.1M; // Accuracy in percentage
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime InstallationDate { get; set; }
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public string? CalibrationAuthority { get; set; }
    public string OperatingHours { get; set; } = "24/7"; // Operating hours
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public decimal? ServiceFee { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public Guid OrganizationId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<WeighbridgeCalibration> Calibrations { get; set; } = new List<WeighbridgeCalibration>();
    public virtual ICollection<WeighbridgeMaintenance> MaintenanceRecords { get; set; } = new List<WeighbridgeMaintenance>();
    public virtual ICollection<WeighbridgeTransaction> Transactions { get; set; } = new List<WeighbridgeTransaction>();
    public virtual ICollection<WeighbridgeDocument> Documents { get; set; } = new List<WeighbridgeDocument>();
    
    // Cross-module relationships (handled via DbContext configuration)
    // public virtual ICollection<RouteWeighbridgeAssociation> RouteAssociations { get; set; } = new List<RouteWeighbridgeAssociation>();
    // public virtual ICollection<OrganizationWeighbridgeOwnership> Ownerships { get; set; } = new List<OrganizationWeighbridgeOwnership>();
}

public class WeighbridgeCalibration : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public DateTime CalibrationDate { get; set; }
    public string CalibrationBy { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime CertificateExpiryDate { get; set; }
    public string CalibrationAuthority { get; set; } = string.Empty;
    public string Status { get; set; } = "Passed"; // Passed, Failed, Conditional
    public decimal? AccuracyAchieved { get; set; }
    public string? TestWeights { get; set; } // JSON array of test weights used
    public string? TestResults { get; set; } // JSON array of test results
    public string? Adjustments { get; set; } // JSON array of adjustments made
    public decimal CalibrationCost { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}

public class WeighbridgeMaintenance : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = "Preventive"; // Preventive, Corrective, Emergency
    public string ServiceProvider { get; set; } = string.Empty;
    public string? WorkOrderNumber { get; set; }
    public string? WorkPerformed { get; set; } // JSON array of work performed
    public string? PartsReplaced { get; set; } // JSON array of parts replaced
    public decimal ServiceCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = "Completed"; // Scheduled, InProgress, Completed, Cancelled
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Warranty { get; set; }
    public string? TechnicianName { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}

public class WeighbridgeTransaction : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = "Weighing"; // Weighing, Calibration, Test
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductType { get; set; }
    public decimal GrossWeight { get; set; }
    public decimal TareWeight { get; set; }
    public decimal NetWeight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    public decimal? ServiceFee { get; set; }
    public string? PaymentMethod { get; set; }
    public string Status { get; set; } = "Completed"; // Pending, Completed, Disputed
    public string? OperatorName { get; set; }
    public string? Comments { get; set; }
    public string? ReceiptUrl { get; set; }
    public bool IsPrinted { get; set; } = false;

    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}

public class WeighbridgeDocument : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string Category { get; set; } = "General"; // Manual, Certificate, Calibration, Maintenance, etc.
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}

// Enumerations
public enum WeighbridgeStatus
{
    Active,
    Inactive,
    UnderMaintenance,
    OutOfService,
    CalibrationDue
}

public enum WeighbridgeType
{
    Truck,
    Rail,
    Portable,
    PitType,
    SurfaceMount,
    Livestock,
    Industrial
}

public enum CalibrationStatus
{
    Passed,
    Failed,
    Conditional,
    Pending,
    Overdue
}

public enum MaintenanceType
{
    Preventive,
    Corrective,
    Emergency,
    Upgrade,
    Inspection
}

public enum TransactionType
{
    Weighing,
    Calibration,
    Test,
    Inspection,
    Demo
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Cancelled,
    Disputed,
    Refunded
}

public enum DocumentCategory
{
    General,
    Manual,
    Certificate,
    Calibration,
    Maintenance,
    Warranty,
    Legal,
    Safety
}