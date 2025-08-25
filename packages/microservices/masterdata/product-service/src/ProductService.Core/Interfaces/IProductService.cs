using ProductService.Core.DTOs;
using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductReadDto>> GetAllAsync();
    Task<ProductReadDto?> GetByIdAsync(string id);
    Task<ProductReadDto> CreateAsync(CreateProductDto dto);
    Task<ProductReadDto?> UpdateAsync(string id, UpdateProductDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // Inventory Management
    Task<bool> IsAvailableAsync(string productId, int quantity = 1);
    Task<int> GetAvailableStockAsync(string productId);
    Task<bool> UpdateStockAsync(string productId, int quantity);
    Task<bool> ReserveStockAsync(string productId, int quantity);
    Task<bool> ReleaseStockAsync(string productId, int quantity);
    Task<IEnumerable<ProductReadDto>> GetLowStockProductsAsync();
    
    // Search and Filtering
    Task<IEnumerable<ProductReadDto>> SearchAsync(string searchTerm);
    Task<IEnumerable<ProductReadDto>> GetByCategoryAsync(string categoryId);
    Task<IEnumerable<ProductReadDto>> GetByStatusAsync(ProductStatus status);
    
    // Integration with Customer Service
    Task<decimal> GetCustomerPriceAsync(string productId, string customerId, int quantity = 1);
    Task<IEnumerable<ProductReadDto>> GetCustomerProductsAsync(string customerId);
}