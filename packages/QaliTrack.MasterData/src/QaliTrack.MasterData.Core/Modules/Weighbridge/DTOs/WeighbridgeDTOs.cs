namespace QaliTrack.MasterData.Core.Modules.Weighbridge.DTOs;

public record CreateWeighbridgeDto(
    string Name,
    string Code,
    string Location,
    decimal Latitude,
    decimal Longitude,
    decimal MaxCapacity,
    decimal MinCapacity,
    string Manufacturer,
    string Model,
    string SerialNumber,
    DateTime InstallationDate,
    Guid OrganizationId,
    string Status = "Active",
    string Type = "Truck",
    decimal Accuracy = 0.1M,
    string? CertificateNumber = null,
    DateTime? CertificateExpiryDate = null,
    string? CalibrationAuthority = null,
    string OperatingHours = "24/7",
    string? ContactPerson = null,
    string? ContactPhone = null,
    decimal? ServiceFee = null,
    string Currency = "USD",
    string? Notes = null
);

public record UpdateWeighbridgeDto(
    string Name,
    string Location,
    decimal Latitude,
    decimal Longitude,
    string Status,
    string Type,
    decimal MaxCapacity,
    decimal MinCapacity,
    decimal Accuracy,
    string Manufacturer,
    string Model,
    string? CertificateNumber = null,
    DateTime? CertificateExpiryDate = null,
    string? CalibrationAuthority = null,
    string OperatingHours = "24/7",
    string? ContactPerson = null,
    string? ContactPhone = null,
    decimal? ServiceFee = null,
    string Currency = "USD",
    bool IsActive = true,
    string? Notes = null
);

public record WeighbridgeSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal MaxCapacity { get; init; }
    public decimal MinCapacity { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastCalibrationDate { get; init; }
    public DateTime? NextCalibrationDate { get; init; }
    public bool IsCalibrationDue { get; init; }
    public int TransactionCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record WeighbridgeDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal MaxCapacity { get; init; }
    public decimal MinCapacity { get; init; }
    public decimal Accuracy { get; init; }
    public string Manufacturer { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public DateTime InstallationDate { get; init; }
    public DateTime? LastCalibrationDate { get; init; }
    public DateTime? NextCalibrationDate { get; init; }
    public DateTime? LastMaintenanceDate { get; init; }
    public DateTime? NextMaintenanceDate { get; init; }
    public string? CertificateNumber { get; init; }
    public DateTime? CertificateExpiryDate { get; init; }
    public string? CalibrationAuthority { get; init; }
    public string OperatingHours { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? ContactPhone { get; init; }
    public decimal? ServiceFee { get; init; }
    public string Currency { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid OrganizationId { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool HasCalibrations { get; init; }
    public bool HasMaintenanceRecords { get; init; }
    public bool HasTransactions { get; init; }
    public bool HasDocuments { get; init; }
    public bool IsCalibrationDue { get; init; }
    public bool IsMaintenanceDue { get; init; }
}

// Weighbridge Calibration DTOs
public record CreateWeighbridgeCalibrationDto(
    DateTime CalibrationDate,
    string CalibrationBy,
    string CertificateNumber,
    DateTime CertificateExpiryDate,
    string CalibrationAuthority,
    decimal CalibrationCost,
    string Status = "Passed",
    decimal? AccuracyAchieved = null,
    string? TestWeights = null,
    string? TestResults = null,
    string? Adjustments = null,
    DateTime? NextCalibrationDate = null,
    string? Notes = null
);

public record WeighbridgeCalibrationDto
{
    public Guid Id { get; init; }
    public Guid WeighbridgeId { get; init; }
    public DateTime CalibrationDate { get; init; }
    public string CalibrationBy { get; init; } = string.Empty;
    public string CertificateNumber { get; init; } = string.Empty;
    public DateTime CertificateExpiryDate { get; init; }
    public string CalibrationAuthority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal? AccuracyAchieved { get; init; }
    public string? TestWeights { get; init; }
    public string? TestResults { get; init; }
    public string? Adjustments { get; init; }
    public decimal CalibrationCost { get; init; }
    public DateTime? NextCalibrationDate { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsExpired { get; init; }
    public int DaysToExpiry { get; init; }
}

// Weighbridge Maintenance DTOs
public record CreateWeighbridgeMaintenanceDto(
    DateTime MaintenanceDate,
    string MaintenanceType,
    string ServiceProvider,
    decimal ServiceCost,
    decimal PartsCost,
    decimal TotalCost,
    string? WorkOrderNumber = null,
    string? WorkPerformed = null,
    string? PartsReplaced = null,
    string Status = "Completed",
    DateTime? NextMaintenanceDate = null,
    string? Warranty = null,
    string? TechnicianName = null,
    string? Notes = null
);

public record WeighbridgeMaintenanceDto
{
    public Guid Id { get; init; }
    public Guid WeighbridgeId { get; init; }
    public DateTime MaintenanceDate { get; init; }
    public string MaintenanceType { get; init; } = string.Empty;
    public string ServiceProvider { get; init; } = string.Empty;
    public string? WorkOrderNumber { get; init; }
    public string? WorkPerformed { get; init; }
    public string? PartsReplaced { get; init; }
    public decimal ServiceCost { get; init; }
    public decimal PartsCost { get; init; }
    public decimal TotalCost { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? NextMaintenanceDate { get; init; }
    public string? Warranty { get; init; }
    public string? TechnicianName { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsWarrantyActive { get; init; }
}

// Weighbridge Transaction DTOs
public record CreateWeighbridgeTransactionDto(
    string TicketNumber,
    DateTime TransactionDate,
    decimal GrossWeight,
    decimal TareWeight,
    decimal NetWeight,
    string TransactionType = "Weighing",
    string? VehicleNumber = null,
    string? DriverName = null,
    string? CustomerName = null,
    string? ProductType = null,
    string WeightUnit = "kg",
    decimal? ServiceFee = null,
    string? PaymentMethod = null,
    string? OperatorName = null,
    string? Comments = null
);

public record WeighbridgeTransactionDto
{
    public Guid Id { get; init; }
    public Guid WeighbridgeId { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public string? VehicleNumber { get; init; }
    public string? DriverName { get; init; }
    public string? CustomerName { get; init; }
    public string? ProductType { get; init; }
    public decimal GrossWeight { get; init; }
    public decimal TareWeight { get; init; }
    public decimal NetWeight { get; init; }
    public string WeightUnit { get; init; } = string.Empty;
    public decimal? ServiceFee { get; init; }
    public string? PaymentMethod { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? OperatorName { get; init; }
    public string? Comments { get; init; }
    public string? ReceiptUrl { get; init; }
    public bool IsPrinted { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Weighbridge Document DTOs
public record CreateWeighbridgeDocumentDto(
    string FileName,
    string OriginalFileName,
    string ContentType,
    string FilePath,
    long FileSize,
    string UploadedBy,
    string Category = "General",
    string? Description = null,
    DateTime? ExpiryDate = null,
    string? FileUrl = null
);

public record WeighbridgeDocumentDto
{
    public Guid Id { get; init; }
    public Guid WeighbridgeId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? FileUrl { get; init; }
    public long FileSize { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public bool IsActive { get; init; }
    public string UploadedBy { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public bool IsExpired { get; init; }
}