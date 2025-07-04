using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;
using SupplierService.Core.Interfaces;

namespace SupplierService.Api.Controllers;

[Route("api/[controller]")]
public class SupplierContactsController : BaseController
{
    private readonly ISupplierService _supplierService;

    public SupplierContactsController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Update a contact
    /// </summary>
    [HttpPut("{contactId}")]
    public async Task<IActionResult> UpdateContact(string contactId, [FromBody] UpdateSupplierContactRequest request)
    {
        try
        {
            var contact = await _supplierService.UpdateContactAsync(contactId, request);
            return Ok(ApiResponseDto<SupplierContactDto>.SuccessResponse(contact, "Contact updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    [HttpDelete("{contactId}")]
    public async Task<IActionResult> DeleteContact(string contactId)
    {
        try
        {
            await _supplierService.DeleteContactAsync(contactId);
            return Ok(ApiResponseDto<object>.SuccessResponse(null!, "Contact deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}