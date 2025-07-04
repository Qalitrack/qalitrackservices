using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(string categoryId);
    Task<IEnumerable<Product>> GetHazardousProductsAsync();
    Task<Product?> GetByCodeAsync(string code);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    Task<IEnumerable<Product>> GetProductsWithSpecificationsAsync();
    Task<IEnumerable<Product>> GetProductsWithPricingAsync();
}