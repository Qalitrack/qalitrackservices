using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;

namespace ProductService.Core.Services;

public class PricingService : IPricingService
{
    private readonly IPricingRepository _pricingRepository;
    private readonly IMapper _mapper;

    public PricingService(IPricingRepository pricingRepository, IMapper mapper)
    {
        _pricingRepository = pricingRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PricingDto>> GetAllAsync()
    {
        var pricings = await _pricingRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PricingDto>>(pricings);
    }

    public async Task<PricingDto?> GetByIdAsync(string id)
    {
        var pricing = await _pricingRepository.GetByIdAsync(id);
        return pricing == null ? null : _mapper.Map<PricingDto>(pricing);
    }

    public async Task<PricingDto> CreateAsync(PricingDto dto)
    {
        var pricing = _mapper.Map<Pricing>(dto);
        pricing.CreatedAt = DateTime.UtcNow;
        pricing.UpdatedAt = DateTime.UtcNow;
        
        var createdPricing = await _pricingRepository.CreateAsync(pricing);
        return _mapper.Map<PricingDto>(createdPricing);
    }

    public async Task<PricingDto?> UpdateAsync(string id, PricingDto dto)
    {
        var existingPricing = await _pricingRepository.GetByIdAsync(id);
        if (existingPricing == null)
        {
            return null;
        }

        _mapper.Map(dto, existingPricing);
        existingPricing.UpdatedAt = DateTime.UtcNow;
        
        var updatedPricing = await _pricingRepository.UpdateAsync(existingPricing);
        return updatedPricing == null ? null : _mapper.Map<PricingDto>(updatedPricing);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _pricingRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PricingDto>> GetByProductIdAsync(string productId)
    {
        var pricings = await _pricingRepository.GetByProductIdAsync(productId);
        return _mapper.Map<IEnumerable<PricingDto>>(pricings);
    }

    public async Task<PricingDto?> GetCustomerPricingAsync(string productId, string customerId)
    {
        var pricing = await _pricingRepository.GetCustomerPricingAsync(productId, customerId);
        return pricing == null ? null : _mapper.Map<PricingDto>(pricing);
    }

    public async Task<decimal> CalculateEffectivePriceAsync(string productId, string? customerId = null, int quantity = 1)
    {
        // Get all applicable pricing rules for the product
        var pricings = await _pricingRepository.GetByProductIdAsync(productId);
        var applicablePricings = pricings.Where(p => p.IsValid).ToList();

        // Filter by customer if specified
        if (!string.IsNullOrEmpty(customerId))
        {
            var customerSpecific = applicablePricings.FirstOrDefault(p => p.CustomerId == customerId);
            if (customerSpecific != null)
            {
                return ApplyQuantityDiscount(customerSpecific, quantity);
            }
        }

        // Get tiered pricing based on quantity
        var tieredPricing = applicablePricings
            .Where(p => p.Type == PricingType.Tiered && quantity >= p.MinQuantity && 
                       (!p.MaxQuantity.HasValue || quantity <= p.MaxQuantity.Value))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();

        if (tieredPricing != null)
        {
            return ApplyQuantityDiscount(tieredPricing, quantity);
        }

        // Get standard pricing
        var standardPricing = applicablePricings
            .Where(p => p.Type == PricingType.Standard)
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();

        return standardPricing?.EffectivePrice ?? 0;
    }

    private static decimal ApplyQuantityDiscount(Pricing pricing, int quantity)
    {
        var basePrice = pricing.EffectivePrice;
        
        if (pricing.DiscountPercentage.HasValue)
        {
            return basePrice * (1 - pricing.DiscountPercentage.Value / 100);
        }
        
        if (pricing.DiscountAmount.HasValue)
        {
            return Math.Max(0, basePrice - pricing.DiscountAmount.Value);
        }
        
        return basePrice;
    }
}