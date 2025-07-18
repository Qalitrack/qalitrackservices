using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class PricingRepository : Repository<Pricing>, IPricingRepository
{
    public PricingRepository(ProductServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pricing>> GetByProductIdAsync(string productId)
    {
        return await _context.Pricings
            .Where(p => p.ProductId == productId)
            .Include(p => p.Product)
            .OrderByDescending(p => p.Priority)
            .ThenBy(p => p.ValidFrom)
            .ToListAsync();
    }

    public async Task<Pricing?> GetCustomerPricingAsync(string productId, string customerId)
    {
        return await _context.Pricings
            .Where(p => p.ProductId == productId && 
                       p.CustomerId == customerId && 
                       p.IsActive &&
                       p.ValidFrom <= DateTime.UtcNow &&
                       (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow))
            .Include(p => p.Product)
            .OrderByDescending(p => p.Priority)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Pricing>> GetByCustomerIdAsync(string customerId)
    {
        return await _context.Pricings
            .Where(p => p.CustomerId == customerId)
            .Include(p => p.Product)
            .OrderBy(p => p.Product!.Name)
            .ThenByDescending(p => p.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pricing>> GetTieredPricingAsync(string productId)
    {
        return await _context.Pricings
            .Where(p => p.ProductId == productId && 
                       p.Type == PricingType.Tiered &&
                       p.IsActive &&
                       p.ValidFrom <= DateTime.UtcNow &&
                       (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow))
            .Include(p => p.Product)
            .OrderBy(p => p.MinQuantity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pricing>> GetPromotionalPricingAsync()
    {
        return await _context.Pricings
            .Where(p => p.IsPromotional && 
                       p.IsActive &&
                       p.ValidFrom <= DateTime.UtcNow &&
                       (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow))
            .Include(p => p.Product)
            .OrderBy(p => p.ValidTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pricing>> GetExpiringPricingAsync(int daysAhead = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Pricings
            .Where(p => p.ValidTo.HasValue && 
                       p.ValidTo.Value <= cutoffDate &&
                       p.ValidTo.Value > DateTime.UtcNow &&
                       p.IsActive)
            .Include(p => p.Product)
            .OrderBy(p => p.ValidTo)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Pricing>> GetAllAsync()
    {
        return await _context.Pricings
            .Include(p => p.Product)
            .OrderBy(p => p.Product!.Name)
            .ThenByDescending(p => p.Priority)
            .ThenBy(p => p.ValidFrom)
            .ToListAsync();
    }

    public override async Task<Pricing?> GetByIdAsync(string id)
    {
        return await _context.Pricings
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Pricing>> GetByCustomerGroupIdAsync(string customerGroupId)
    {
        return await _context.Pricings
            .Where(p => p.CustomerGroupId == customerGroupId)
            .Include(p => p.Product)
            .OrderBy(p => p.Product!.Name)
            .ThenByDescending(p => p.Priority)
            .ToListAsync();
    }

    public async Task<Pricing?> GetEffectivePricingAsync(string productId, string? customerId = null, string? customerGroupId = null, int quantity = 1)
    {
        var query = _context.Pricings
            .Where(p => p.ProductId == productId && 
                       p.IsActive &&
                       p.ValidFrom <= DateTime.UtcNow &&
                       (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow));

        // Customer-specific pricing has highest priority
        if (!string.IsNullOrEmpty(customerId))
        {
            var customerPricing = await query
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.Priority)
                .FirstOrDefaultAsync();
            if (customerPricing != null) return customerPricing;
        }

        // Customer group pricing
        if (!string.IsNullOrEmpty(customerGroupId))
        {
            var groupPricing = await query
                .Where(p => p.CustomerGroupId == customerGroupId)
                .OrderByDescending(p => p.Priority)
                .FirstOrDefaultAsync();
            if (groupPricing != null) return groupPricing;
        }

        // Tiered pricing based on quantity
        var tieredPricing = await query
            .Where(p => p.Type == PricingType.Tiered && 
                       quantity >= p.MinQuantity && 
                       (!p.MaxQuantity.HasValue || quantity <= p.MaxQuantity.Value))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefaultAsync();
        if (tieredPricing != null) return tieredPricing;

        // Standard pricing
        return await query
            .Where(p => p.Type == PricingType.Standard)
            .OrderByDescending(p => p.Priority)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Pricing>> GetPromotionalPricingAsync(string productId)
    {
        return await _context.Pricings
            .Where(p => p.ProductId == productId && 
                       p.IsPromotional && 
                       p.IsActive &&
                       p.ValidFrom <= DateTime.UtcNow &&
                       (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow))
            .Include(p => p.Product)
            .OrderBy(p => p.ValidTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pricing>> GetActivePricingAsync(string productId)
    {
        return await _context.Pricings
            .Where(p => p.ProductId == productId && 
                       p.IsActive &&
                       p.ValidFrom <= DateTime.UtcNow &&
                       (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow))
            .Include(p => p.Product)
            .OrderByDescending(p => p.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pricing>> GetExpiredPricingAsync()
    {
        return await _context.Pricings
            .Where(p => p.ValidTo.HasValue && p.ValidTo.Value < DateTime.UtcNow)
            .Include(p => p.Product)
            .OrderBy(p => p.ValidTo)
            .ToListAsync();
    }

    public async Task<bool> HasActivePricingAsync(string productId)
    {
        return await _context.Pricings
            .AnyAsync(p => p.ProductId == productId && 
                          p.IsActive &&
                          p.ValidFrom <= DateTime.UtcNow &&
                          (!p.ValidTo.HasValue || p.ValidTo.Value >= DateTime.UtcNow));
    }
}