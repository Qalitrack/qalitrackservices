namespace QaliTrack.MasterData.Core.Modules.Vehicle.DTOs;

/// <summary>
/// Simplified DTO for creating a vehicle - only essential fields
/// </summary>
public class CreateVehicleDto
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public Guid VehicleTypeId { get; set; }
    public Guid OrganizationId { get; set; }
    
    // Optional basic fields
    public string? EngineNumber { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? PayloadCapacity { get; set; }
    public decimal? CurrentMileage { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating basic vehicle information
/// </summary>
public class UpdateVehicleDto
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal? CurrentMileage { get; set; }
    
    // Optional fields
    public string? EngineNumber { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? PayloadCapacity { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Simplified DTO for vehicle list view
/// </summary>
public class VehicleSummaryDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string VehicleTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? CurrentMileage { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for single vehicle view - without child collections
/// </summary>
public class VehicleDetailDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal TareWeight { get; set; }
    public decimal PayloadCapacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public decimal? CurrentMileage { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Status flags
    public bool HasRegistration { get; set; }
    public bool HasInsurance { get; set; }
    public bool HasSpecification { get; set; }
    public bool InspectionDue { get; set; }
}

// Vehicle Type DTOs
public class VehicleTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? MaxWeightCapacity { get; set; }
    public decimal? MaxVolumeCapacity { get; set; }
    public string? RequiredLicenseClass { get; set; }
    public bool RequiresSpecialLicense { get; set; }
    public bool IsActive { get; set; }
}

// Vehicle Registration DTOs
public class CreateVehicleRegistrationDto
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal? RegistrationFee { get; set; }
}

public class VehicleRegistrationDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? RegistrationFee { get; set; }
    public DateTime? LastRenewalDate { get; set; }
    public string? RenewalStatus { get; set; }
    public bool IsExpired { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Vehicle Specification DTOs
public class CreateVehicleSpecificationDto
{
    public Guid VehicleId { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal EngineCapacity { get; set; }
    public int EnginePower { get; set; }
    public decimal FuelTankCapacity { get; set; }
    public int SeatingCapacity { get; set; }
    public string TransmissionType { get; set; } = string.Empty;
    public string DriveType { get; set; } = string.Empty;
}

public class VehicleSpecificationDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Wheelbase { get; set; }
    public decimal GroundClearance { get; set; }
    public string TransmissionType { get; set; } = string.Empty;
    public int NumberOfGears { get; set; }
    public string DriveType { get; set; } = string.Empty;
    public decimal EngineCapacity { get; set; }
    public int EnginePower { get; set; }
    public int EngineTorque { get; set; }
    public decimal FuelTankCapacity { get; set; }
    public decimal FuelConsumption { get; set; }
    public string EmissionStandard { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public string? SpecialFeatures { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Vehicle Inspection DTOs
public class CreateVehicleInspectionDto
{
    public Guid VehicleId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string InspectionType { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionCenter { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal? Mileage { get; set; }
}

public class VehicleInspectionDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string InspectionType { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionCenter { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public decimal? Mileage { get; set; }
    public string? DefectsFound { get; set; }
    public string? RepairsRequired { get; set; }
    public string? RepairsCompleted { get; set; }
    public decimal? InspectionFee { get; set; }
    public bool IsPassed { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Vehicle Insurance DTOs
public class CreateVehicleInsuranceDto
{
    public Guid VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Deductible { get; set; }
}

public class VehicleInsuranceDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Deductible { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? CoverageDetails { get; set; }
    public string? Claims { get; set; }
    public DateTime? LastClaimDate { get; set; }
    public bool IsExpired { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Vehicle Maintenance DTOs
public class CreateVehicleMaintenanceDto
{
    public Guid VehicleId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string ServiceProvider { get; set; } = string.Empty;
    public decimal? Mileage { get; set; }
    public decimal ServiceCost { get; set; }
    public decimal PartsCost { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}

public class VehicleMaintenanceDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string ServiceProvider { get; set; } = string.Empty;
    public decimal? Mileage { get; set; }
    public string? ServicesPerformed { get; set; }
    public string? PartsReplaced { get; set; }
    public decimal ServiceCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? NextServiceDate { get; set; }
    public decimal? NextServiceMileage { get; set; }
    public string? Warranty { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Vehicle Document DTOs
public class VehicleDocumentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
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