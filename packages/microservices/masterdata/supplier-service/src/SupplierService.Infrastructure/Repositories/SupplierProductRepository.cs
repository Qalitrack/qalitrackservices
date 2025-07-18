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

    public async Task<IEnumerable<SupplierProduct>> GetBySupplierId(string supplierId)
    {
        return await _dbSet
            .Include(sp => sp.Pricing.Where(p => !p.IsDeleted))
            .Where(sp => sp.SupplierId == supplierId && !sp.IsDeleted)
            .OrderBy(sp => sp.SupplierSKU)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierProduct>> GetByProductId(string productId)
    {
        return await _dbSet
            .Include(sp => sp.Supplier)
            .Include(sp => sp.Pricing.Where(p => !p.IsDeleted))
            .Where(sp => sp.ProductId == productId && !sp.IsDeleted)
            .ToListAsync();
    }

    public async Task<SupplierProduct?> GetBySupplierAndProductId(string supplierId, string productId)
    {
        return await _dbSet
            .Include(sp => sp.Pricing.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(sp => sp.SupplierId == supplierId && sp.ProductId == productId && !sp.IsDeleted);
    }

    public async Task<IEnumerable<SupplierProduct>> GetByAvailabilityStatus(ProductAvailabilityStatus status)
    {
        return await _dbSet
            .Include(sp => sp.Supplier)
            .Where(sp => sp.Status == status && !sp.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierProduct>> GetPreferredProducts(string supplierId)
    {
        return await _dbSet
            .Include(sp => sp.Pricing.Where(p => !p.IsDeleted))
            .Where(sp => sp.SupplierId == supplierId && sp.IsPreferred && !sp.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierProduct>> GetLowStockProducts(string supplierId, int threshold = 0)
    {
        return await _dbSet
            .Where(sp => sp.SupplierId == supplierId && 
                        sp.CurrentStock <= (threshold > 0 ? threshold : sp.ReorderLevel) && 
                        !sp.IsDeleted)
            .ToListAsync();
    }

    public override async Task<SupplierProduct?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(sp => sp.Supplier)
            .Include(sp => sp.Pricing.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(sp => sp.Id == id && !sp.IsDeleted);
    }
}