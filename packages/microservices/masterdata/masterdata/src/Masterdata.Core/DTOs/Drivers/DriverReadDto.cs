using System;
using System.Collections.Generic;

namespace Masterdata.Core.DTOs.Drivers;

public class DriverReadDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
    public DateTime? LicenseExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? TransporterId { get; set; }
    public string? SupplierId { get; set; }
    public IEnumerable<string> AssignedVehicleIds { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
