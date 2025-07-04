using Microsoft.AspNetCore.Mvc;
using TransporterService.Core.DTOs;
using TransporterService.Core.Interfaces;

namespace TransporterService.Api.Controllers;

[Route("api/transporters/{transporterId}/fleet")]
public class TransporterFleetController : BaseController
{
    private readonly ITransporterService _transporterService;

    public TransporterFleetController(ITransporterService transporterService)
    {
        _transporterService = transporterService;
    }

    /// <summary>
    /// Get all fleet vehicles for a transporter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFleet(string transporterId)
    {
        try
        {
            var fleet = await _transporterService.GetFleetByTransporterIdAsync(transporterId);
            return HandleListResult(fleet, "Fleet vehicles retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get available vehicles for a transporter
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableVehicles(string transporterId)
    {
        try
        {
            var vehicles = await _transporterService.GetAvailableVehiclesAsync(transporterId);
            return HandleListResult(vehicles, "Available vehicles retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Add a new vehicle to fleet
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddFleetVehicle(string transporterId, [FromBody] AddFleetVehicleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return HandleError("Validation failed", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }

            request.TransporterId = transporterId;
            var vehicle = await _transporterService.AddFleetVehicleAsync(request);
            return HandleCreated(vehicle, "Fleet vehicle added successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message);
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
    /// Delete a fleet vehicle
    /// </summary>
    [HttpDelete("{vehicleId}")]
    public async Task<IActionResult> DeleteFleetVehicle(string transporterId, string vehicleId)
    {
        try
        {
            var result = await _transporterService.DeleteFleetVehicleAsync(vehicleId);
            if (!result)
            {
                return NotFound();
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(null, "Fleet vehicle deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}