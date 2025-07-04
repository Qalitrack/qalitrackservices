using Microsoft.AspNetCore.Mvc;
using TransporterService.Core.DTOs;
using TransporterService.Core.Interfaces;

namespace TransporterService.Api.Controllers;

[Route("api/transporters/{transporterId}/contacts")]
public class TransporterContactsController : BaseController
{
    private readonly ITransporterService _transporterService;

    public TransporterContactsController(ITransporterService transporterService)
    {
        _transporterService = transporterService;
    }

    /// <summary>
    /// Get all contacts for a transporter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContacts(string transporterId)
    {
        try
        {
            var contacts = await _transporterService.GetContactsByTransporterIdAsync(transporterId);
            return HandleListResult(contacts, "Contacts retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get primary contact for a transporter
    /// </summary>
    [HttpGet("primary")]
    public async Task<IActionResult> GetPrimaryContact(string transporterId)
    {
        try
        {
            var contact = await _transporterService.GetPrimaryContactAsync(transporterId);
            if (contact == null)
            {
                return NotFound(ApiResponseDto<object>.ErrorResponse("No primary contact found"));
            }
            return HandleResult(contact, "Primary contact retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Add a new contact for a transporter
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddContact(string transporterId, [FromBody] CreateTransporterContactRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return HandleError("Validation failed", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }

            request.TransporterId = transporterId;
            var contact = await _transporterService.AddContactAsync(request);
            return HandleCreated(contact, "Contact added successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message);
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
    public async Task<IActionResult> DeleteContact(string transporterId, string contactId)
    {
        try
        {
            var result = await _transporterService.DeleteContactAsync(contactId);
            if (!result)
            {
                return NotFound();
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(null, "Contact deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}