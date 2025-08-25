using SupplierService.Core.DTOs;

namespace SupplierService.Core.Interfaces;

public interface ISupplierPricingService
{
    Task<SupplierPricingDto> CreatePricingAsync(CreateSupplierPricingDto dto);
    Task<SupplierPricingDto?> UpdatePricingAsync(string id, UpdateSupplierPricingDto dto);
    Task<IEnumerable<SupplierPricingDto>> GetPricingByProductAsync(string supplierProductId);
    Task<IEnumerable<SupplierPricingDto>> GetActivePricingByProductAsync(string supplierProductId);
    Task<SupplierPricingDto?> GetCurrentPricingAsync(string supplierProductId);
    Task<bool> DeletePricingAsync(string id);
    
    // Additional methods expected by controllers
    Task<IEnumerable<SupplierPricingDto>> GetAllAsync();
    Task<SupplierPricingDto?> GetByIdAsync(string id);
    Task<SupplierPricingDto?> UpdateAsync(string id, UpdateSupplierPricingDto dto);
    Task<bool> DeleteAsync(string id);
    Task<SupplierPricingDto> CreateAsync(CreateSupplierPricingDto dto);
    Task<IEnumerable<SupplierPricingDto>> GetActivePricingAsync(string supplierProductId);
    Task<decimal> CalculateEffectivePriceAsync(string supplierProductId, int quantity, DateTime? effectiveDate = null);
}