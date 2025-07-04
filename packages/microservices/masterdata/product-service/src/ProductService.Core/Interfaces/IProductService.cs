using ProductService.Core.DTOs;

namespace ProductService.Core.Interfaces;

public interface IProductService
{
    // Product Management
    Task<ProductDto> RegisterProductAsync(RegisterProductRequest request);
    Task<ProductDto> UpdateProductAsync(string id, UpdateProductRequest request);
    Task<ProductDto?> GetProductByIdAsync(string id);
    Task<ProductDto?> GetProductByCodeAsync(string code);
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<List<ProductDto>> GetProductsByCategoryAsync(string categoryId);
    Task<List<ProductDto>> GetHazardousProductsAsync();
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
    Task DeleteProductAsync(string id);

    // Category Management
    Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequest request);
    Task<List<ProductCategoryDto>> GetAllCategoriesAsync();
    Task<List<ProductCategoryDto>> GetRootCategoriesAsync();
    Task<List<ProductCategoryDto>> GetSubCategoriesAsync(string parentCategoryId);

    // Specification Management
    Task<List<ProductSpecificationDto>> GetProductSpecificationsAsync(string productId);
    Task UpdateProductSpecificationsAsync(string productId, UpdateProductSpecificationsRequest request);

    // Pricing Management
    Task<List<ProductPricingDto>> GetProductPricingAsync(string productId);
    Task UpdateProductPricingAsync(string productId, UpdateProductPricingRequest request);

    // Compliance Management
    Task<ProductComplianceDto> GetProductComplianceAsync(string productId);
}