using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Base product definitions (OPC 42.5N, Clinker Grade A, etc.)
/// Contains core product information before packaging variants
/// </summary>
public class ProductBase : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Grade { get; set; } // 42.5N, Grade A, etc.
    public string? ChemicalFormula { get; set; } // Ca3SiO5, etc.
    public decimal? StandardDensity { get; set; } // tons/m³
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ProductCategory Category { get; set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
}