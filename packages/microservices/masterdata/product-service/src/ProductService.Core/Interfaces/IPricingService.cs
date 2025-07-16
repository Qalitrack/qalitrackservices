using ProductService.Core.DTOs;

namespace ProductService.Core.Interfaces;

public interface IPricingService
{
    Task<IEnumerable<PricingReadDto>> GetAllAsync();
    Task<PricingReadDto?> GetByIdAsync(string id);
    Task<PricingReadDto> CreateAsync(CreatePricingDto dto);
    Task<PricingReadDto?> UpdateAsync(string id, UpdatePricingDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<PricingReadDto>> GetByProductIdAsync(string productId);
    Task<PricingReadDto?> GetEffectivePricingAsync(string productId, string? customerId = null, string? customerGroupId = null, int quantity = 1);
    Task<IEnumerable<PricingReadDto>> GetTieredPricingAsync(string productId);
    Task<decimal> CalculateEffectivePriceAsync(string productId, int quantity, string? customerId = null);
}