using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Drivers;

public class CreateDriverDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot be longer than 200 characters")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "License number is required")]
    [StringLength(50, ErrorMessage = "License number cannot be longer than 50 characters")]
    public string LicenseNumber { get; set; } = null!;

    private DateTime? _licenseExpiryDate;
    
    /// <summary>
    /// The date when the driver's license expires. Must be in UTC format.
    /// Example: 2025-10-15T00:00:00Z
    /// </summary>
    [DataType(DataType.Date)]
    public DateTime? LicenseExpiryDate
    {
        get => _licenseExpiryDate;
        set => _licenseExpiryDate = value?.ToUniversalTime();
    }

    [StringLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
    public string? Status { get; set; } = "active";

    private string? _transporterId;
    private string? _supplierId;

    [StringLength(50, ErrorMessage = "Transporter ID cannot be longer than 50 characters")]
    public string? TransporterId 
    { 
        get => _transporterId;
        set => _transporterId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [StringLength(50, ErrorMessage = "Supplier ID cannot be longer than 50 characters")]
    public string? SupplierId 
    { 
        get => _supplierId;
        set => _supplierId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    // Removed AssignedVehicleIds as per requirement to assign vehicles after creation
}
