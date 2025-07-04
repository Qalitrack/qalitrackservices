using Microsoft.AspNetCore.Mvc;
using VehicleService.Core.DTOs;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;

namespace VehicleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleDocumentsController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleDocumentsController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDocumentDto>> GetVehicleDocument(string id)
    {
        var document = await _vehicleService.GetVehicleDocumentByIdAsync(id);
        if (document == null)
        {
            return NotFound();
        }
        return Ok(document);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadVehicleDocument(string id)
    {
        var document = await _vehicleService.GetVehicleDocumentByIdAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var fileData = await _vehicleService.GetDocumentFileAsync(id);
        if (fileData == null)
        {
            return NotFound("File not found");
        }

        return File(fileData, document.FileType, document.FileName);
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<List<VehicleDocumentDto>>> GetExpiringDocuments([FromQuery] DateTime? beforeDate)
    {
        var date = beforeDate ?? DateTime.UtcNow.AddDays(30); // Default to 30 days from now
        var documents = await _vehicleService.GetExpiringDocumentsAsync(date);
        return Ok(documents);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleDocumentDto>> UpdateVehicleDocument(string id, UpdateVehicleDocumentRequest request)
    {
        try
        {
            var document = await _vehicleService.UpdateVehicleDocumentAsync(id, request);
            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleDocument(string id)
    {
        try
        {
            await _vehicleService.DeleteVehicleDocumentAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}