using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Defines what each site can produce or handle
/// Controls operational capabilities per location
/// </summary>
public class SiteCapability : BaseEntity
{
    public Guid SiteId { get; set; }
    public string CapabilityType { get; set; } = string.Empty; // Production, Storage, Grinding, Distribution
    public Guid ProductCategoryId { get; set; }
    public decimal? MaxCapacityTons { get; set; }
    public string? OperationalHours { get; set; } // "24/7", "6AM-10PM"
    public bool RequiresSpecialEquipment { get; set; } = false;
    public string? EquipmentRequired { get; set; } // "Kiln, Grinding Mill", "Silos"
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ProductCategory ProductCategory { get; set; } = null!;
}