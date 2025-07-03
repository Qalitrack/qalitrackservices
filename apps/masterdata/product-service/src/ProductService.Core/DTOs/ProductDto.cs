namespace ProductService.Core.DTOs;

public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Density { get; set; }
    public bool IsHazardous { get; set; }
    public string? HazmatClass { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RegisterProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Density { get; set; }
    public bool IsHazardous { get; set; }
    public string? HazmatClass { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Density { get; set; }
    public bool IsHazardous { get; set; }
    public string? HazmatClass { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}