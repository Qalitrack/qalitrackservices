using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class SuppliersController : BaseController
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Get all suppliers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return Ok(ApiResponseDto<IEnumerable<SupplierDto>>.SuccessResponse(suppliers));
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
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
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
    public async Task<IActionResult> RegisterSupplier([FromBody] RegisterSupplierRequest request)
    {
        try
        {
            var supplier = await _supplierService.RegisterSupplierAsync(request);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, 
                ApiResponseDto<SupplierDto>.SuccessResponse(supplier, "Supplier registered successfully"));
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
    public async Task<IActionResult> UpdateSupplier(string id, [FromBody] UpdateSupplierRequest request)
    {
        try
        {
            var supplier = await _supplierService.UpdateSupplierAsync(id, request);
            return Ok(ApiResponseDto<SupplierDto>.SuccessResponse(supplier, "Supplier updated successfully"));
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
            await _supplierService.DeleteSupplierAsync(id);
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
            return Ok(ApiResponseDto<IEnumerable<SupplierDto>>.SuccessResponse(suppliers));
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
            var suppliers = await _supplierService.GetSuppliersByStatusAsync(status);
            return Ok(ApiResponseDto<IEnumerable<SupplierDto>>.SuccessResponse(suppliers));
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
            var suppliers = await _supplierService.GetSuppliersByTypeAsync(type);
            return Ok(ApiResponseDto<IEnumerable<SupplierDto>>.SuccessResponse(suppliers));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier contacts
    /// </summary>
    [HttpGet("{id}/contacts")]
    public async Task<IActionResult> GetSupplierContacts(string id)
    {
        try
        {
            var contacts = await _supplierService.GetSupplierContactsAsync(id);
            return Ok(ApiResponseDto<IEnumerable<SupplierContactDto>>.SuccessResponse(contacts));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create supplier contact
    /// </summary>
    [HttpPost("{id}/contacts")]
    public async Task<IActionResult> CreateSupplierContact(string id, [FromBody] CreateSupplierContactRequest request)
    {
        try
        {
            var contact = await _supplierService.CreateContactAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierContacts), new { id }, 
                ApiResponseDto<SupplierContactDto>.SuccessResponse(contact, "Contact created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier contracts
    /// </summary>
    [HttpGet("{id}/contracts")]
    public async Task<IActionResult> GetSupplierContracts(string id)
    {
        try
        {
            var contracts = await _supplierService.GetSupplierContractsAsync(id);
            return Ok(ApiResponseDto<IEnumerable<SupplierContractDto>>.SuccessResponse(contracts));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create supplier contract
    /// </summary>
    [HttpPost("{id}/contracts")]
    public async Task<IActionResult> CreateSupplierContract(string id, [FromBody] CreateSupplierContractRequest request)
    {
        try
        {
            var contract = await _supplierService.CreateContractAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierContracts), new { id }, 
                ApiResponseDto<SupplierContractDto>.SuccessResponse(contract, "Contract created successfully"));
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
            var products = await _supplierService.GetSupplierProductsAsync(id);
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
    public async Task<IActionResult> CreateSupplierProduct(string id, [FromBody] CreateSupplierProductRequest request)
    {
        try
        {
            var product = await _supplierService.CreateProductAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierProducts), new { id }, 
                ApiResponseDto<SupplierProductDto>.SuccessResponse(product, "Product created successfully"));
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
            var performance = await _supplierService.GetSupplierPerformanceAsync(id);
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
    public async Task<IActionResult> CreateSupplierPerformance(string id, [FromBody] CreateSupplierPerformanceRequest request)
    {
        try
        {
            var performance = await _supplierService.CreatePerformanceAsync(id, request);
            return CreatedAtAction(nameof(GetSupplierPerformance), new { id }, 
                ApiResponseDto<SupplierPerformanceDto>.SuccessResponse(performance, "Performance record created successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get supplier financial information
    /// </summary>
    [HttpGet("{id}/financial")]
    public async Task<IActionResult> GetSupplierFinancial(string id)
    {
        try
        {
            var financial = await _supplierService.GetSupplierFinancialAsync(id);
            return HandleResult(financial);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update supplier financial information
    /// </summary>
    [HttpPut("{id}/financial")]
    public async Task<IActionResult> UpdateSupplierFinancial(string id, [FromBody] UpdateSupplierFinancialRequest request)
    {
        try
        {
            var financial = await _supplierService.UpdateFinancialAsync(id, request);
            return Ok(ApiResponseDto<SupplierFinancialDto>.SuccessResponse(financial, "Financial information updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}