using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductCategoryRepository : Repository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductCategory>> GetRootCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == null || c.ParentCategoryId == "")
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(string parentCategoryId)
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ProductCategory?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<IEnumerable<ProductCategory>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public override async Task<ProductCategory?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}