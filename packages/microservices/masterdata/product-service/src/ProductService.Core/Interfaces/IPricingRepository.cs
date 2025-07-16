using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface IPricingRepository : IRepository<Pricing>
{
    Task<IEnumerable<Pricing>> GetByProductIdAsync(string productId);
    Task<IEnumerable<Pricing>> GetByCustomerIdAsync(string customerId);
    Task<IEnumerable<Pricing>> GetByCustomerGroupIdAsync(string customerGroupId);
    Task<Pricing?> GetEffectivePricingAsync(string productId, string? customerId = null, string? customerGroupId = null, int quantity = 1);
    Task<IEnumerable<Pricing>> GetTieredPricingAsync(string productId);
    Task<IEnumerable<Pricing>> GetPromotionalPricingAsync(string productId);
    Task<IEnumerable<Pricing>> GetActivePricingAsync(string productId);
    Task<IEnumerable<Pricing>> GetExpiredPricingAsync();
    Task<bool> HasActivePricingAsync(string productId);
}