using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Hierarchical product categories (Cement → OPC/PPC, Clinker, Raw Materials)
/// </summary>
public class ProductCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public virtual ProductCategory? ParentCategory { get; set; }
    public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<ProductBase> Products { get; set; } = new List<ProductBase>();
}