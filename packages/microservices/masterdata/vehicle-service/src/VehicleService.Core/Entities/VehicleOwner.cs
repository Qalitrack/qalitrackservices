namespace VehicleService.Core.Entities;

public class VehicleOwner : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty; // For corporate owners
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty; // Driver's license, national ID, etc.
    public string IdentificationType { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string OwnerType { get; set; } = string.Empty; // Individual, Corporate, Government
    public bool IsActive { get; set; } = true;

    public string FullName => string.IsNullOrEmpty(CompanyName) 
        ? $"{FirstName} {LastName}".Trim() 
        : CompanyName;

    // Navigation properties - Note: This is a separate entity from Vehicle.OwnerName for normalization
    // Vehicle.OwnerName can be used for simple cases, while this entity handles complex ownership scenarios
}