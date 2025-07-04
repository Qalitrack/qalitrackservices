using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierContractRepository : Repository<SupplierContract>, ISupplierContractRepository
{
    public SupplierContractRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierContract>> GetBySupplierIdAsync(string supplierId)
    {
        return await _dbSet
            .Where(c => c.SupplierId == supplierId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupplierContract?> GetByContractNumberAsync(string contractNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber && !c.IsDeleted);
    }

    public async Task<IEnumerable<SupplierContract>> GetByStatusAsync(ContractStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierContract>> GetExpiringContractsAsync(DateTime date)
    {
        return await _dbSet
            .Where(c => c.EndDate <= date && c.Status == ContractStatus.Active && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierContract>> GetActiveContractsAsync(string supplierId)
    {
        return await _dbSet
            .Where(c => c.SupplierId == supplierId && c.Status == ContractStatus.Active && !c.IsDeleted)
            .ToListAsync();
    }

    public override async Task<IEnumerable<SupplierContract>> GetAllAsync()
    {
        return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
    }

    public override async Task<SupplierContract?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }
}