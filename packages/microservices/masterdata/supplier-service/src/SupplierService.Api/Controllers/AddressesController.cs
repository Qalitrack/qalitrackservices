using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class AddressesController : BaseController
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddressById(string id)
    {
        try
        {
            var address = await _addressService.GetByIdAsync(id);
            return HandleResult(address);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update address
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(string id, [FromBody] UpdateAddressDto request)
    {
        try
        {
            var address = await _addressService.UpdateAsync(id, request);
            if (address == null)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Address not found"));
            
            return Ok(ApiResponseDto<AddressDto>.SuccessResponse(address, "Address updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete address
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(string id)
    {
        try
        {
            var result = await _addressService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Address not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Address deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Set address as default
    /// </summary>
    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(string id, [FromQuery] string supplierId)
    {
        try
        {
            var result = await _addressService.SetDefaultAddressAsync(id, supplierId);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Address not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Address set as default successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}