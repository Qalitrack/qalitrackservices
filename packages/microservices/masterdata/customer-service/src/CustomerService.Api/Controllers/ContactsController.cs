using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[Route("api/[controller]")]
public class ContactsController : BaseController
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(IContactService contactService, ILogger<ContactsController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    /// <summary>
    /// Get contact by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContact(string id)
    {
        try
        {
            var contact = await _contactService.GetContactAsync(id);
            if (contact == null)
            {
                return NotFound("Contact not found");
            }

            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact with id {Id}", id);
            return InternalServerError("An error occurred while retrieving contact");
        }
    }

    /// <summary>
    /// Update an existing contact
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(string id, [FromBody] UpdateContactDto request)
    {
        try
        {
            var contact = await _contactService.UpdateContactAsync(id, request);
            if (contact == null)
            {
                return NotFound("Contact not found");
            }

            return Ok(contact, "Contact updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact with id {Id}", id);
            return InternalServerError("An error occurred while updating contact");
        }
    }

    /// <summary>
    /// Delete a contact (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(string id)
    {
        try
        {
            var result = await _contactService.DeleteContactAsync(id);
            if (!result)
            {
                return NotFound("Contact not found");
            }

            return Ok<object?>(null, "Contact deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact with id {Id}", id);
            return InternalServerError("An error occurred while deleting contact");
        }
    }

    /// <summary>
    /// Get contacts by type for a customer
    /// </summary>
    [HttpGet("customer/{customerId}/type/{contactType}")]
    public async Task<IActionResult> GetContactsByType(string customerId, ContactType contactType)
    {
        try
        {
            var contacts = await _contactService.GetContactsByTypeAsync(customerId, contactType);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts by type for customer {CustomerId}", customerId);
            return InternalServerError("An error occurred while retrieving contacts");
        }
    }

    /// <summary>
    /// Get contacts by role for a customer
    /// </summary>
    [HttpGet("customer/{customerId}/role/{role}")]
    public async Task<IActionResult> GetContactsByRole(string customerId, ContactRole role)
    {
        try
        {
            var contacts = await _contactService.GetContactsByRoleAsync(customerId, role);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts by role for customer {CustomerId}", customerId);
            return InternalServerError("An error occurred while retrieving contacts");
        }
    }

    /// <summary>
    /// Update contact preferences
    /// </summary>
    [HttpPut("{id}/preferences")]
    public async Task<IActionResult> UpdateContactPreferences(string id, [FromBody] UpdateContactPreferencesDto request)
    {
        try
        {
            var result = await _contactService.UpdateContactPreferencesAsync(id, request);
            if (!result)
            {
                return NotFound("Contact not found");
            }

            return Ok<object?>(null, "Contact preferences updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences for contact {Id}", id);
            return InternalServerError("An error occurred while updating contact preferences");
        }
    }

    // Communication logging endpoints

    /// <summary>
    /// Log communication with a contact
    /// </summary>
    [HttpPost("{id}/communications")]
    public async Task<IActionResult> LogCommunication(string id, [FromBody] LogCommunicationDto request)
    {
        try
        {
            request.ContactId = id; // Ensure contact ID matches route
            var communication = await _contactService.LogCommunicationAsync(request);
            return CreatedAtAction(nameof(GetContactCommunications), new { id }, communication);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging communication for contact {Id}", id);
            return InternalServerError("An error occurred while logging communication");
        }
    }

    /// <summary>
    /// Get communications for a contact
    /// </summary>
    [HttpGet("{id}/communications")]
    public async Task<IActionResult> GetContactCommunications(string id)
    {
        try
        {
            var communications = await _contactService.GetContactCommunicationsAsync(id);
            return Ok(communications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communications for contact {Id}", id);
            return InternalServerError("An error occurred while retrieving communications");
        }
    }

    /// <summary>
    /// Mark communication as resolved
    /// </summary>
    [HttpPut("communications/{communicationId}/resolve")]
    public async Task<IActionResult> MarkCommunicationResolved(string communicationId, [FromBody] ResolveCommunicationDto request)
    {
        try
        {
            var result = await _contactService.MarkCommunicationResolvedAsync(communicationId, request.Resolution);
            if (!result)
            {
                return NotFound("Communication not found");
            }

            return Ok<object?>(null, "Communication marked as resolved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving communication {CommunicationId}", communicationId);
            return InternalServerError("An error occurred while resolving communication");
        }
    }

    /// <summary>
    /// Get pending communications
    /// </summary>
    [HttpGet("communications/pending")]
    public async Task<IActionResult> GetPendingCommunications([FromQuery] string? customerId = null)
    {
        try
        {
            var communications = await _contactService.GetPendingCommunicationsAsync(customerId);
            return Ok(communications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending communications");
            return InternalServerError("An error occurred while retrieving pending communications");
        }
    }

    /// <summary>
    /// Get contacts for notification by type
    /// </summary>
    [HttpGet("customer/{customerId}/notifications/{notificationType}")]
    public async Task<IActionResult> GetContactsForNotification(string customerId, string notificationType)
    {
        try
        {
            var contacts = await _contactService.GetContactsForNotificationAsync(customerId, notificationType);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts for notification for customer {CustomerId}", customerId);
            return InternalServerError("An error occurred while retrieving contacts for notification");
        }
    }
}