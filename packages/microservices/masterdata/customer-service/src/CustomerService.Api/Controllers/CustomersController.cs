using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        try
        {
            IEnumerable<CustomerDto> customers;

            if (!string.IsNullOrEmpty(search))
            {
                customers = await _customerService.SearchCustomersAsync(search);
            }
            else
            {
                customers = await _customerService.GetCustomersPagedAsync(page, pageSize);
            }

            return HandleResult(Success(customers, "Customers retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<CustomerDto>>(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.RegisterCustomerAsync(request);
            return HandleResult(Success(customer, "Customer registered successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerDto>(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(string id)
    {
        try
        {
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound(Error<CustomerDto>($"Customer with ID {id} not found"));
            }

            return HandleResult(Success(customer, "Customer retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerDto>(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(string id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.UpdateCustomerAsync(id, request);
            return HandleResult(Success(customer, "Customer updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<CustomerDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerDto>(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        try
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result)
            {
                return NotFound(Error($"Customer with ID {id} not found"));
            }

            return HandleResult(Success("Customer deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateCustomer(string id)
    {
        try
        {
            var result = await _customerService.ActivateCustomerAsync(id);
            if (!result)
            {
                return NotFound(Error($"Customer with ID {id} not found"));
            }

            return HandleResult(Success("Customer activated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateCustomer(string id)
    {
        try
        {
            var result = await _customerService.DeactivateCustomerAsync(id);
            if (!result)
            {
                return NotFound(Error($"Customer with ID {id} not found"));
            }

            return HandleResult(Success("Customer deactivated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetCustomerDetails(string id)
    {
        try
        {
            var customer = await _customerService.GetCustomerDetailsAsync(id);
            if (customer == null)
            {
                return NotFound(Error<CustomerDetailDto>($"Customer with ID {id} not found"));
            }

            return HandleResult(Success(customer, "Customer details retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerDetailDto>(ex.Message));
        }
    }

    [HttpGet("{id}/contacts")]
    public async Task<IActionResult> GetCustomerContacts(string id)
    {
        try
        {
            var contacts = await _customerService.GetCustomerContactsAsync(id);
            return HandleResult(Success(contacts, "Customer contacts retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<CustomerContactDto>>(ex.Message));
        }
    }

    [HttpPost("{id}/contacts")]
    public async Task<IActionResult> AddContact(string id, [FromBody] CreateCustomerContactRequest request)
    {
        try
        {
            var contact = await _customerService.AddContactAsync(id, request);
            return HandleResult(Success(contact, "Contact added successfully"));
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

    [HttpGet("{id}/contracts")]
    public async Task<IActionResult> GetCustomerContracts(string id)
    {
        try
        {
            var contracts = await _customerService.GetCustomerContractsAsync(id);
            return HandleResult(Success(contracts, "Customer contracts retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<IEnumerable<CustomerContractDto>>(ex.Message));
        }
    }

    [HttpPost("{id}/contracts")]
    public async Task<IActionResult> CreateContract(string id, [FromBody] CreateCustomerContractRequest request)
    {
        try
        {
            var contract = await _customerService.CreateContractAsync(id, request);
            return HandleResult(Success(contract, "Contract created successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<CustomerContractDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerContractDto>(ex.Message));
        }
    }

    [HttpGet("{id}/billing")]
    public async Task<IActionResult> GetCustomerBilling(string id)
    {
        try
        {
            var billing = await _customerService.GetCustomerBillingAsync(id);
            if (billing == null)
            {
                return NotFound(Error<CustomerBillingDto>($"Billing information for customer {id} not found"));
            }

            return HandleResult(Success(billing, "Customer billing retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerBillingDto>(ex.Message));
        }
    }

    [HttpPut("{id}/billing")]
    public async Task<IActionResult> UpdateBilling(string id, [FromBody] UpdateCustomerBillingRequest request)
    {
        try
        {
            var billing = await _customerService.UpdateBillingAsync(id, request);
            return HandleResult(Success(billing, "Customer billing updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<CustomerBillingDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerBillingDto>(ex.Message));
        }
    }

    [HttpGet("{id}/credit")]
    public async Task<IActionResult> GetCustomerCredit(string id)
    {
        try
        {
            var credit = await _customerService.GetCustomerCreditAsync(id);
            if (credit == null)
            {
                return NotFound(Error<CustomerCreditDto>($"Credit information for customer {id} not found"));
            }

            return HandleResult(Success(credit, "Customer credit retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerCreditDto>(ex.Message));
        }
    }

    [HttpPut("{id}/credit")]
    public async Task<IActionResult> UpdateCredit(string id, [FromBody] UpdateCustomerCreditRequest request)
    {
        try
        {
            var credit = await _customerService.UpdateCreditAsync(id, request);
            return HandleResult(Success(credit, "Customer credit updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<CustomerCreditDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerCreditDto>(ex.Message));
        }
    }
}