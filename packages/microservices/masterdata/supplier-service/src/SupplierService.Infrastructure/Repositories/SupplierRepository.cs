using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<Supplier?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Code == code && !s.IsDeleted);
    }

    public async Task<bool> IsCodeUniqueAsync(string code)
    {
        return !await _dbSet.AnyAsync(s => s.Code == code && !s.IsDeleted);
    }

    public async Task<bool> IsNameUniqueAsync(string name)
    {
        return !await _dbSet.AnyAsync(s => s.Name == name && !s.IsDeleted);
    }

    public async Task<IEnumerable<Supplier>> GetByStatusAsync(SupplierStatus status)
    {
        return await _dbSet.Where(s => s.Status == status && !s.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> GetByTypeAsync(SupplierType type)
    {
        return await _dbSet.Where(s => s.Type == type && !s.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(s => (s.Name.ToLower().Contains(searchTerm) ||
                        s.Code.ToLower().Contains(searchTerm) ||
                        s.Email.ToLower().Contains(searchTerm) ||
                        s.ContactPerson.ToLower().Contains(searchTerm) ||
                        s.TaxIdentificationNumber.ToLower().Contains(searchTerm)) && !s.IsDeleted)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _dbSet
            .Include(s => s.Addresses.Where(a => !a.IsDeleted))
            .Where(s => !s.IsDeleted)
            .ToListAsync();
    }

    public override async Task<Supplier?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Addresses.Where(a => !a.IsDeleted))
            .Include(s => s.Products.Where(p => !p.IsDeleted))
            .Include(s => s.PerformanceMetrics.Where(pm => !pm.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }
}