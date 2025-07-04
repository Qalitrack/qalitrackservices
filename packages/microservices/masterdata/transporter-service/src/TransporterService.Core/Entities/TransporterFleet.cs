namespace TransporterService.Core.Entities;

public enum VehicleStatus
{
    Available,
    InUse,
    Maintenance,
    OutOfService,
    Retired
}

public enum VehicleType
{
    Truck,
    Trailer,
    Van,
    Container,
    Tanker,
    Flatbed,
    Refrigerated
}

public class TransporterFleet : BaseEntity
{
    public string TransporterId { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public decimal? LoadCapacity { get; set; }
    public decimal? FuelCapacity { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? CurrentDriverId { get; set; }
    public string? CurrentLocation { get; set; }
    public decimal? Mileage { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Transporter Transporter { get; set; } = null!;
}