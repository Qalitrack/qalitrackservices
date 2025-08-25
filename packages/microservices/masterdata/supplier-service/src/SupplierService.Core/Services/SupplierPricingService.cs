using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class SupplierPricingService : ISupplierPricingService
{
    private readonly ISupplierPricingRepository _pricingRepository;
    private readonly ISupplierProductRepository _supplierProductRepository;
    private readonly IMapper _mapper;

    public SupplierPricingService(
        ISupplierPricingRepository pricingRepository,
        ISupplierProductRepository supplierProductRepository,
        IMapper mapper)
    {
        _pricingRepository = pricingRepository;
        _supplierProductRepository = supplierProductRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetAllAsync(string supplierProductId)
    {
        var pricing = await _pricingRepository.GetBySupplierProductId(supplierProductId);
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<SupplierPricingDto?> GetByIdAsync(string id)
    {
        var pricing = await _pricingRepository.GetByIdAsync(id);
        return pricing == null ? null : _mapper.Map<SupplierPricingDto>(pricing);
    }

    public async Task<SupplierPricingDto> CreateAsync(string supplierProductId, CreateSupplierPricingDto dto)
    {
        // Verify supplier product exists
        var supplierProduct = await _supplierProductRepository.GetByIdAsync(supplierProductId);
        if (supplierProduct == null)
            throw new InvalidOperationException($"Supplier product with ID '{supplierProductId}' not found");

        var pricing = _mapper.Map<SupplierPricing>(dto);
        pricing.SupplierProductId = supplierProductId;

        var createdPricing = await _pricingRepository.AddAsync(pricing);
        return _mapper.Map<SupplierPricingDto>(createdPricing);
    }

    public async Task<SupplierPricingDto?> UpdateAsync(string id, UpdateSupplierPricingDto dto)
    {
        var pricing = await _pricingRepository.GetByIdAsync(id);
        if (pricing == null)
            return null;

        _mapper.Map(dto, pricing);
        pricing.UpdatedAt = DateTime.UtcNow;

        var updatedPricing = await _pricingRepository.UpdateAsync(pricing);
        return _mapper.Map<SupplierPricingDto>(updatedPricing);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var pricing = await _pricingRepository.GetByIdAsync(id);
        if (pricing == null)
            return false;

        await _pricingRepository.DeleteAsync(id);
        return true;
    }

    public async Task<SupplierPricingDto?> GetActivePricingAsync(string supplierProductId, int quantity = 1)
    {
        var pricing = await _pricingRepository.GetActivePricing(supplierProductId, quantity);
        return pricing == null ? null : _mapper.Map<SupplierPricingDto>(pricing);
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetByPricingTypeAsync(string supplierProductId, PricingType type)
    {
        var pricing = await _pricingRepository.GetActivePricingsByType(supplierProductId, type);
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetPromotionalPricingAsync(string supplierProductId)
    {
        var pricing = await _pricingRepository.GetPromotionalPricing(supplierProductId);
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetExpiringPricingAsync(int daysAhead = 7)
    {
        var pricing = await _pricingRepository.GetExpiringPricing(daysAhead);
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<decimal> CalculateEffectivePriceAsync(string supplierProductId, int quantity = 1)
    {
        var pricing = await _pricingRepository.GetActivePricing(supplierProductId, quantity);
        if (pricing == null)
            return 0;

        var effectivePrice = pricing.Amount;

        // Apply discount if available
        if (pricing.DiscountPercentage.HasValue)
        {
            effectivePrice -= effectivePrice * (pricing.DiscountPercentage.Value / 100);
        }
        else if (pricing.DiscountAmount.HasValue)
        {
            effectivePrice -= pricing.DiscountAmount.Value;
        }

        return Math.Max(0, effectivePrice); // Ensure price doesn't go negative
    }

    public async Task<SupplierPricingDto> CreatePricingAsync(CreateSupplierPricingDto dto)
    {
        var pricing = _mapper.Map<SupplierPricing>(dto);
        var createdPricing = await _pricingRepository.AddAsync(pricing);
        return _mapper.Map<SupplierPricingDto>(createdPricing);
    }

    public async Task<SupplierPricingDto?> UpdatePricingAsync(string id, UpdateSupplierPricingDto dto)
    {
        var pricing = await _pricingRepository.GetByIdAsync(id);
        if (pricing == null)
            return null;

        _mapper.Map(dto, pricing);
        pricing.UpdatedAt = DateTime.UtcNow;

        var updatedPricing = await _pricingRepository.UpdateAsync(pricing);
        return _mapper.Map<SupplierPricingDto>(updatedPricing);
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetPricingByProductAsync(string supplierProductId)
    {
        var pricing = await _pricingRepository.GetBySupplierProductIdAsync(supplierProductId);
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetActivePricingByProductAsync(string supplierProductId)
    {
        var pricing = await _pricingRepository.GetActiveBySupplierProductIdAsync(supplierProductId);
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<SupplierPricingDto?> GetCurrentPricingAsync(string supplierProductId)
    {
        var pricing = await _pricingRepository.GetCurrentPricingAsync(supplierProductId);
        return pricing == null ? null : _mapper.Map<SupplierPricingDto>(pricing);
    }

    public async Task<bool> DeletePricingAsync(string id)
    {
        var pricing = await _pricingRepository.GetByIdAsync(id);
        if (pricing == null)
            return false;

        await _pricingRepository.DeleteAsync(id);
        return true;
    }

    // Additional methods expected by controllers
    public async Task<IEnumerable<SupplierPricingDto>> GetAllAsync()
    {
        var pricing = await _pricingRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierPricingDto>>(pricing);
    }

    public async Task<SupplierPricingDto> CreateAsync(CreateSupplierPricingDto dto)
    {
        return await CreatePricingAsync(dto);
    }

    public async Task<IEnumerable<SupplierPricingDto>> GetActivePricingAsync(string supplierProductId)
    {
        return await GetActivePricingByProductAsync(supplierProductId);
    }

    public async Task<decimal> CalculateEffectivePriceAsync(string supplierProductId, int quantity, DateTime? effectiveDate = null)
    {
        return await CalculateEffectivePriceAsync(supplierProductId, quantity);
    }

}