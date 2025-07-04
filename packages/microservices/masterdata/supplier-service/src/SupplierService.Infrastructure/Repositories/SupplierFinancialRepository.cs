using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierFinancialRepository : Repository<SupplierFinancial>, ISupplierFinancialRepository
{
    public SupplierFinancialRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<SupplierFinancial?> GetBySupplierIdAsync(string supplierId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.SupplierId == supplierId && !f.IsDeleted);
    }

    public override async Task<IEnumerable<SupplierFinancial>> GetAllAsync()
    {
        return await _dbSet.Where(f => !f.IsDeleted).ToListAsync();
    }

    public override async Task<SupplierFinancial?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
    }
}