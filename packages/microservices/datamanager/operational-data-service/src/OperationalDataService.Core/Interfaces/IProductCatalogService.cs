using OperationalDataService.Core.DTOs;

namespace OperationalDataService.Core.Interfaces;

public interface IProductCatalogService
{
    Task<ProductCatalogDto> SyncProductAsync(string productId, string organizationId);
    Task<List<ProductCatalogDto>> GetActiveProductsAsync(string organizationId);
    Task<ProductValidationResult> ValidateProductForTransactionAsync(string productId, string vehicleId);
    Task<ProductPricingDto> GetProductPricingAsync(string productId, DateTime date);
    Task UpdateProductAvailabilityAsync(string productId, decimal quantity, string operation = "SET", string? reason = null);
    Task<ProductCatalogDto?> GetProductByIdAsync(string productId);
    Task<List<ProductCatalogDto>> GetProductsByCategoryAsync(string category, string organizationId);
    Task<List<ProductCatalogDto>> GetLowStockProductsAsync(string organizationId);
    Task<List<ProductCatalogDto>> SearchProductsAsync(string searchTerm, string organizationId);
    Task<bool> IsProductAvailableAsync(string productId, decimal requiredQuantity);
    Task<List<ProductCatalogDto>> GetProductsBySupplierAsync(string supplierId, string organizationId);
    Task RefreshProductCatalogAsync(string organizationId);
    Task<ProductCatalogDto> UpdateProductAsync(string productId, ProductCatalogDto productData);
    Task<bool> DeleteProductAsync(string productId);
}