using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;

/// <summary>
/// Quality specifications and test requirements for products
/// (Compressive Strength, Fineness, Chemical Composition, etc.)
/// </summary>
public class ProductSpecification : BaseEntity
{
    public Guid ProductBaseId { get; set; }
    public string SpecCategory { get; set; } = string.Empty; // Physical, Chemical, Performance
    public string SpecName { get; set; } = string.Empty; // "Compressive Strength 28-day"
    public string? SpecUnit { get; set; } // MPa, m²/kg, %
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TypicalValue { get; set; }
    public string? TestMethod { get; set; } // ASTM C109, IS 4031, etc.
    public bool IsMandatory { get; set; } = true;

    // Navigation properties
    public virtual ProductBase ProductBase { get; set; } = null!;
}