using Microsoft.AspNetCore.Mvc;
using VehicleService.Core.DTOs;
using VehicleService.Core.Interfaces;

namespace VehicleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleInsuranceController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleInsuranceController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleInsuranceDto>> GetVehicleInsurance(string id)
    {
        var insurance = await _vehicleService.GetVehicleInsuranceByIdAsync(id);
        if (insurance == null)
        {
            return NotFound();
        }
        return Ok(insurance);
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<List<VehicleInsuranceDto>>> GetExpiringInsurance([FromQuery] DateTime? beforeDate)
    {
        var date = beforeDate ?? DateTime.UtcNow.AddDays(30); // Default to 30 days from now
        var insurance = await _vehicleService.GetExpiringInsuranceAsync(date);
        return Ok(insurance);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleInsuranceDto>> UpdateVehicleInsurance(string id, UpdateVehicleInsuranceRequest request)
    {
        try
        {
            var insurance = await _vehicleService.UpdateVehicleInsuranceAsync(id, request);
            return Ok(insurance);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleInsurance(string id)
    {
        try
        {
            await _vehicleService.DeleteVehicleInsuranceAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}