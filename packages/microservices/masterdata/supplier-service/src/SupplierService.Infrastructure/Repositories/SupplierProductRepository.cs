using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierProductRepository : Repository<SupplierProduct>, ISupplierProductRepository
{
    public SupplierProductRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierProduct>> GetBySupplierIdAsync(string supplierId)
    {
        return await _dbSet
            .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierProduct>> GetByProductIdAsync(string productId)
    {
        return await _dbSet
            .Where(p => p.ProductId == productId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierProduct>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(p => p.Category == category && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierProduct>> GetActiveProductsAsync(string supplierId)
    {
        return await _dbSet
            .Where(p => p.SupplierId == supplierId && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

    public override async Task<IEnumerable<SupplierProduct>> GetAllAsync()
    {
        return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
    }

    public override async Task<SupplierProduct?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }
}