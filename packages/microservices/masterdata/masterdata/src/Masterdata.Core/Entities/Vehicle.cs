using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Masterdata.Core.Enums;

namespace Masterdata.Core.Entities
{
    public class Vehicle : BaseEntity
    {
        // Basic Information
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

        // Ownership – now OPTIONAL (nullable)
        public string? OwnerId { get; set; }               // ← Changed: no [Required]

        [ForeignKey(nameof(OwnerId))]
        public virtual Owner? Owner { get; set; }          // ← Changed: nullable reference

        [NotMapped]
        public OwnerType OwnerType => Owner?.Type ?? OwnerType.Individual;

        // Relationships
        public string? SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }

        public string? TransporterId { get; set; }

        [ForeignKey(nameof(TransporterId))]
        public virtual Transporter? Transporter { get; set; }

        [Required]
        public string AxleConfigurationId { get; set; } = null!;

        [ForeignKey(nameof(AxleConfigurationId))]
        public virtual AxleConfiguration AxleConfiguration { get; set; } = null!;

        // Technical Specifications
        public decimal? GrossWeight { get; set; }           // in kg
        public decimal? TareWeight { get; set; }            // in kg
        public decimal? NetWeightCapacity { get; set; }     // in kg
        public int? SeatingCapacity { get; set; }
        public decimal? FuelTankCapacity { get; set; }      // in liters

        // Insurance and Registration
        [MaxLength(50)]
        public string? InsurancePolicyNumber { get; set; }

        public DateTime? InsuranceExpiryDate { get; set; }

        [MaxLength(50)]
        public string? RoadWorthinessNumber { get; set; }

        public string? RfiDcode { get; set; }

        public DateTime? RoadWorthinessExpiryDate { get; set; }

        // Navigation properties
        public virtual ICollection<DriverVehicle> DriverVehicles { get; set; } = new List<DriverVehicle>();

        // Helper methods
        public bool IsOwnedBySacco() => OwnerType == OwnerType.Sacco;
        public bool IsOwnedByCompany() => OwnerType == OwnerType.Company;
        public bool IsOwnedByIndividual() => OwnerType == OwnerType.Individual;
    }
}