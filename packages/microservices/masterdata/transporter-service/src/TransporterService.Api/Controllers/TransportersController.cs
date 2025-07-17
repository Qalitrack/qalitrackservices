using Microsoft.AspNetCore.Mvc;
using TransporterService.Core.DTOs;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;

namespace TransporterService.Api.Controllers;

public class TransportersController : BaseController
{
    private readonly ITransporterService _transporterService;

    public TransportersController(ITransporterService transporterService)
    {
        _transporterService = transporterService;
    }

    /// <summary>
    /// Get all transporters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransporters([FromQuery] string? status = null, [FromQuery] string? type = null)
    {
        try
        {
            IEnumerable<TransporterDto> transporters;

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TransporterStatus>(status, true, out var statusEnum))
            {
                transporters = await _transporterService.GetTransportersByStatusAsync(statusEnum);
            }
            else if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransporterType>(type, true, out var typeEnum))
            {
                transporters = await _transporterService.GetTransportersByTypeAsync(typeEnum);
            }
            else
            {
                transporters = await _transporterService.GetAllTransportersAsync();
            }

            return HandleListResult(transporters, "Transporters retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get active transporters
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveTransporters()
    {
        try
        {
            var transporters = await _transporterService.GetActiveTransportersAsync();
            return HandleListResult(transporters, "Active transporters retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get available transporters for a specific date and route
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableTransporters([FromQuery] DateTime date, [FromQuery] string? routeId = null)
    {
        try
        {
            var transporters = await _transporterService.GetAvailableTransportersAsync(date, routeId);
            return HandleListResult(transporters, "Available transporters retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Search transporters
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchTransporters([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return HandleError("Search term is required");
            }

            var transporters = await _transporterService.SearchTransportersAsync(searchTerm);
            return HandleListResult(transporters, "Search completed successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transporter by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransporter(string id)
    {
        try
        {
            var transporter = await _transporterService.GetTransporterByIdAsync(id);
            return HandleResult(transporter, "Transporter retrieved successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Register a new transporter
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RegisterTransporter([FromBody] RegisterTransporterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return HandleError("Validation failed", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }

            var transporter = await _transporterService.RegisterTransporterAsync(request);
            return HandleCreated(transporter, "Transporter registered successfully");
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update transporter
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransporter(string id, [FromBody] UpdateTransporterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return HandleError("Validation failed", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }

            var transporter = await _transporterService.UpdateTransporterAsync(id, request);
            return HandleResult(transporter, "Transporter updated successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete transporter
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransporter(string id)
    {
        try
        {
            var result = await _transporterService.DeleteTransporterAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(null, "Transporter deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transporter performance summary
    /// </summary>
    [HttpGet("{id}/performance")]
    public async Task<IActionResult> GetPerformanceSummary(string id)
    {
        try
        {
            var summary = await _transporterService.GetPerformanceSummaryAsync(id);
            return HandleResult(summary, "Performance summary retrieved successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Enable dual-role (customer) for transporter
    /// </summary>
    [HttpPost("{id}/enable-customer-role")]
    public async Task<IActionResult> EnableCustomerRole(string id, [FromBody] EnableCustomerRoleDto enableCustomerRoleDto)
    {
        try
        {
            var result = await _transporterService.EnableCustomerRoleAsync(id, enableCustomerRoleDto);
            return HandleResult(result, "Customer role enabled successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Disable dual-role (customer) for transporter
    /// </summary>
    [HttpPost("{id}/disable-customer-role")]
    public async Task<IActionResult> DisableCustomerRole(string id)
    {
        try
        {
            var result = await _transporterService.DisableCustomerRoleAsync(id);
            return HandleResult(result, "Customer role disabled successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transporters with dual-role (customer) capability
    /// </summary>
    [HttpGet("dual-role")]
    public async Task<IActionResult> GetDualRoleTransporters()
    {
        try
        {
            var transporters = await _transporterService.GetDualRoleTransportersAsync();
            return HandleListResult(transporters, "Dual-role transporters retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}