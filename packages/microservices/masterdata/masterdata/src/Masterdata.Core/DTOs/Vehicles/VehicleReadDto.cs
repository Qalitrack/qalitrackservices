namespace Masterdata.Core.DTOs.Vehicles;

public class VehicleReadDto
{
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Color { get; set; }
    public string? Model { get; set; }
    public string Status { get; set; } = "active";
    
    // References
    public string? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    
    public string? TransporterId { get; set; }
    public string? TransporterName { get; set; }
    
    public string? DriverId { get; set; }
    public string? DriverName { get; set; }
    
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    
    public string? AxleConfigurationId { get; set; }
    public string? AxleConfigurationName { get; set; }
    
    // Collections - using simple string list for driver IDs
    public ICollection<string> AssignedDriverIds { get; set; } = new List<string>();
}
