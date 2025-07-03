using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierContactRepository : Repository<SupplierContact>, ISupplierContactRepository
{
    public SupplierContactRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierContact>> GetBySupplierIdAsync(string supplierId)
    {
        return await _dbSet
            .Where(c => c.SupplierId == supplierId && !c.IsDeleted)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<SupplierContact?> GetPrimaryContactAsync(string supplierId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.SupplierId == supplierId && c.IsPrimary && c.IsActive && !c.IsDeleted);
    }

    public async Task<IEnumerable<SupplierContact>> GetByContactTypeAsync(string supplierId, ContactType contactType)
    {
        return await _dbSet
            .Where(c => c.SupplierId == supplierId && c.ContactType == contactType && c.IsActive && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<SupplierContact?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);
    }

    public override async Task<IEnumerable<SupplierContact>> GetAllAsync()
    {
        return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
    }

    public override async Task<SupplierContact?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }
}