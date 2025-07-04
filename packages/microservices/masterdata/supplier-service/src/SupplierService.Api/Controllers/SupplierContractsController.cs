using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class SupplierContractsController : BaseController
{
    private readonly ISupplierService _supplierService;

    public SupplierContractsController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Update a contract
    /// </summary>
    [HttpPut("{contractId}")]
    public async Task<IActionResult> UpdateContract(string contractId, [FromBody] UpdateSupplierContractRequest request)
    {
        try
        {
            var contract = await _supplierService.UpdateContractAsync(contractId, request);
            return Ok(ApiResponseDto<SupplierContractDto>.SuccessResponse(contract, "Contract updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a contract
    /// </summary>
    [HttpDelete("{contractId}")]
    public async Task<IActionResult> DeleteContract(string contractId)
    {
        try
        {
            await _supplierService.DeleteContractAsync(contractId);
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Contract deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get expiring contracts
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringContracts([FromQuery] DateTime? date = null)
    {
        try
        {
            var expiryDate = date ?? DateTime.UtcNow.AddMonths(3); // Default to 3 months from now
            var contracts = await _supplierService.GetExpiringContractsAsync(expiryDate);
            return Ok(ApiResponseDto<IEnumerable<SupplierContractDto>>.SuccessResponse(contracts));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}