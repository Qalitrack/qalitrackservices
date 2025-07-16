using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[Route("api/[controller]")]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;
    private readonly IContactService _contactService;
    private readonly IContractService _contractService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        IContactService contactService,
        IContractService contractService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _contactService = contactService;
        _contractService = contractService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return InternalServerError("An error occurred while retrieving customers");
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer with id {Id}", id);
            return InternalServerError("An error occurred while retrieving customer");
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto request)
    {
        try
        {
            var customer = await _customerService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return InternalServerError("An error occurred while creating customer");
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCustomerDto request)
    {
        try
        {
            var customer = await _customerService.UpdateAsync(id, request);
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(customer, "Customer updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with id {Id}", id);
            return InternalServerError("An error occurred while updating customer");
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _customerService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Customer not found");
            }

            return Ok<object?>(null, "Customer deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with id {Id}", id);
            return InternalServerError("An error occurred while deleting customer");
        }
    }

    /// <summary>
    /// Check if customer name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _customerService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking customer name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }

    /// <summary>
    /// Activate a customer
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id)
    {
        try
        {
            var result = await _customerService.ActivateCustomerAsync(id);
            if (!result)
            {
                return NotFound("Customer not found");
            }

            return Ok<object?>(null, "Customer activated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating customer with id {Id}", id);
            return InternalServerError("An error occurred while activating customer");
        }
    }

    /// <summary>
    /// Deactivate a customer
    /// </summary>
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        try
        {
            var result = await _customerService.DeactivateCustomerAsync(id);
            if (!result)
            {
                return NotFound("Customer not found");
            }

            return Ok<object?>(null, "Customer deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating customer with id {Id}", id);
            return InternalServerError("An error occurred while deactivating customer");
        }
    }

    /// <summary>
    /// Enable transporter role for customer (dual-role support)
    /// </summary>
    [HttpPost("{id}/enable-transporter")]
    public async Task<IActionResult> EnableTransporterRole(string id, [FromBody] EnableTransporterRoleDto request)
    {
        try
        {
            var result = await _customerService.EnableTransporterRoleAsync(id, request.TransporterId);
            if (!result)
            {
                return NotFound("Customer not found");
            }

            return Ok<object?>(null, "Transporter role enabled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling transporter role for customer with id {Id}", id);
            return InternalServerError("An error occurred while enabling transporter role");
        }
    }

    /// <summary>
    /// Set preferred transporter for customer
    /// </summary>
    [HttpPost("{id}/set-preferred-transporter")]
    public async Task<IActionResult> SetPreferredTransporter(string id, [FromBody] SetPreferredTransporterDto request)
    {
        try
        {
            var result = await _customerService.SetPreferredTransporterAsync(id, request.TransporterId);
            if (!result)
            {
                return NotFound("Customer not found");
            }

            return Ok<object?>(null, "Preferred transporter set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting preferred transporter for customer with id {Id}", id);
            return InternalServerError("An error occurred while setting preferred transporter");
        }
    }

    // Contact Management Endpoints

    /// <summary>
    /// Get customer contacts
    /// </summary>
    [HttpGet("{id}/contacts")]
    public async Task<IActionResult> GetContacts(string id)
    {
        try
        {
            var contacts = await _contactService.GetCustomerContactsAsync(id);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts for customer with id {Id}", id);
            return InternalServerError("An error occurred while retrieving contacts");
        }
    }

    /// <summary>
    /// Add contact to customer
    /// </summary>
    [HttpPost("{id}/contacts")]
    public async Task<IActionResult> AddContact(string id, [FromBody] CreateContactDto request)
    {
        try
        {
            request.CustomerId = id; // Ensure customer ID matches route
            var contact = await _contactService.CreateContactAsync(request);
            return CreatedAtAction(nameof(GetContacts), new { id }, contact);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contact to customer with id {Id}", id);
            return InternalServerError("An error occurred while adding contact");
        }
    }

    /// <summary>
    /// Get primary contact for customer
    /// </summary>
    [HttpGet("{id}/contacts/primary")]
    public async Task<IActionResult> GetPrimaryContact(string id)
    {
        try
        {
            var contact = await _contactService.GetPrimaryContactAsync(id);
            if (contact == null)
            {
                return NotFound("Primary contact not found");
            }

            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting primary contact for customer with id {Id}", id);
            return InternalServerError("An error occurred while retrieving primary contact");
        }
    }

    // Contract Management Endpoints

    /// <summary>
    /// Get customer contracts
    /// </summary>
    [HttpGet("{id}/contracts")]
    public async Task<IActionResult> GetContracts(string id)
    {
        try
        {
            var contracts = await _contractService.GetCustomerContractsAsync(id);
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contracts for customer with id {Id}", id);
            return InternalServerError("An error occurred while retrieving contracts");
        }
    }

    /// <summary>
    /// Create contract for customer
    /// </summary>
    [HttpPost("{id}/contracts")]
    public async Task<IActionResult> CreateContract(string id, [FromBody] CreateContractDto request)
    {
        try
        {
            request.CustomerId = id; // Ensure customer ID matches route
            var contract = await _contractService.CreateContractAsync(request);
            return CreatedAtAction(nameof(GetContracts), new { id }, contract);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract for customer with id {Id}", id);
            return InternalServerError("An error occurred while creating contract");
        }
    }

    /// <summary>
    /// Get active contracts for customer
    /// </summary>
    [HttpGet("{id}/contracts/active")]
    public async Task<IActionResult> GetActiveContracts(string id)
    {
        try
        {
            var allContracts = await _contractService.GetCustomerContractsAsync(id);
            var activeContracts = allContracts.Where(c => c.Status == "Active");
            return Ok(activeContracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active contracts for customer with id {Id}", id);
            return InternalServerError("An error occurred while retrieving active contracts");
        }
    }

    /// <summary>
    /// Get customer communications
    /// </summary>
    [HttpGet("{id}/communications")]
    public async Task<IActionResult> GetCommunications(string id)
    {
        try
        {
            var communications = await _contactService.GetCustomerCommunicationsAsync(id);
            return Ok(communications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communications for customer with id {Id}", id);
            return InternalServerError("An error occurred while retrieving communications");
        }
    }
}