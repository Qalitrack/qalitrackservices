using Microsoft.AspNetCore.Mvc;
using VehicleService.Core.DTOs;
using VehicleService.Core.Interfaces;

namespace VehicleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleInspectionsController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleInspectionsController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleInspectionDto>> GetVehicleInspection(string id)
    {
        var inspection = await _vehicleService.GetVehicleInspectionByIdAsync(id);
        if (inspection == null)
        {
            return NotFound();
        }
        return Ok(inspection);
    }

    [HttpGet("due")]
    public async Task<ActionResult<List<VehicleInspectionDto>>> GetDueInspections([FromQuery] DateTime? beforeDate)
    {
        var date = beforeDate ?? DateTime.UtcNow.AddDays(30); // Default to 30 days from now
        var inspections = await _vehicleService.GetDueInspectionsAsync(date);
        return Ok(inspections);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleInspectionDto>> UpdateVehicleInspection(string id, UpdateVehicleInspectionRequest request)
    {
        try
        {
            var inspection = await _vehicleService.UpdateVehicleInspectionAsync(id, request);
            return Ok(inspection);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleInspection(string id)
    {
        try
        {
            await _vehicleService.DeleteVehicleInspectionAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}