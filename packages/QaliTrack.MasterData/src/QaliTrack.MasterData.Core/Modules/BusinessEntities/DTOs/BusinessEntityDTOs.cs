using System.ComponentModel.DataAnnotations;

namespace QaliTrack.MasterData.Core.Modules.BusinessEntities.DTOs;

/// <summary>
/// Simplified DTO for creating a business entity - only essential fields
/// </summary>
public class CreateBusinessEntityDto
{
    [Required(ErrorMessage = "Business entity name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Business entity code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Registration number is required")]
    [StringLength(100, ErrorMessage = "Registration number cannot exceed 100 characters")]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters")]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact phone is required")]
    [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters")]
    public string ContactPhone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Entity type is required")]
    [StringLength(50, ErrorMessage = "Entity type cannot exceed 50 characters")]
    public string EntityType { get; set; } = "Company"; // Customer, Supplier, Transporter, Company
    
    // Optional basic fields
    [StringLength(50, ErrorMessage = "Tax number cannot exceed 50 characters")]
    public string? TaxNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }
    
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string? Country { get; set; }
    
    [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters")]
    [Url(ErrorMessage = "Please provide a valid website URL")]
    public string? Website { get; set; }
}

/// <summary>
/// DTO for updating basic business entity information
/// </summary>
public class UpdateBusinessEntityDto
{
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? Name { get; set; }
    
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters")]
    public string? ContactEmail { get; set; }
    
    [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters")]
    public string? ContactPhone { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
    
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string? Status { get; set; }
    
    // Optional fields
    [StringLength(50, ErrorMessage = "Tax number cannot exceed 50 characters")]
    public string? TaxNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }
    
    [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string? State { get; set; }
    
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string? Country { get; set; }
    
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string? PostalCode { get; set; }
    
    [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters")]
    [Url(ErrorMessage = "Please provide a valid website URL")]
    public string? Website { get; set; }
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
    
    public DateTime? EstablishedDate { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Employee count must be greater than 0")]
    public int? EmployeeCount { get; set; }
    
    [StringLength(100, ErrorMessage = "Industry cannot exceed 100 characters")]
    public string? Industry { get; set; }
}

/// <summary>
/// Simplified DTO for business entity list view
/// </summary>
public class BusinessEntitySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsCustomer { get; set; }
    public bool IsSupplier { get; set; }
    public bool IsTransporter { get; set; }
}

/// <summary>
/// Detailed DTO for single business entity view - without child collections
/// </summary>
public class BusinessEntityDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public string Industry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Profile flags
    public bool IsCustomer { get; set; }
    public bool IsSupplier { get; set; }
    public bool IsTransporter { get; set; }
}

// Customer Profile DTOs
public class CreateCustomerProfileDto
{
    public Guid BusinessEntityId { get; set; }
    public decimal CreditLimit { get; set; } = 0;
    public int PaymentTermsDays { get; set; } = 30;
    public string Currency { get; set; } = "USD";
    public string PreferredContactMethod { get; set; } = "Email";
    public string? BillingAddress { get; set; }
}

public class UpdateCustomerProfileDto
{
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public string Currency { get; set; } = "USD";
    public string PreferredContactMethod { get; set; } = "Email";
    public string? BillingAddress { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class PatchCustomerProfileDto
{
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermsDays { get; set; }
    public string? Currency { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? BillingAddress { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public Guid BusinessEntityId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal UsedCredit { get; set; }
    public int PaymentTermsDays { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CreditRating { get; set; } = string.Empty;
    public DateTime? LastCreditReview { get; set; }
    public DateTime? NextCreditReview { get; set; }
    public decimal SecurityDeposit { get; set; }
    public string PreferredContactMethod { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Supplier Profile DTOs
public class CreateSupplierProfileDto
{
    public Guid BusinessEntityId { get; set; }
    public string SupplierType { get; set; } = "Standard";
    public string QualityRating { get; set; } = "Unrated";
    public int LeadTimeDays { get; set; } = 7;
    public decimal MinOrderValue { get; set; } = 0;
    public string PaymentTerms { get; set; } = "Net 30";
}

public class UpdateSupplierProfileDto
{
    public string SupplierType { get; set; } = "Standard";
    public string QualityRating { get; set; } = "Unrated";
    public int LeadTimeDays { get; set; } = 7;
    public decimal MinOrderValue { get; set; } = 0;
    public string PaymentTerms { get; set; } = "Net 30";
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class PatchSupplierProfileDto
{
    public string? SupplierType { get; set; }
    public string? QualityRating { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinOrderValue { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class SupplierProfileDto
{
    public Guid Id { get; set; }
    public Guid BusinessEntityId { get; set; }
    public string SupplierType { get; set; } = string.Empty;
    public string QualityRating { get; set; } = string.Empty;
    public int DeliveryRating { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string CertificationLevel { get; set; } = string.Empty;
    public int LeadTimeDays { get; set; }
    public decimal MinOrderValue { get; set; }
    public string PaymentTerms { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Transporter Profile DTOs
public class CreateTransporterProfileDto
{
    public Guid BusinessEntityId { get; set; }
    public string TransporterType { get; set; } = "General";
    public int FleetSize { get; set; } = 0;
    public string OperatingLicense { get; set; } = string.Empty;
    public DateTime? LicenseExpiryDate { get; set; }
    public string ServiceAreas { get; set; } = string.Empty;
    public decimal BaseRate { get; set; } = 0;
    public string RateStructure { get; set; } = "Per Mile";
}

public class UpdateTransporterProfileDto
{
    public string TransporterType { get; set; } = "General";
    public int FleetSize { get; set; } = 0;
    public string OperatingLicense { get; set; } = string.Empty;
    public DateTime? LicenseExpiryDate { get; set; }
    public string ServiceAreas { get; set; } = string.Empty;
    public decimal BaseRate { get; set; } = 0;
    public string RateStructure { get; set; } = "Per Mile";
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class PatchTransporterProfileDto
{
    public string? TransporterType { get; set; }
    public int? FleetSize { get; set; }
    public string? OperatingLicense { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? ServiceAreas { get; set; }
    public decimal? BaseRate { get; set; }
    public string? RateStructure { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class TransporterProfileDto
{
    public Guid Id { get; set; }
    public Guid BusinessEntityId { get; set; }
    public string TransporterType { get; set; } = string.Empty;
    public int FleetSize { get; set; }
    public string OperatingLicense { get; set; } = string.Empty;
    public DateTime? LicenseExpiryDate { get; set; }
    public int Rating { get; set; }
    public string ServiceAreas { get; set; } = string.Empty;
    public string SpecializedServices { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public string RateStructure { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Contact DTOs
public class CreateContactDto
{
    public Guid BusinessEntityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ContactType { get; set; } = "General";
    public bool IsPrimary { get; set; } = false;
}

public class UpdateContactDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ContactType { get; set; } = "General";
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class PatchContactDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public string? ContactType { get; set; }
    public bool? IsPrimary { get; set; }
    public bool? IsActive { get; set; }
    public string? Notes { get; set; }
}

public class ContactDto
{
    public Guid Id { get; set; }
    public Guid BusinessEntityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ContactType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}