using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Business rules for inter-site product transfers
/// Prevents invalid product routes between sites
/// </summary>
public class SiteProductConstraint : BaseEntity
{
    public Guid FromSiteId { get; set; }
    public Guid ToSiteId { get; set; }
    public Guid ProductCategoryId { get; set; }
    public string ConstraintType { get; set; } = string.Empty; // Allowed, Restricted, RequiresApproval
    public string? Reason { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool RequiresApproval { get; set; } = false;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ProductCategory ProductCategory { get; set; } = null!;
}