using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[Route("api/[controller]")]
public class ContractsController : BaseController
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    /// <summary>
    /// Get contract by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContract(string id)
    {
        try
        {
            var contract = await _contractService.GetContractAsync(id);
            if (contract == null)
            {
                return NotFound("Contract not found");
            }

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract with id {Id}", id);
            return InternalServerError("An error occurred while retrieving contract");
        }
    }

    /// <summary>
    /// Update an existing contract
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContract(string id, [FromBody] UpdateContractDto request)
    {
        try
        {
            var contract = await _contractService.UpdateContractAsync(id, request);
            if (contract == null)
            {
                return NotFound("Contract not found");
            }

            return Ok(contract, "Contract updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract with id {Id}", id);
            return InternalServerError("An error occurred while updating contract");
        }
    }

    /// <summary>
    /// Delete a contract
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(string id)
    {
        try
        {
            var result = await _contractService.DeleteContractAsync(id);
            if (!result)
            {
                return NotFound("Contract not found");
            }

            return Ok<object?>(null, "Contract deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract with id {Id}", id);
            return InternalServerError("An error occurred while deleting contract");
        }
    }

    /// <summary>
    /// Activate a contract
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateContract(string id)
    {
        try
        {
            var result = await _contractService.ActivateContractAsync(id);
            if (!result)
            {
                return NotFound("Contract not found");
            }

            return Ok<object?>(null, "Contract activated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating contract with id {Id}", id);
            return InternalServerError("An error occurred while activating contract");
        }
    }

    /// <summary>
    /// Suspend a contract
    /// </summary>
    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendContract(string id, [FromBody] SuspendContractDto request)
    {
        try
        {
            var result = await _contractService.SuspendContractAsync(id, request.Reason);
            if (!result)
            {
                return NotFound("Contract not found");
            }

            return Ok<object?>(null, "Contract suspended successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending contract with id {Id}", id);
            return InternalServerError("An error occurred while suspending contract");
        }
    }

    /// <summary>
    /// Terminate a contract
    /// </summary>
    [HttpPost("{id}/terminate")]
    public async Task<IActionResult> TerminateContract(string id, [FromBody] TerminateContractDto request)
    {
        try
        {
            var result = await _contractService.TerminateContractAsync(id, request.Reason);
            if (!result)
            {
                return NotFound("Contract not found");
            }

            return Ok<object?>(null, "Contract terminated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating contract with id {Id}", id);
            return InternalServerError("An error occurred while terminating contract");
        }
    }

    /// <summary>
    /// Renew a contract
    /// </summary>
    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewContract(string id, [FromBody] RenewContractDto request)
    {
        try
        {
            var renewal = await _contractService.RenewContractAsync(id, request);
            return CreatedAtAction(nameof(GetContractRenewals), new { id }, renewal);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing contract with id {Id}", id);
            return InternalServerError("An error occurred while renewing contract");
        }
    }

    /// <summary>
    /// Get contract renewals
    /// </summary>
    [HttpGet("{id}/renewals")]
    public async Task<IActionResult> GetContractRenewals(string id)
    {
        try
        {
            var renewals = await _contractService.GetContractRenewalsAsync(id);
            return Ok(renewals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting renewals for contract {Id}", id);
            return InternalServerError("An error occurred while retrieving contract renewals");
        }
    }

    /// <summary>
    /// Get expiring contracts
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringContracts([FromQuery] int daysAhead = 30)
    {
        try
        {
            var contracts = await _contractService.GetExpiringContractsAsync(daysAhead);
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring contracts");
            return InternalServerError("An error occurred while retrieving expiring contracts");
        }
    }

    /// <summary>
    /// Get contracts for renewal notification
    /// </summary>
    [HttpGet("renewal-notifications")]
    public async Task<IActionResult> GetContractsForRenewalNotification()
    {
        try
        {
            var contracts = await _contractService.GetContractsForRenewalNotificationAsync();
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contracts for renewal notification");
            return InternalServerError("An error occurred while retrieving contracts for renewal notification");
        }
    }

    /// <summary>
    /// Get active contracts
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveContracts()
    {
        try
        {
            var contracts = await _contractService.GetActiveContractsAsync();
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active contracts");
            return InternalServerError("An error occurred while retrieving active contracts");
        }
    }

    /// <summary>
    /// Search contracts
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchContracts([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            var contracts = await _contractService.SearchContractsAsync(searchTerm);
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching contracts with term {SearchTerm}", searchTerm);
            return InternalServerError("An error occurred while searching contracts");
        }
    }
}