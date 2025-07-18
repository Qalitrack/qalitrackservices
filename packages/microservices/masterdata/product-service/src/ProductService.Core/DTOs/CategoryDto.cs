using ProductService.Core.Entities;

namespace ProductService.Core.DTOs;

public class CategoryReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Hierarchical Properties
    public string? ParentCategoryId { get; set; }
    public CategoryReadDto? ParentCategory { get; set; }
    public ICollection<CategoryReadDto> SubCategories { get; set; } = new List<CategoryReadDto>();
    
    // Classification Properties
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    
    // Business Properties
    public string? ImageUrl { get; set; }
    public string? IconClass { get; set; }
    public bool IsVisible { get; set; }
    public bool AllowProducts { get; set; }
    
    // SEO and Display Properties
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public string? DisplayName { get; set; }
    
    // Helper Properties
    public bool IsRootCategory { get; set; }
    public bool HasSubCategories { get; set; }
    public bool HasProducts { get; set; }
    public int ProductCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public CategoryStatus Status { get; set; } = CategoryStatus.Active;
    
    // Hierarchical Properties
    public string? ParentCategoryId { get; set; }
    
    // Classification Properties
    public int SortOrder { get; set; } = 0;
    
    // Business Properties
    public string? ImageUrl { get; set; }
    public string? IconClass { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool AllowProducts { get; set; } = true;
    
    // SEO and Display Properties
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public string? DisplayName { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public CategoryStatus Status { get; set; } = CategoryStatus.Active;
    
    // Hierarchical Properties
    public string? ParentCategoryId { get; set; }
    
    // Classification Properties
    public int SortOrder { get; set; } = 0;
    
    // Business Properties
    public string? ImageUrl { get; set; }
    public string? IconClass { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool AllowProducts { get; set; } = true;
    
    // SEO and Display Properties
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public string? DisplayName { get; set; }
}p
ublic class CategoryReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public bool AllowProducts { get; set; }
}