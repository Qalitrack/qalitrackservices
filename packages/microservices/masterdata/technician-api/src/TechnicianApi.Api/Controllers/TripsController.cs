using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _service;
    private readonly IFileStorageService _fileStorage;

    public TripsController(ITripService service, IFileStorageService fileStorage)
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
        [FromQuery] string? driverId = null,
        [FromQuery] string? truckId = null,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, driverId, truckId, status);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTripDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTripDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartTrip(string id, [FromBody] StartTripDto dto)
    {
        var result = await _service.StartTripAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/end")]
    public async Task<IActionResult> EndTrip(string id, [FromBody] EndTripDto dto)
    {
        var result = await _service.EndTripAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/recalculate-cost")]
    public async Task<IActionResult> RecalculateCost(string id)
    {
        var result = await _service.RecalculateTotalCostAsync(id);
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
        var attachment = await _fileStorage.SaveFileAsync(file, "Trip", id, userId, "Proof Image");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to trip entity
        var result = await _service.UpdateProofImageAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, trip = result });
    }

    [HttpPost("{id}/upload-end-proof-image")]
    public async Task<IActionResult> UploadEndProofImage(string id, [FromForm] IFormFile file)
    {
        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "Trip", id, userId, "End Proof Image");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to trip entity
        var result = await _service.UpdateEndProofImageAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, trip = result });
    }

    [HttpPost("{id}/upload-material-loading-photo")]
    public async Task<IActionResult> UploadMaterialLoadingPhoto(string id, [FromForm] IFormFile file)
    {
        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "Trip", id, userId, "Material Loading Photo");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Add to trip's material loading photos array
        var result = await _service.AddMaterialLoadingPhotoAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, trip = result });
    }

    [HttpGet("{id}/attachments")]
    public async Task<IActionResult> GetTripAttachments(string id)
    {
        var attachments = await _fileStorage.GetAttachmentsForEntityAsync("Trip", id);
        return Ok(attachments);
    }
}
