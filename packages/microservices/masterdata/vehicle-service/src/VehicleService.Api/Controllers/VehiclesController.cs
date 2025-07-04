using Microsoft.AspNetCore.Mvc;
using VehicleService.Core.DTOs;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;

namespace VehicleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleDto>>> GetVehicles()
    {
        var vehicles = await _vehicleService.GetAllVehiclesAsync();
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetVehicle(string id)
    {
        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound();
        }
        return Ok(vehicle);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<VehicleDto>> GetVehicleWithDetails(string id)
    {
        var vehicle = await _vehicleService.GetVehicleWithDetailsAsync(id);
        if (vehicle == null)
        {
            return NotFound();
        }
        return Ok(vehicle);
    }

    [HttpGet("by-registration/{registrationNumber}")]
    public async Task<ActionResult<VehicleDto>> GetVehicleByRegistrationNumber(string registrationNumber)
    {
        var vehicle = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
        if (vehicle == null)
        {
            return NotFound();
        }
        return Ok(vehicle);
    }

    [HttpGet("by-vin/{vin}")]
    public async Task<ActionResult<VehicleDto>> GetVehicleByVIN(string vin)
    {
        var vehicle = await _vehicleService.GetVehicleByVINAsync(vin);
        if (vehicle == null)
        {
            return NotFound();
        }
        return Ok(vehicle);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<VehicleDto>>> GetVehiclesByStatus(VehicleStatus status)
    {
        var vehicles = await _vehicleService.GetVehiclesByStatusAsync(status);
        return Ok(vehicles);
    }

    [HttpGet("by-owner/{ownerName}")]
    public async Task<ActionResult<List<VehicleDto>>> GetVehiclesByOwner(string ownerName)
    {
        var vehicles = await _vehicleService.GetVehiclesByOwnerAsync(ownerName);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> RegisterVehicle(RegisterVehicleRequest request)
    {
        try
        {
            var vehicle = await _vehicleService.RegisterVehicleAsync(request);
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleDto>> UpdateVehicle(string id, UpdateVehicleRequest request)
    {
        try
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, request);
            return Ok(vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(string id)
    {
        try
        {
            await _vehicleService.DeleteVehicleAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/documents")]
    public async Task<ActionResult<List<VehicleDocumentDto>>> GetVehicleDocuments(string id)
    {
        var documents = await _vehicleService.GetVehicleDocumentsAsync(id);
        return Ok(documents);
    }

    [HttpPost("{id}/documents")]
    public async Task<ActionResult<VehicleDocumentDto>> UploadVehicleDocument(string id, [FromForm] UploadVehicleDocumentRequest request, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        request.VehicleId = id;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        try
        {
            var document = await _vehicleService.UploadVehicleDocumentAsync(request, fileData, file.FileName, file.ContentType);
            return CreatedAtAction("GetVehicleDocument", "VehicleDocuments", new { id = document.Id }, document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/inspections")]
    public async Task<ActionResult<List<VehicleInspectionDto>>> GetVehicleInspections(string id)
    {
        var inspections = await _vehicleService.GetVehicleInspectionsAsync(id);
        return Ok(inspections);
    }

    [HttpPost("{id}/inspections")]
    public async Task<ActionResult<VehicleInspectionDto>> RecordVehicleInspection(string id, RecordVehicleInspectionRequest request)
    {
        request.VehicleId = id;
        try
        {
            var inspection = await _vehicleService.RecordVehicleInspectionAsync(request);
            return CreatedAtAction("GetVehicleInspection", "VehicleInspections", new { id = inspection.Id }, inspection);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/insurance")]
    public async Task<ActionResult<List<VehicleInsuranceDto>>> GetVehicleInsurance(string id)
    {
        var insurance = await _vehicleService.GetVehicleInsuranceAsync(id);
        return Ok(insurance);
    }

    [HttpGet("{id}/insurance/active")]
    public async Task<ActionResult<VehicleInsuranceDto>> GetActiveVehicleInsurance(string id)
    {
        var insurance = await _vehicleService.GetActiveVehicleInsuranceAsync(id);
        if (insurance == null)
        {
            return NotFound("No active insurance policy found for this vehicle");
        }
        return Ok(insurance);
    }

    [HttpPost("{id}/insurance")]
    public async Task<ActionResult<VehicleInsuranceDto>> CreateVehicleInsurance(string id, CreateVehicleInsuranceRequest request)
    {
        request.VehicleId = id;
        try
        {
            var insurance = await _vehicleService.CreateVehicleInsuranceAsync(request);
            return CreatedAtAction("GetVehicleInsurance", "VehicleInsurance", new { id = insurance.Id }, insurance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}