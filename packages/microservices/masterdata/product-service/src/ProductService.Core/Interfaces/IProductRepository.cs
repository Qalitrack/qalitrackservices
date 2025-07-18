using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface IProductRepository : IRepository<ProductService.Core.Entities.Product>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<ProductService.Core.Entities.Product?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}