using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : BaseController
{
    private readonly ICustomerService _customerService;

    public ContactsController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(string id, [FromBody] UpdateCustomerContactRequest request)
    {
        try
        {
            var contact = await _customerService.UpdateContactAsync(id, request);
            return HandleResult(Success(contact, "Contact updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<CustomerContactDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerContactDto>(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(string id)
    {
        try
        {
            var result = await _customerService.DeleteContactAsync(id);
            if (!result)
            {
                return NotFound(Error($"Contact with ID {id} not found"));
            }

            return HandleResult(Success("Contact deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }
}