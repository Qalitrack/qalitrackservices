using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Types of product packaging (50kg Bags, 25kg Bags, Bulk)
/// </summary>
public class PackagingType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty; // bags, tons, pieces
    public bool RequiresContainer { get; set; } = false;
    public string? HandlingEquipment { get; set; } // "Forklift, Conveyor", "Manual, Forklift", etc.
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}