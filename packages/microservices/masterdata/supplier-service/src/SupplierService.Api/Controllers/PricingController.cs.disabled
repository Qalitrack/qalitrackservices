using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class PricingController : BaseController
{
    private readonly ISupplierPricingService _pricingService;

    public PricingController(ISupplierPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    /// <summary>
    /// Get pricing by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPricingById(string id)
    {
        try
        {
            var pricing = await _pricingService.GetByIdAsync(id);
            return HandleResult(pricing);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update pricing
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePricing(string id, [FromBody] UpdateSupplierPricingDto request)
    {
        try
        {
            var pricing = await _pricingService.UpdateAsync(id, request);
            if (pricing == null)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Pricing not found"));
            
            return Ok(ApiResponseDto<SupplierPricingDto>.SuccessResponse(pricing, "Pricing updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete pricing
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePricing(string id)
    {
        try
        {
            var result = await _pricingService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Pricing not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Pricing deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get promotional pricing
    /// </summary>
    [HttpGet("promotional")]
    public async Task<IActionResult> GetPromotionalPricing([FromQuery] string supplierProductId)
    {
        try
        {
            var pricing = await _pricingService.GetPromotionalPricingAsync(supplierProductId);
            return Ok(ApiResponseDto<IEnumerable<SupplierPricingDto>>.SuccessResponse(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get expiring pricing
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringPricing([FromQuery] int daysAhead = 7)
    {
        try
        {
            var pricing = await _pricingService.GetExpiringPricingAsync(daysAhead);
            return Ok(ApiResponseDto<IEnumerable<SupplierPricingDto>>.SuccessResponse(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}