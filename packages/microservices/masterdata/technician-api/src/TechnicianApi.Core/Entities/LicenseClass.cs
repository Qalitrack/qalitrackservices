using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class LicenseClass : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<DriverProfile> DriverProfiles { get; set; } = new List<DriverProfile>();
}
