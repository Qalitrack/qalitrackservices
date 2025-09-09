using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Vehicle.Entities;

public class Vehicle : BaseEntity
{
    public string NumberPlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty; // Truck, Pick-up, Train, etc.
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty; // Diesel, Petrol, Electric, Hybrid
    public decimal MaxWeight { get; set; }
    public decimal MaxLegalLoad { get; set; }
    public decimal MaxSafeLoad { get; set; }
    public decimal TareWeight { get; set; }
    public bool HasContainer { get; set; } = false;
    public string? ContainerType { get; set; } // Tarpaulin, Sealed, etc.
    public string Status { get; set; } = "Active";
    public bool IsBlocked { get; set; } = false;
    public string? BlockedReason { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Driver-Vehicle assignment for cement operations
/// </summary>
public class DriverVehicleAssignment : BaseEntity
{
    public Guid DriverId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? UnassignedDate { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties (will be configured in DbContext)
    // public virtual Driver Driver { get; set; } = null!;
    // public virtual Vehicle Vehicle { get; set; } = null!;
}

// Enumerations
public enum VehicleStatus
{
    Active,
    Inactive,
    InMaintenance,
    OutOfService,
    Blocked
}