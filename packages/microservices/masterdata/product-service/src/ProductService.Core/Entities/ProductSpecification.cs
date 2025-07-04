namespace ProductService.Core.Entities;

public class ProductSpecification : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public SpecificationType Type { get; set; } = SpecificationType.Text;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
}

public enum SpecificationType
{
    Text,
    Number,
    Decimal,
    Boolean,
    Date,
    Select
}