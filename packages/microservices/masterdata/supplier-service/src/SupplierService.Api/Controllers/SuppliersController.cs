using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class SuppliersController : BaseController
{
    private readonly ISupplierService _supplierService;
    private readonly IAddressService _addressService;
    private readonly ISupplierProductService _supplierProductService;
    private readonly ISupplierPerformanceService _performanceService;

    public SuppliersController(
        ISupplierService supplierService,
        IAddressService addressService,
        ISupplierProductService supplierProductService,
        ISupplierPerformanceService performanceService)
    {
        _supplierService = supplierService;
        _addressService = addressService;
        _supplierProductService = supplierProductService;
        _performanceService = performanceService;
    }

    /// <summary>
    /// Get all suppliers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<SupplierReadDto>>.SuccessResponse(suppliers));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(string id)
    {
        try
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            return HandleResult(supplier);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetSupplierByCode(string code)
    {
        try
        {
            var supplier = await _supplierService.GetByCodeAsync(code);
            return HandleResult(supplier);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Register a new supplier
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto request)
    {
        try
        {
            var supplier = await _supplierService.CreateAsync(request);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, 
                ApiResponseDto<SupplierReadDto>.SuccessResponse(supplier, "Supplier created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update supplier information
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(string id, [FromBody] UpdateSupplierDto request)
    {
        try
        {
            var supplier = await _supplierService.UpdateAsync(id, request);
            if (supplier == null)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Supplier not found"));
            
            return Ok(ApiResponseDto<SupplierReadDto>.SuccessResponse(supplier, "Supplier updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a supplier
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(string id)
    {
        try
        {
            var result = await _supplierService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Supplier not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Supplier deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Search suppliers
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchSuppliers([FromQuery] string term)
    {
        try
        {
            var suppliers = await _supplierService.SearchSuppliersAsync(term);
            return Ok(ApiResponseDto<IEnumerable<SupplierReadDto>>.SuccessResponse(suppliers));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get suppliers by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetSuppliersByStatus(SupplierStatus status)
    {
        try
        {
            var suppliers = await _supplierService.GetByStatusAsync(status);
            return Ok(ApiResponseDto<IEnumerable<SupplierReadDto>>.SuccessResponse(suppliers));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get suppliers by type
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetSuppliersByType(SupplierType type)
    {
        try
        {
            var suppliers = await _supplierService.GetByTypeAsync(type);
            return Ok(ApiResponseDto<IEnumerable<SupplierReadDto>>.SuccessResponse(suppliers));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Verify a supplier
    /// </summary>
    [HttpPost("{id}/verify")]
    public async Task<IActionResult> VerifySupplier(string id)
    {
        try
        {
            var result = await _supplierService.VerifySupplierAsync(id);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResponse("Supplier not found"));
            
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Supplier verified successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier addresses
    /// </summary>
    [HttpGet("{id}/addresses")]
    public async Task<IActionResult> GetSupplierAddresses(string id)
    {
        try
        {
            var addresses = await _addressService.GetAllAsync(id);
            return Ok(ApiResponseDto<IEnumerable<AddressDto>>.SuccessResponse(addresses));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create supplier address
    /// </summary>
    [HttpPost("{id}/addresses")]
    public async Task<IActionResult> CreateSupplierAddress(string id, [FromBody] CreateAddressDto request)
    {
        try
        {
            var address = await _addressService.CreateAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierAddresses), new { id }, 
                ApiResponseDto<AddressDto>.SuccessResponse(address, "Address created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update supplier address
    /// </summary>
    [HttpPut("{supplierId}/addresses/{addressId}")]
    public async Task<IActionResult> UpdateSupplierAddress(string supplierId, string addressId, [FromBody] UpdateAddressDto request)
    {
        try
        {
            var address = await _addressService.UpdateAsync(addressId, request);
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
    /// Delete supplier address
    /// </summary>
    [HttpDelete("{supplierId}/addresses/{addressId}")]
    public async Task<IActionResult> DeleteSupplierAddress(string supplierId, string addressId)
    {
        try
        {
            var result = await _addressService.DeleteAsync(addressId);
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
    /// Get supplier products
    /// </summary>
    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetSupplierProducts(string id)
    {
        try
        {
            var products = await _supplierProductService.GetAllAsync(id);
            return Ok(ApiResponseDto<IEnumerable<SupplierProductDto>>.SuccessResponse(products));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create supplier product
    /// </summary>
    [HttpPost("{id}/products")]
    public async Task<IActionResult> CreateSupplierProduct(string id, [FromBody] CreateSupplierProductDto request)
    {
        try
        {
            var product = await _supplierProductService.CreateAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierProducts), new { id }, 
                ApiResponseDto<SupplierProductDto>.SuccessResponse(product, "Product created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update supplier product
    /// </summary>
    [HttpPut("{supplierId}/products/{productId}")]
    public async Task<IActionResult> UpdateSupplierProduct(string supplierId, string productId, [FromBody] UpdateSupplierProductDto request)
    {
        try
        {
            var product = await _supplierProductService.UpdateAsync(productId, request);
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
    [HttpDelete("{supplierId}/products/{productId}")]
    public async Task<IActionResult> DeleteSupplierProduct(string supplierId, string productId)
    {
        try
        {
            var result = await _supplierProductService.DeleteAsync(productId);
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
    /// Get supplier performance
    /// </summary>
    [HttpGet("{id}/performance")]
    public async Task<IActionResult> GetSupplierPerformance(string id)
    {
        try
        {
            var performance = await _performanceService.GetAllAsync(id);
            return Ok(ApiResponseDto<IEnumerable<SupplierPerformanceDto>>.SuccessResponse(performance));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create supplier performance record
    /// </summary>
    [HttpPost("{id}/performance")]
    public async Task<IActionResult> CreateSupplierPerformance(string id, [FromBody] CreateSupplierPerformanceDto request)
    {
        try
        {
            var performance = await _performanceService.CreateAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierPerformance), new { id }, 
                ApiResponseDto<SupplierPerformanceDto>.SuccessResponse(performance, "Performance record created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier performance summary
    /// </summary>
    [HttpGet("{id}/performance/summary")]
    public async Task<IActionResult> GetSupplierPerformanceSummary(string id)
    {
        try
        {
            var summary = await _performanceService.GetPerformanceSummaryAsync(id);
            return Ok(ApiResponseDto<SupplierPerformanceSummaryDto>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}