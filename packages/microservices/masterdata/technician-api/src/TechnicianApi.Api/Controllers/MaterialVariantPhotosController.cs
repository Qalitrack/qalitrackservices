using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class MaterialVariantPhotosController : ControllerBase
{
    private readonly IMaterialVariantPhotoService _service;
    private readonly IFileStorageService _fileStorage;

    public MaterialVariantPhotosController(IMaterialVariantPhotoService service, IFileStorageService fileStorage)
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

    [HttpGet("variant/{variantId}")]
    public async Task<IActionResult> GetByVariantId(string variantId)
    {
        var result = await _service.GetByVariantIdAsync(variantId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialVariantPhotoDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string variantId, [FromForm] IFormFile file, [FromForm] string? caption)
    {
        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "MaterialVariant", variantId, userId, caption ?? "Variant Photo");
        var photoUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        var dto = new CreateMaterialVariantPhotoDto
        {
            MaterialVariantId = variantId,
            PhotoUrl = photoUrl,
            Caption = caption
        };

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
