using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierPricingRepository : IRepository<SupplierPricing>
{
    Task<IEnumerable<SupplierPricing>> GetBySupplierProductIdAsync(string supplierProductId);
    Task<IEnumerable<SupplierPricing>> GetActiveBySupplierProductIdAsync(string supplierProductId);
    Task<SupplierPricing?> GetCurrentPricingAsync(string supplierProductId);
    Task<IEnumerable<SupplierPricing>> GetBySupplierProductId(string supplierProductId);
    Task<SupplierPricing?> GetActivePricing(string supplierProductId, int quantity);
    Task<IEnumerable<SupplierPricing>> GetActivePricingsByType(string supplierProductId, PricingType type);
    Task<IEnumerable<SupplierPricing>> GetPromotionalPricing(string supplierProductId);
    Task<IEnumerable<SupplierPricing>> GetExpiringPricing(int daysAhead);
}