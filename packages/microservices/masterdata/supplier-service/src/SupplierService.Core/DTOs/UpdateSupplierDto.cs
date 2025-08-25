using SupplierService.Core.Entities;

namespace SupplierService.Core.DTOs;

public class UpdateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public SupplierType SupplierType { get; set; }
    public SupplierStatus SupplierStatus { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Industry { get; set; }
    
    // Test compatibility properties
    public string Email 
    { 
        get => ContactEmail; 
        set => ContactEmail = value; 
    }
    
    public string? ContactPerson { get; set; }
    
    public string? Phone 
    { 
        get => ContactPhone; 
        set => ContactPhone = value; 
    }
    
    public string Type 
    { 
        get => SupplierType.ToString(); 
        set => SupplierType = Enum.TryParse<SupplierType>(value, out var result) ? result : SupplierType.Manufacturer; 
    }
    
    public string Status 
    { 
        get => SupplierStatus.ToString(); 
        set => SupplierStatus = Enum.TryParse<SupplierStatus>(value, out var result) ? result : SupplierStatus.Active; 
    }
}