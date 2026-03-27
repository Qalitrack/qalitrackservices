using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class DriverProfilesController : ControllerBase
{
    private readonly IDriverProfileService _service;
    private readonly IDriverActivityService _activityService;
    private readonly IFileStorageService _fileStorage;

    public DriverProfilesController(
        IDriverProfileService service,
        IDriverActivityService activityService,
        IFileStorageService fileStorage)
    {
        _service = service;
        _activityService = activityService;
        _fileStorage = fileStorage;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("current/{driverId}")]
    public async Task<IActionResult> GetCurrentByDriverId(string driverId)
    {
        var result = await _service.GetCurrentProfileByDriverIdAsync(driverId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? driverId = null,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, driverId, status);
        return Ok(result);
    }

    [HttpGet("history/{driverId}")]
    public async Task<IActionResult> GetHistory(string driverId)
    {
        var result = await _service.GetProfileHistoryAsync(driverId);
        return Ok(result);
    }

    [HttpGet("expiring-licenses")]
    public async Task<IActionResult> GetExpiringLicenses([FromQuery] int days = 30)
    {
        var result = await _service.GetExpiringLicensesAsync(days);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverProfileDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateDriverProfileDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(string id)
    {
        var result = await _service.SubmitForApprovalAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, [FromBody] ApproveDriverProfileDto dto)
    {
        // TODO: Get reviewedBy from authenticated user context
        var reviewedBy = "admin"; // Placeholder
        var result = await _service.ApproveProfileAsync(id, dto, reviewedBy);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/upload-profile-photo")]
    public async Task<IActionResult> UploadProfilePhoto(string id, [FromForm] IFormFile file)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();

        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "DriverProfile", id, userId, "Profile Photo");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to profile entity
        var result = await _service.UpdateProfilePhotoAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, profile = result });
    }

    [HttpPost("{id}/upload-license-front")]
    public async Task<IActionResult> UploadLicenseFront(string id, [FromForm] IFormFile file)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();

        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "DriverProfile", id, userId, "License Front");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to profile entity
        var result = await _service.UpdateLicenseFrontAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, profile = result });
    }

    [HttpPost("{id}/upload-license-back")]
    public async Task<IActionResult> UploadLicenseBack(string id, [FromForm] IFormFile file)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();

        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "DriverProfile", id, userId, "License Back");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to profile entity
        var result = await _service.UpdateLicenseBackAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, profile = result });
    }

    [HttpPost("{id}/upload-id-front")]
    public async Task<IActionResult> UploadIdFront(string id, [FromForm] IFormFile file)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();

        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "DriverProfile", id, userId, "ID Front");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to profile entity
        var result = await _service.UpdateIdFrontAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, profile = result });
    }

    [HttpPost("{id}/upload-id-back")]
    public async Task<IActionResult> UploadIdBack(string id, [FromForm] IFormFile file)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();

        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "DriverProfile", id, userId, "ID Back");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Save URL to profile entity
        var result = await _service.UpdateIdBackAsync(id, imageUrl);
        if (result == null) return NotFound();

        return Ok(new { url = imageUrl, profile = result });
    }

    [HttpGet("{id}/documents")]
    public async Task<IActionResult> GetProfileDocuments(string id)
    {
        var attachments = await _fileStorage.GetAttachmentsForEntityAsync("DriverProfile", id);
        return Ok(attachments);
    }
}
