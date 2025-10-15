using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Drivers;

public class UpdateDriverDto
{
    public string? FullName { get; set; }
    public string? LicenseNumber { get; set; }
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
    public string? Status { get; set; }
    private string? _transporterId;
    private string? _supplierId;

    public string? TransporterId 
    { 
        get => _transporterId;
        set => _transporterId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? SupplierId 
    { 
        get => _supplierId;
        set => _supplierId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    public IEnumerable<string>? AssignedVehicleIds { get; set; }
}
