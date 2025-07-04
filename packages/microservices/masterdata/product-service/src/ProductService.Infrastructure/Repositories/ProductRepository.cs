using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetHazardousProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsHazardous && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.HazmatInfo)
            .ToListAsync();
    }

    public async Task<Product?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Product>();

        return await _dbSet
            .Where(p => !p.IsDeleted && 
                       (p.Name.Contains(searchTerm) || 
                        p.Code.Contains(searchTerm) || 
                        (p.Description != null && p.Description.Contains(searchTerm))))
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsWithSpecificationsAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Specifications)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsWithPricingAsync()
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Pricing)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Specifications)
            .Include(p => p.Pricing)
            .Include(p => p.HazmatInfo)
            .Include(p => p.ComplianceRequirements)
            .Include(p => p.Inventory)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }
}