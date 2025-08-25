using SupplierService.Core.Entities;

namespace SupplierService.Core.DTOs;

public class SupplierReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public SupplierType SupplierType { get; set; }
    public SupplierStatus Status { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Industry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Test compatibility properties
    public string Type 
    { 
        get => SupplierType.ToString(); 
        set => SupplierType = Enum.TryParse<SupplierType>(value, out var result) ? result : SupplierType.Manufacturer; 
    }
    
    public bool IsVerified { get; set; } = false;
    
    public DateTime? VerificationDate { get; set; }
}