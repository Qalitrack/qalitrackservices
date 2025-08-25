using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierPricingRepository : Repository<SupplierPricing>, ISupplierPricingRepository
{
    public SupplierPricingRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierPricing>> GetBySupplierProductId(string supplierProductId)
    {
        return await _dbSet
            .Where(sp => sp.SupplierProductId == supplierProductId && !sp.IsDeleted)
            .OrderBy(sp => sp.Priority)
            .ThenBy(sp => sp.MinQuantity)
            .ToListAsync();
    }

    public async Task<SupplierPricing?> GetActivePricing(string supplierProductId, int quantity = 1)
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .Where(sp => sp.SupplierProductId == supplierProductId && 
                        sp.IsActive && 
                        sp.ValidFrom <= now && 
                        (sp.ValidTo == null || sp.ValidTo >= now) &&
                        sp.MinQuantity <= quantity &&
                        (sp.MaxQuantity == null || sp.MaxQuantity >= quantity) &&
                        !sp.IsDeleted)
            .OrderBy(sp => sp.Priority)
            .ThenBy(sp => sp.Amount)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SupplierPricing>> GetActivePricingsByType(string supplierProductId, PricingType type)
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .Where(sp => sp.SupplierProductId == supplierProductId && 
                        sp.Type == type &&
                        sp.IsActive && 
                        sp.ValidFrom <= now && 
                        (sp.ValidTo == null || sp.ValidTo >= now) &&
                        !sp.IsDeleted)
            .OrderBy(sp => sp.Priority)
            .ThenBy(sp => sp.MinQuantity)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPricing>> GetPromotionalPricing(string supplierProductId)
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .Where(sp => sp.SupplierProductId == supplierProductId && 
                        sp.Type == PricingType.Promotional &&
                        sp.IsActive && 
                        sp.ValidFrom <= now && 
                        (sp.ValidTo == null || sp.ValidTo >= now) &&
                        !sp.IsDeleted)
            .OrderBy(sp => sp.Priority)
            .ThenBy(sp => sp.Amount)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPricing>> GetExpiringPricing(int daysAhead = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        
        return await _dbSet
            .Include(sp => sp.SupplierProduct)
                .ThenInclude(sp => sp!.Supplier)
            .Where(sp => sp.ValidTo != null && 
                        sp.ValidTo <= cutoffDate && 
                        sp.IsActive &&
                        !sp.IsDeleted)
            .OrderBy(sp => sp.ValidTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPricing>> GetBySupplierProductIdAsync(string supplierProductId)
    {
        return await GetBySupplierProductId(supplierProductId);
    }

    public async Task<IEnumerable<SupplierPricing>> GetActiveBySupplierProductIdAsync(string supplierProductId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(sp => sp.SupplierProductId == supplierProductId && 
                        sp.IsActive && 
                        sp.ValidFrom <= now && 
                        (sp.ValidTo == null || sp.ValidTo >= now) &&
                        !sp.IsDeleted)
            .OrderBy(sp => sp.Priority)
            .ThenBy(sp => sp.MinQuantity)
            .ToListAsync();
    }

    public async Task<SupplierPricing?> GetCurrentPricingAsync(string supplierProductId)
    {
        return await GetActivePricing(supplierProductId, 1);
    }
}