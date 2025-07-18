using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Category?> GetByNameAsync(string name);
    Task<Category?> GetByCodeAsync(string code);
    Task<IEnumerable<Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(string parentId);
    Task<IEnumerable<Category>> GetCategoryHierarchyAsync(string categoryId);
    Task<bool> HasSubCategoriesAsync(string categoryId);
    Task<bool> HasProductsAsync(string categoryId);
    Task<int> GetCategoryLevelAsync(string categoryId);
    Task<string> BuildCategoryPathAsync(string categoryId);
}