using System;
using System.Text.Json;
using Masterdata.Core.Entities;
using Masterdata.Core.Enums;

namespace Masterdata.Core.DTOs.Owner;

public class OwnerDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public OwnerType Type { get; set; }
    
    // Company specific
    public string? BusinessRegistrationNumber { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    
    // Sacco specific
    public string? RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? ContactPerson { get; set; }
    
    // Individual specific
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    
    // Common
    public JsonDocument? ContactInfo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
