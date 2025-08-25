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
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var pricings = await _pricingService.GetAllAsync();
            return Ok(pricings);
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
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var pricing = await _pricingService.GetByIdAsync(id);
            if (pricing == null)
            {
                return NotFound("Pricing not found");
            }

            return Ok(pricing);
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
    public async Task<IActionResult> GetByProductId(string productId)
    {
        try
        {
            var pricings = await _pricingService.GetByProductIdAsync(productId);
            return Ok(pricings);
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
    public async Task<IActionResult> GetCustomerPricing(string productId, string customerId)
    {
        try
        {
            var pricing = await _pricingService.GetEffectivePricingAsync(productId, customerId);
            if (pricing == null)
            {
                return NotFound("Customer-specific pricing not found");
            }

            return Ok(pricing);
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
    public async Task<IActionResult> GetEffectivePrice(
        string productId, 
        [FromQuery] string? customerId = null, 
        [FromQuery] int quantity = 1)
    {
        try
        {
            var effectivePrice = await _pricingService.CalculateEffectivePriceAsync(productId, quantity, customerId);
            return Ok(effectivePrice);
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
    public async Task<IActionResult> Create([FromBody] CreatePricingDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state", GetModelStateErrors());
            }

            var pricing = await _pricingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = pricing.Id }, pricing);
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
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePricingDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state", GetModelStateErrors());
            }

            var pricing = await _pricingService.UpdateAsync(id, dto);
            if (pricing == null)
            {
                return NotFound("Pricing not found");
            }

            return Ok(pricing);
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
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _pricingService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Pricing not found");
            }

            return Ok(true);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}