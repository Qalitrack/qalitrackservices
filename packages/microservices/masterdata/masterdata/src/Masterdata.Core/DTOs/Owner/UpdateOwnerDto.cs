using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Masterdata.Core.Enums;

namespace Masterdata.Core.DTOs.Owner;

public class UpdateOwnerDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string Name { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Owner type is required")]
    public OwnerType Type { get; set; }
    
    // Company specific
    [StringLength(100, ErrorMessage = "Business registration number cannot be longer than 100 characters")]
    public string? BusinessRegistrationNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "Tax identification number cannot be longer than 100 characters")]
    public string? TaxIdentificationNumber { get; set; }
    
    // Sacco specific
    [StringLength(50, ErrorMessage = "Registration number cannot be longer than 50 characters")]
    public string? RegistrationNumber { get; set; }
    
    public DateTime? RegistrationDate { get; set; }
    
    [StringLength(100, ErrorMessage = "Contact person cannot be longer than 100 characters")]
    public string? ContactPerson { get; set; }
    
    // Individual specific
    [StringLength(50, ErrorMessage = "National ID cannot be longer than 50 characters")]
    public string? NationalId { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(10, ErrorMessage = "Gender cannot be longer than 10 characters")]
    public string? Gender { get; set; }
    
    // Additional contact info
    public JsonDocument? ContactInfo { get; set; }
}
