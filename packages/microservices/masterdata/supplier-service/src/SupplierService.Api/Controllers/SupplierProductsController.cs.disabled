using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class SupplierProductsController : BaseController
{
    private readonly ISupplierProductService _supplierProductService;
    private readonly ISupplierPricingService _pricingService;

    public SupplierProductsController(
        ISupplierProductService supplierProductService,
        ISupplierPricingService pricingService)
    {
        _supplierProductService = supplierProductService;
        _pricingService = pricingService;
    }

    /// <summary>
    /// Get supplier product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierProductById(string id)
    {
        try
        {
            var product = await _supplierProductService.GetByIdAsync(id);
            return HandleResult(product);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update supplier product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplierProduct(string id, [FromBody] UpdateSupplierProductDto request)
    {
        try
        {
            var product = await _supplierProductService.UpdateAsync(id, request);
            if (product == null)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Product not found"));
            
            return Ok(ApiResponseDto<SupplierProductDto>.SuccessResponse(product, "Product updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete supplier product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplierProduct(string id)
    {
        try
        {
            var result = await _supplierProductService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Product not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Product deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    [HttpPost("{id}/stock")]
    public async Task<IActionResult> UpdateStock(string id, [FromBody] int quantity)
    {
        try
        {
            var result = await _supplierProductService.UpdateStockAsync(id, quantity);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Product not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Stock updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Set product as preferred
    /// </summary>
    [HttpPost("{id}/preferred")]
    public async Task<IActionResult> SetPreferred(string id, [FromBody] bool isPreferred)
    {
        try
        {
            var result = await _supplierProductService.SetPreferredStatusAsync(id, isPreferred);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Product not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Preferred status updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get product pricing
    /// </summary>
    [HttpGet("{id}/pricing")]
    public async Task<IActionResult> GetProductPricing(string id)
    {
        try
        {
            var pricing = await _pricingService.GetAllAsync(id);
            return Ok(ApiResponseDto<IEnumerable<SupplierPricingDto>>.SuccessResponse(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create product pricing
    /// </summary>
    [HttpPost("{id}/pricing")]
    public async Task<IActionResult> CreateProductPricing(string id, [FromBody] CreateSupplierPricingDto request)
    {
        try
        {
            var pricing = await _pricingService.CreateAsync(id, request);
            return CreatedAtAction(nameof(GetProductPricing), new { id }, 
                ApiResponseDto<SupplierPricingDto>.SuccessResponse(pricing, "Pricing created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get active pricing for quantity
    /// </summary>
    [HttpGet("{id}/pricing/active")]
    public async Task<IActionResult> GetActivePricing(string id, [FromQuery] int quantity = 1)
    {
        try
        {
            var pricing = await _pricingService.GetActivePricingAsync(id, quantity);
            return HandleResult(pricing);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Calculate effective price
    /// </summary>
    [HttpGet("{id}/pricing/calculate")]
    public async Task<IActionResult> CalculatePrice(string id, [FromQuery] int quantity = 1)
    {
        try
        {
            var price = await _pricingService.CalculateEffectivePriceAsync(id, quantity);
            return Ok(ApiResponseDto<decimal>.SuccessResponse(price));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}