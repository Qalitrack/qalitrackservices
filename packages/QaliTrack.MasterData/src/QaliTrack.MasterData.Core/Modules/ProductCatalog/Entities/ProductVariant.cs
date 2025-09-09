using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Product variants combining base products with packaging types
/// (OPC 42.5N in 50kg bags vs OPC 42.5N in 25kg bags)
/// </summary>
public class ProductVariant : BaseEntity
{
    public Guid ProductBaseId { get; set; }
    public Guid PackagingTypeId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public decimal? UnitWeight { get; set; } // Weight per unit (50.0 for 50kg bags)
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = "KSH";
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ProductBase ProductBase { get; set; } = null!;
    public virtual PackagingType PackagingType { get; set; } = null!;
    public virtual ICollection<ProductUsagePermission> UsagePermissions { get; set; } = new List<ProductUsagePermission>();
}