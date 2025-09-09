using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Defines which products can be sold/transferred from each site
/// Controls business rules for product-site combinations
/// </summary>
public class ProductUsagePermission : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public string UsageType { get; set; } = string.Empty; // Sale, InterPlant, Purchase
    public Guid SiteId { get; set; }
    public bool IsPermitted { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    // Navigation properties
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}