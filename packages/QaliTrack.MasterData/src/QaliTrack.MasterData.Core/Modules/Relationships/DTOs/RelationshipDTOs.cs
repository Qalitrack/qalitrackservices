namespace QaliTrack.MasterData.Core.Modules.Relationships.DTOs;

// Driver-SACCO Membership DTOs
public record CreateDriverSaccoMembershipDto(
    Guid DriverId,
    Guid SaccoId,
    DateTime MembershipDate,
    string MembershipNumber,
    decimal ShareContribution,
    string Status = "Active",
    DateTime? ExpiryDate = null,
    decimal? MonthlyContribution = null,
    string? MembershipType = null,
    string? Benefits = null,
    string? Notes = null
);

public record UpdateDriverSaccoMembershipDto(
    DateTime MembershipDate,
    string MembershipNumber,
    string Status,
    decimal ShareContribution,
    DateTime? ExpiryDate = null,
    decimal? MonthlyContribution = null,
    string? MembershipType = null,
    string? Benefits = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchDriverSaccoMembershipDto(
    DateTime? MembershipDate = null,
    string? MembershipNumber = null,
    string? Status = null,
    decimal? ShareContribution = null,
    DateTime? ExpiryDate = null,
    decimal? MonthlyContribution = null,
    string? MembershipType = null,
    string? Benefits = null,
    bool? IsActive = null,
    string? Notes = null
);

public record DriverSaccoMembershipDto
{
    public Guid Id { get; init; }
    public Guid DriverId { get; init; }
    public Guid SaccoId { get; init; }
    public DateTime MembershipDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string MembershipNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal ShareContribution { get; init; }
    public decimal? MonthlyContribution { get; init; }
    public string? MembershipType { get; init; }
    public string? Benefits { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsExpired { get; init; }
    public string? DriverName { get; init; }
    public string? SaccoName { get; init; }
}

// Vehicle-Transporter Ownership DTOs
public record CreateVehicleTransporterOwnershipDto(
    Guid VehicleId,
    Guid TransporterProfileId,
    DateTime OwnershipStartDate,
    string OwnershipType = "Owned",
    DateTime? OwnershipEndDate = null,
    decimal? PurchasePrice = null,
    decimal? CurrentValue = null,
    string? FinancingDetails = null,
    string? InsuranceDetails = null,
    string? Notes = null
);

public record UpdateVehicleTransporterOwnershipDto(
    DateTime OwnershipStartDate,
    string OwnershipType,
    DateTime? OwnershipEndDate = null,
    decimal? PurchasePrice = null,
    decimal? CurrentValue = null,
    string? FinancingDetails = null,
    string? InsuranceDetails = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchVehicleTransporterOwnershipDto(
    DateTime? OwnershipStartDate = null,
    string? OwnershipType = null,
    DateTime? OwnershipEndDate = null,
    decimal? PurchasePrice = null,
    decimal? CurrentValue = null,
    string? FinancingDetails = null,
    string? InsuranceDetails = null,
    bool? IsActive = null,
    string? Notes = null
);

public record VehicleTransporterOwnershipDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid TransporterProfileId { get; init; }
    public DateTime OwnershipStartDate { get; init; }
    public DateTime? OwnershipEndDate { get; init; }
    public string OwnershipType { get; init; } = string.Empty;
    public decimal? PurchasePrice { get; init; }
    public decimal? CurrentValue { get; init; }
    public string? FinancingDetails { get; init; }
    public string? InsuranceDetails { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? VehicleRegistrationNumber { get; init; }
    public string? TransporterName { get; init; }
}

// Driver-Vehicle Assignment DTOs
public record CreateDriverVehicleAssignmentDto(
    Guid DriverId,
    Guid VehicleId,
    DateTime AssignedDate,
    bool IsPrimary = false,
    string AssignmentType = "Regular",
    string AssignedBy = "",
    DateTime? UnassignedDate = null,
    string? Reason = null,
    string? Notes = null
);

public record UpdateDriverVehicleAssignmentDto(
    DateTime AssignedDate,
    bool IsPrimary,
    string AssignmentType,
    string AssignedBy,
    DateTime? UnassignedDate = null,
    string? Reason = null,
    string? UnassignedBy = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchDriverVehicleAssignmentDto(
    DateTime? AssignedDate = null,
    bool? IsPrimary = null,
    string? AssignmentType = null,
    string? AssignedBy = null,
    DateTime? UnassignedDate = null,
    string? Reason = null,
    string? UnassignedBy = null,
    bool? IsActive = null,
    string? Notes = null
);

public record DriverVehicleAssignmentDto
{
    public Guid Id { get; init; }
    public Guid DriverId { get; init; }
    public Guid VehicleId { get; init; }
    public DateTime AssignedDate { get; init; }
    public DateTime? UnassignedDate { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsActive { get; init; }
    public string AssignmentType { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string AssignedBy { get; init; } = string.Empty;
    public string? UnassignedBy { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? DriverName { get; init; }
    public string? VehicleRegistrationNumber { get; init; }
    public bool IsCurrentlyAssigned { get; init; }
}

// Driver-Transporter Employment DTOs
public record CreateDriverTransporterEmploymentDto(
    Guid DriverId,
    Guid TransporterProfileId,
    DateTime HireDate,
    string EmploymentType = "FullTime",
    string Status = "Active",
    DateTime? TerminationDate = null,
    decimal? Salary = null,
    string? Position = null,
    string? Department = null,
    string? ReportsTo = null,
    string? Notes = null
);

public record UpdateDriverTransporterEmploymentDto(
    DateTime HireDate,
    string EmploymentType,
    string Status,
    DateTime? TerminationDate = null,
    decimal? Salary = null,
    string? Position = null,
    string? Department = null,
    string? ReportsTo = null,
    string? TerminationReason = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchDriverTransporterEmploymentDto(
    DateTime? HireDate = null,
    string? EmploymentType = null,
    string? Status = null,
    DateTime? TerminationDate = null,
    decimal? Salary = null,
    string? Position = null,
    string? Department = null,
    string? ReportsTo = null,
    string? TerminationReason = null,
    bool? IsActive = null,
    string? Notes = null
);

public record DriverTransporterEmploymentDto
{
    public Guid Id { get; init; }
    public Guid DriverId { get; init; }
    public Guid TransporterProfileId { get; init; }
    public DateTime HireDate { get; init; }
    public DateTime? TerminationDate { get; init; }
    public string EmploymentType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal? Salary { get; init; }
    public string? Position { get; init; }
    public string? Department { get; init; }
    public string? ReportsTo { get; init; }
    public string? TerminationReason { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? DriverName { get; init; }
    public string? TransporterName { get; init; }
    public bool IsCurrentEmployment { get; init; }
}

// Product-Supplier Catalog DTOs
public record CreateProductSupplierCatalogDto(
    Guid ProductId,
    Guid SupplierProfileId,
    decimal UnitPrice,
    DateTime ValidFrom,
    string Currency = "USD",
    int MinOrderQuantity = 1,
    int? MaxOrderQuantity = null,
    int LeadTimeDays = 7,
    bool IsPreferred = false,
    DateTime? ValidTo = null,
    string? ProductCode = null,
    string? Description = null,
    string? QualityRating = null,
    decimal? Discount = null,
    string? PaymentTerms = null,
    string? Notes = null
);

public record UpdateProductSupplierCatalogDto(
    decimal UnitPrice,
    DateTime ValidFrom,
    string Currency,
    int MinOrderQuantity,
    int LeadTimeDays,
    bool IsPreferred,
    int? MaxOrderQuantity = null,
    DateTime? ValidTo = null,
    string? ProductCode = null,
    string? Description = null,
    string? QualityRating = null,
    decimal? Discount = null,
    string? PaymentTerms = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchProductSupplierCatalogDto(
    decimal? UnitPrice = null,
    DateTime? ValidFrom = null,
    string? Currency = null,
    int? MinOrderQuantity = null,
    int? MaxOrderQuantity = null,
    int? LeadTimeDays = null,
    bool? IsPreferred = null,
    DateTime? ValidTo = null,
    string? ProductCode = null,
    string? Description = null,
    string? QualityRating = null,
    decimal? Discount = null,
    string? PaymentTerms = null,
    bool? IsActive = null,
    string? Notes = null
);

public record ProductSupplierCatalogDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public Guid SupplierProfileId { get; init; }
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int MinOrderQuantity { get; init; }
    public int? MaxOrderQuantity { get; init; }
    public int LeadTimeDays { get; init; }
    public bool IsPreferred { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? ProductCode { get; init; }
    public string? Description { get; init; }
    public string? QualityRating { get; init; }
    public decimal? Discount { get; init; }
    public string? PaymentTerms { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? ProductName { get; init; }
    public string? SupplierName { get; init; }
    public bool IsCurrentlyValid { get; init; }
}

// Route-Weighbridge Association DTOs
public record CreateRouteWeighbridgeAssociationDto(
    Guid RouteId,
    Guid WeighbridgeId,
    int SequenceOrder,
    string AssociationType = "Intermediate",
    bool IsMandatory = true,
    string? Instructions = null,
    int? EstimatedDurationMinutes = null,
    decimal? Distance = null,
    string? Conditions = null,
    string? Notes = null
);

public record UpdateRouteWeighbridgeAssociationDto(
    int SequenceOrder,
    string AssociationType,
    bool IsMandatory,
    string? Instructions = null,
    int? EstimatedDurationMinutes = null,
    decimal? Distance = null,
    string? Conditions = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchRouteWeighbridgeAssociationDto(
    int? SequenceOrder = null,
    string? AssociationType = null,
    bool? IsMandatory = null,
    string? Instructions = null,
    int? EstimatedDurationMinutes = null,
    decimal? Distance = null,
    string? Conditions = null,
    bool? IsActive = null,
    string? Notes = null
);

public record RouteWeighbridgeAssociationDto
{
    public Guid Id { get; init; }
    public Guid RouteId { get; init; }
    public Guid WeighbridgeId { get; init; }
    public string AssociationType { get; init; } = string.Empty;
    public int SequenceOrder { get; init; }
    public bool IsMandatory { get; init; }
    public bool IsActive { get; init; }
    public string? Instructions { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
    public decimal? Distance { get; init; }
    public string? Conditions { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? RouteName { get; init; }
    public string? WeighbridgeName { get; init; }
}

// Vehicle-SACCO Registration DTOs
public record CreateVehicleSaccoRegistrationDto(
    Guid VehicleId,
    Guid SaccoId,
    DateTime RegistrationDate,
    string RegistrationNumber,
    decimal RegistrationFee,
    string Status = "Active",
    DateTime? ExpiryDate = null,
    string? CertificateNumber = null,
    string? Conditions = null,
    string? Notes = null
);

public record UpdateVehicleSaccoRegistrationDto(
    DateTime RegistrationDate,
    string RegistrationNumber,
    string Status,
    decimal RegistrationFee,
    DateTime? ExpiryDate = null,
    string? CertificateNumber = null,
    string? Conditions = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchVehicleSaccoRegistrationDto(
    DateTime? RegistrationDate = null,
    string? RegistrationNumber = null,
    string? Status = null,
    decimal? RegistrationFee = null,
    DateTime? ExpiryDate = null,
    string? CertificateNumber = null,
    string? Conditions = null,
    bool? IsActive = null,
    string? Notes = null
);

public record VehicleSaccoRegistrationDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid SaccoId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string RegistrationNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal RegistrationFee { get; init; }
    public string? CertificateNumber { get; init; }
    public string? Conditions { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsExpired { get; init; }
    public string? VehicleRegistrationNumber { get; init; }
    public string? SaccoName { get; init; }
}