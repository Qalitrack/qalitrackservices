using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Masterdata.Core.Enums;

namespace Masterdata.Core.Entities;

public class Owner : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    [Required]
    public OwnerType Type { get; set; }

    // Common fields
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(200)]
    public string? Address { get; set; }

    // Sacco-specific fields
    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }
    
    public DateTime? RegistrationDate { get; set; }
    
    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    // Company-specific fields
    [MaxLength(100)]
    public string? TaxIdentificationNumber { get; set; }
    
    [MaxLength(100)]
    public string? BusinessRegistrationNumber { get; set; }

    // Individual-specific fields
    [MaxLength(50)]
    public string? NationalId { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; }

    // Navigation properties
    [InverseProperty(nameof(Vehicle.Owner))]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    // Helper properties
    [NotMapped]
    public bool IsSacco => Type == OwnerType.Sacco;
    
    [NotMapped]
    public bool IsCompany => Type == OwnerType.Company;
    
    [NotMapped]
    public bool IsIndividual => Type == OwnerType.Individual;
}
