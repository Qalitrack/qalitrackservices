using Microsoft.AspNetCore.Mvc;
using VehicleService.Core.DTOs;
using VehicleService.Core.Interfaces;

namespace VehicleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleTypesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleTypesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleTypeDto>>> GetVehicleTypes()
    {
        var vehicleTypes = await _vehicleService.GetAllVehicleTypesAsync();
        return Ok(vehicleTypes);
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<VehicleTypeDto>>> GetActiveVehicleTypes()
    {
        var vehicleTypes = await _vehicleService.GetActiveVehicleTypesAsync();
        return Ok(vehicleTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleTypeDto>> GetVehicleType(string id)
    {
        var vehicleType = await _vehicleService.GetVehicleTypeByIdAsync(id);
        if (vehicleType == null)
        {
            return NotFound();
        }
        return Ok(vehicleType);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleTypeDto>> CreateVehicleType(CreateVehicleTypeRequest request)
    {
        try
        {
            var vehicleType = await _vehicleService.CreateVehicleTypeAsync(request);
            return CreatedAtAction(nameof(GetVehicleType), new { id = vehicleType.Id }, vehicleType);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleTypeDto>> UpdateVehicleType(string id, UpdateVehicleTypeRequest request)
    {
        try
        {
            var vehicleType = await _vehicleService.UpdateVehicleTypeAsync(id, request);
            return Ok(vehicleType);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleType(string id)
    {
        try
        {
            await _vehicleService.DeleteVehicleTypeAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}