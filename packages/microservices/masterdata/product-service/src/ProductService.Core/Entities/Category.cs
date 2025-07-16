namespace ProductService.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public CategoryStatus Status { get; set; } = CategoryStatus.Active;
    
    // Hierarchical Properties
    public string? ParentCategoryId { get; set; }
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    
    // Classification Properties
    public int Level { get; set; } = 0; // 0 = root level
    public string Path { get; set; } = string.Empty; // e.g., "Electronics/Computers/Laptops"
    public int SortOrder { get; set; } = 0;
    
    // Business Properties
    public string? ImageUrl { get; set; }
    public string? IconClass { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool AllowProducts { get; set; } = true; // Some categories might be just for organization
    
    // SEO and Display Properties
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public string? DisplayName { get; set; } // Different from Name for display purposes
    
    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
    // Helper Methods
    public bool IsRootCategory => string.IsNullOrEmpty(ParentCategoryId);
    public bool HasSubCategories => SubCategories.Any();
    public bool HasProducts => Products.Any();
}

public enum CategoryStatus
{
    Active,
    Inactive,
    Hidden,
    Archived
}