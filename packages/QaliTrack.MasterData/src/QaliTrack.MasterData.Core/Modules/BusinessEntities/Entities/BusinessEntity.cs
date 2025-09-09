using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;

/// <summary>
/// Core business entity that can serve as Customer, Supplier, and/or Transporter
/// This replaces the separate Customer, Supplier, and Transporter entities from the original microservices
/// </summary>
public class BusinessEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string EntityType { get; set; } = "Active";
    public string Status { get; set; } = "Active";
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Industry { get; set; }
    // Navigation properties
    public virtual CustomerProfile? CustomerProfile { get; set; }
    public virtual SupplierProfile? SupplierProfile { get; set; }
    public virtual ICollection<BusinessEntityContact> Contacts { get; set; } = new List<BusinessEntityContact>();
    public virtual ICollection<BusinessEntityLocation> Locations { get; set; } = new List<BusinessEntityLocation>();
    public virtual ICollection<BusinessEntityDocument> Documents { get; set; } = new List<BusinessEntityDocument>();
}

/// <summary>
/// Customer-specific profile for business entities
/// </summary>
public class CustomerProfile : BaseEntity
{
    public Guid BusinessEntityId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal UsedCredit { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public string Currency { get; set; } = "USD";
    public string? CreditRating { get; set; }
    public DateTime? LastCreditReview { get; set; }
    public DateTime? NextCreditReview { get; set; }
    public decimal SecurityDeposit { get; set; }
    public string PreferredContactMethod { get; set; } = "Active";
    public string? PreferredLanguage { get; set; }
    public string? BillingAddress { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // Navigation properties
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;
    public virtual ICollection<CustomerContract> Contracts { get; set; } = new List<CustomerContract>();
    public virtual ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
}

/// <summary>
/// Supplier-specific profile for business entities
/// </summary>
public class SupplierProfile : BaseEntity
{
    public Guid BusinessEntityId { get; set; }
    public string SupplierType { get; set; } = "Active";
    public string QualityRating { get; set; } = "A";
    public int DeliveryRating { get; set; } = 5;
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? CertificationLevel { get; set; }
    public int LeadTimeDays { get; set; } = 7;
    public decimal MinOrderValue { get; set; }
    public string PaymentTerms { get; set; } = "NET30";
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // Navigation properties
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;
    public virtual ICollection<SupplierContract> Contracts { get; set; } = new List<SupplierContract>();
    public virtual ICollection<SupplierPerformance> PerformanceRecords { get; set; } = new List<SupplierPerformance>();
}


public class BusinessEntityContact : BaseEntity
{
    public Guid BusinessEntityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string ContactType { get; set; } = "Active";
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;
}

public class BusinessEntityLocation : BaseEntity
{
    public Guid BusinessEntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string LocationType { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public string? AccessInstructions { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;
}

public class BusinessEntityDocument : BaseEntity
{
    public Guid BusinessEntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string Category { get; set; } = "Active";
    public string? Description { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;
}

// Supporting entities for profiles
public class CustomerContract : BaseEntity
{
    public Guid CustomerProfileId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string Status { get; set; } = "Active";
    public string ContractType { get; set; } = "Active";
    public string? Terms { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalPeriodMonths { get; set; }

    // Navigation properties
    public virtual CustomerProfile CustomerProfile { get; set; } = null!;
}

public class CustomerOrder : BaseEntity
{
    public Guid CustomerProfileId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // Navigation properties
    public virtual CustomerProfile CustomerProfile { get; set; } = null!;
}

public class SupplierContract : BaseEntity
{
    public Guid SupplierProfileId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Terms { get; set; }

    // Navigation properties
    public virtual SupplierProfile SupplierProfile { get; set; } = null!;
}

public class SupplierPerformance : BaseEntity
{
    public Guid SupplierProfileId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public decimal QualityScore { get; set; }
    public decimal DeliveryScore { get; set; }
    public decimal ServiceScore { get; set; }
    public decimal OverallScore { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public virtual SupplierProfile SupplierProfile { get; set; } = null!;
}


// Enumerations
public enum BusinessEntityType
{
    Individual,
    Corporate,
    Government,
    NonProfit,
    Partnership
}

public enum BusinessEntityStatus
{
    Active,
    Inactive,
    Suspended,
    Pending,
    Closed
}

public enum ContactMethod
{
    Email,
    Phone,
    SMS,
    Mail
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}

public enum SupplierType
{
    Manufacturer,
    Distributor,
    Retailer,
    ServiceProvider,
    Contractor,
    Consultant
}

public enum SupplierStatus
{
    Active,
    Inactive,
    Pending,
    Suspended,
    Blacklisted
}


public enum ContactType
{
    General,
    Technical,
    Billing,
    Emergency,
    Management
}

public enum BusinessLocationTypes
{
    Office,
    Warehouse,
    Plant,
    Service,
    Distribution
}

public enum DocumentCategory
{
    General,
    Legal,
    Financial,
    Technical,
    Insurance,
    License,
    Contract
}

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    Terminated,
    Suspended
}

public enum ContractType
{
    Service,
    Maintenance,
    Supply,
    Transport,
    Lease
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum InsuranceStatus
{
    Active,
    Expired,
    Cancelled,
    Suspended
}

/// <summary>
/// Long-term supply agreements for cement materials and products
/// Used for HODIM integration and structured procurement
/// </summary>
public class ScheduleAgreement : BaseEntity
{
    public string AgreementNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid ProductVariantId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = "KSH";
    public string DeliveryTerms { get; set; } = string.Empty;
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // Navigation properties
    public virtual SupplierProfile Supplier { get; set; } = null!;
}