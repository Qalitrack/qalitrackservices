using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface IProductCategoryRepository : IRepository<ProductCategory>
{
    Task<IEnumerable<ProductCategory>> GetRootCategoriesAsync();
    Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(string parentCategoryId);
    Task<ProductCategory?> GetByCodeAsync(string code);
    Task<IEnumerable<ProductCategory>> GetActiveCategoriesAsync();
}