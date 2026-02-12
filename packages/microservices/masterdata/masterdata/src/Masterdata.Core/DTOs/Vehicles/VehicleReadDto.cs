using System.ComponentModel.DataAnnotations;
using Masterdata.Core.Enums;

namespace Masterdata.Core.DTOs.Vehicles;

public class VehicleReadDto
{
    // Basic Information
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    
    // Identification
    [Required]
    [MaxLength(20)]
    public string RegistrationNumber { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = null!;
    
    [MaxLength(50)]
    public string? Make { get; set; }
    
    [MaxLength(50)]
    public string? Model { get; set; }
    
    public int? YearOfManufacture { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; }
    
    [MaxLength(50)]
    public string? ChassisNumber { get; set; }
    
    [MaxLength(50)]
    public string? EngineNumber { get; set; }
    
    // Status and Classification
    [MaxLength(20)]
    public string Status { get; set; } = "Active";
    
    [MaxLength(50)]
    public string? VehicleClass { get; set; }
    
    [MaxLength(50)]
    public string? BodyType { get; set; }
    
    // Technical Specifications
    public decimal? GrossWeight { get; set; } // in kg
    public decimal? TareWeight { get; set; } // in kg
    public decimal? NetWeightCapacity { get; set; } // in kg
    public int? SeatingCapacity { get; set; }
    public decimal? FuelTankCapacity { get; set; } // in liters

    // Insurance and Registration
    [MaxLength(50)]
    public string? InsurancePolicyNumber { get; set; }
    
    public DateTime? InsuranceExpiryDate { get; set; }
    
    [MaxLength(50)]
    public string? RoadWorthinessNumber { get; set; }
    
    public DateTime? RoadWorthinessExpiryDate { get; set; }
    
    // References
    public string? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    
    public string? TransporterId { get; set; }
    public string? TransporterName { get; set; }
    
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public OwnerType OwnerType { get; set; }
    
    public string? AxleConfigurationId { get; set; }
    public string? AxleConfigurationName { get; set; }
    public string? NfCcode { get; set; }

    
    // Driver assignments
    public List<string> DriverIds { get; set; } = new();
    public List<string> DriverNames { get; set; } = new();
    public ICollection<string> AssignedDriverIds { get; set; } = new List<string>();
}
