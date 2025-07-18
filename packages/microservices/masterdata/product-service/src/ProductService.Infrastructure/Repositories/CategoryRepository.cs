using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ProductServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(string parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => string.IsNullOrEmpty(c.ParentCategoryId))
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByCodeAsync(string code)
    {
        return await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<bool> HasProductsAsync(string categoryId)
    {
        return await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<bool> HasSubCategoriesAsync(string categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.ParentCategoryId == categoryId);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public override async Task<Category?> GetByIdAsync(string id)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _context.Categories.AnyAsync(c => c.Name == name);
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(string categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        if (category == null) return new List<Category>();

        var hierarchy = new List<Category>();
        var current = category;
        
        while (current != null)
        {
            hierarchy.Insert(0, current);
            current = current.ParentCategory;
        }
        
        return hierarchy;
    }

    public async Task<int> GetCategoryLevelAsync(string categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId);
        return category?.Level ?? 0;
    }

    public async Task<string> BuildCategoryPathAsync(string categoryId)
    {
        var hierarchy = await GetCategoryHierarchyAsync(categoryId);
        return string.Join("/", hierarchy.Select(c => c.Name));
    }
}