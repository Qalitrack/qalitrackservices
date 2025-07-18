using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricingController : BaseController
{
    private readonly IPricingService _pricingService;

    public PricingController(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    /// <summary>
    /// Get all pricing records
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PricingDto>>>> GetAll()
    {
        try
        {
            var pricings = await _pricingService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<PricingDto>>.Success(pricings));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get pricing by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<PricingDto>>> GetById(string id)
    {
        try
        {
            var pricing = await _pricingService.GetByIdAsync(id);
            if (pricing == null)
            {
                return NotFound(ApiResponseDto<PricingDto>.Error("Pricing not found"));
            }

            return Ok(ApiResponseDto<PricingDto>.Success(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get pricing for a specific product
    /// </summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PricingDto>>>> GetByProductId(string productId)
    {
        try
        {
            var pricings = await _pricingService.GetByProductIdAsync(productId);
            return Ok(ApiResponseDto<IEnumerable<PricingDto>>.Success(pricings));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get customer-specific pricing for a product
    /// </summary>
    [HttpGet("product/{productId}/customer/{customerId}")]
    public async Task<ActionResult<ApiResponseDto<PricingDto>>> GetCustomerPricing(string productId, string customerId)
    {
        try
        {
            var pricing = await _pricingService.GetCustomerPricingAsync(productId, customerId);
            if (pricing == null)
            {
                return NotFound(ApiResponseDto<PricingDto>.Error("Customer-specific pricing not found"));
            }

            return Ok(ApiResponseDto<PricingDto>.Success(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Calculate effective price for a product
    /// </summary>
    [HttpGet("product/{productId}/effective-price")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetEffectivePrice(
        string productId, 
        [FromQuery] string? customerId = null, 
        [FromQuery] int quantity = 1)
    {
        try
        {
            var effectivePrice = await _pricingService.CalculateEffectivePriceAsync(productId, customerId, quantity);
            return Ok(ApiResponseDto<decimal>.Success(effectivePrice));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new pricing record
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<PricingDto>>> Create([FromBody] PricingDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDto<PricingDto>.Error("Invalid model state", GetModelStateErrors()));
            }

            var pricing = await _pricingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = pricing.Id }, 
                ApiResponseDto<PricingDto>.Success(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update an existing pricing record
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<PricingDto>>> Update(string id, [FromBody] PricingDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDto<PricingDto>.Error("Invalid model state", GetModelStateErrors()));
            }

            var pricing = await _pricingService.UpdateAsync(id, dto);
            if (pricing == null)
            {
                return NotFound(ApiResponseDto<PricingDto>.Error("Pricing not found"));
            }

            return Ok(ApiResponseDto<PricingDto>.Success(pricing));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a pricing record
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(string id)
    {
        try
        {
            var result = await _pricingService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponseDto<bool>.Error("Pricing not found"));
            }

            return Ok(ApiResponseDto<bool>.Success(true));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}