namespace ProductService.Core.DTOs;

public class ProductSpecificationDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateProductSpecificationsRequest
{
    public List<ProductSpecificationDto> Specifications { get; set; } = new List<ProductSpecificationDto>();
}