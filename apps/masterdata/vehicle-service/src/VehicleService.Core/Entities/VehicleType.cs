namespace VehicleService.Core.Entities;

public class VehicleType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Commercial", "Personal", "Heavy Duty"
    public decimal? MaxWeightLimit { get; set; }
    public bool RequiresSpecialLicense { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}