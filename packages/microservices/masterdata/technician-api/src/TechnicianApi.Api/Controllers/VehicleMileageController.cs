using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class VehicleMileageController : ControllerBase
{
    private readonly IVehicleMileageService _service;
    private readonly IFileStorageService _fileStorage;

    public VehicleMileageController(IVehicleMileageService service, IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? truckId = null,
        [FromQuery] string? driverId = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, truckId, driverId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleMileageDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateVehicleMileageDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/upload-proof-image")]
    public async Task<IActionResult> UploadProofImage(string id, [FromForm] IFormFile file)
    {
        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "VehicleMileage", id, userId, "Proof Image");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        var result = await _service.UpdateProofImageAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, mileage = result });
    }

    [HttpPost("{id}/upload-proof-end-image")]
    public async Task<IActionResult> UploadProofEndImage(string id, [FromForm] IFormFile file)
    {
        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "VehicleMileage", id, userId, "Proof End Image");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        var result = await _service.UpdateProofEndImageAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, mileage = result });
    }
}
