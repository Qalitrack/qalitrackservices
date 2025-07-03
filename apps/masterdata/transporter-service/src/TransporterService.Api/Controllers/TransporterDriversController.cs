using Microsoft.AspNetCore.Mvc;
using TransporterService.Core.DTOs;
using TransporterService.Core.Interfaces;

namespace TransporterService.Api.Controllers;

[Route("api/transporters/{transporterId}/drivers")]
public class TransporterDriversController : BaseController
{
    private readonly ITransporterService _transporterService;

    public TransporterDriversController(ITransporterService transporterService)
    {
        _transporterService = transporterService;
    }

    /// <summary>
    /// Get all drivers for a transporter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDrivers(string transporterId)
    {
        try
        {
            var drivers = await _transporterService.GetDriversByTransporterIdAsync(transporterId);
            return HandleListResult(drivers, "Drivers retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get active drivers for a transporter
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDrivers(string transporterId)
    {
        try
        {
            var drivers = await _transporterService.GetActiveDriversAsync(transporterId);
            return HandleListResult(drivers, "Active drivers retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Assign a driver to a transporter
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AssignDriver(string transporterId, [FromBody] AssignDriverRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return HandleError("Validation failed", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }

            request.TransporterId = transporterId;
            var driver = await _transporterService.AssignDriverAsync(request);
            return HandleCreated(driver, "Driver assigned successfully");
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
    /// Unassign a driver from a transporter
    /// </summary>
    [HttpDelete("{driverId}")]
    public async Task<IActionResult> UnassignDriver(string transporterId, string driverId)
    {
        try
        {
            var result = await _transporterService.UnassignDriverAsync(driverId);
            if (!result)
            {
                return NotFound();
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(null, "Driver unassigned successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}