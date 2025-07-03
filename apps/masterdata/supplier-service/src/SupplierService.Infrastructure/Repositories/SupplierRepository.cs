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

    public async Task<Supplier?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Name == name && !s.IsDeleted);
    }

    public async Task<Supplier?> GetByTaxNumberAsync(string taxNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.TaxNumber == taxNumber && !s.IsDeleted);
    }

    public async Task<Supplier?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.RegistrationNumber == registrationNumber && !s.IsDeleted);
    }

    public async Task<IEnumerable<Supplier>> GetByStatusAsync(SupplierStatus status)
    {
        return await _dbSet.Where(s => s.Status == status && !s.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> GetByTypeAsync(SupplierType type)
    {
        return await _dbSet.Where(s => s.SupplierType == type && !s.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> SearchAsync(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(s => (s.Name.ToLower().Contains(searchTerm) ||
                        s.ContactEmail.ToLower().Contains(searchTerm) ||
                        (s.TaxNumber != null && s.TaxNumber.ToLower().Contains(searchTerm)) ||
                        (s.Industry != null && s.Industry.ToLower().Contains(searchTerm))) && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<Supplier?> GetWithContactsAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Contacts)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Supplier?> GetWithContractsAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Contracts)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Supplier?> GetWithProductsAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Supplier?> GetWithPerformanceAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Performance)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Supplier?> GetWithFinancialAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Financial)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Supplier?> GetCompleteAsync(string id)
    {
        return await _dbSet
            .Include(s => s.Contacts)
            .Include(s => s.Contracts)
            .Include(s => s.Products)
            .Include(s => s.Locations)
            .Include(s => s.Documents)
            .Include(s => s.Performance)
            .Include(s => s.Financial)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public override async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _dbSet.Where(s => !s.IsDeleted).ToListAsync();
    }

    public override async Task<Supplier?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }
}