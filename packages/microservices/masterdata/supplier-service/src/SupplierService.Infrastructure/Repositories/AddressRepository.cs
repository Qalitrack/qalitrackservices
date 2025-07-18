using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class AddressRepository : Repository<Address>, IAddressRepository
{
    public AddressRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Address>> GetBySupplierId(string supplierId)
    {
        return await _dbSet
            .Where(a => a.SupplierId == supplierId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Type)
            .ToListAsync();
    }

    public async Task<Address?> GetDefaultAddressAsync(string supplierId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.SupplierId == supplierId && a.IsDefault && !a.IsDeleted);
    }

    public async Task<IEnumerable<Address>> GetByAddressTypeAsync(string supplierId, AddressType type)
    {
        return await _dbSet
            .Where(a => a.SupplierId == supplierId && a.Type == type && !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> SetDefaultAddressAsync(string addressId, string supplierId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove default flag from all addresses for this supplier
            var existingDefaults = await _dbSet
                .Where(a => a.SupplierId == supplierId && a.IsDefault && !a.IsDeleted)
                .ToListAsync();

            foreach (var address in existingDefaults)
            {
                address.IsDefault = false;
                address.UpdatedAt = DateTime.UtcNow;
            }

            // Set the new default address
            var newDefault = await _dbSet
                .FirstOrDefaultAsync(a => a.Id == addressId && a.SupplierId == supplierId && !a.IsDeleted);

            if (newDefault == null)
                return false;

            newDefault.IsDefault = true;
            newDefault.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}