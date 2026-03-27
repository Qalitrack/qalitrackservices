using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class MaterialPhotosController : ControllerBase
{
    private readonly IMaterialPhotoService _service;
    private readonly IFileStorageService _fileStorage;

    public MaterialPhotosController(IMaterialPhotoService service, IFileStorageService fileStorage)
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

    [HttpGet("material/{materialId}")]
    public async Task<IActionResult> GetByMaterialId(string materialId)
    {
        var result = await _service.GetByMaterialIdAsync(materialId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialPhotoDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string materialId, [FromForm] IFormFile file, [FromForm] string? caption)
    {
        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "Material", materialId, userId, caption ?? "Material Photo");
        var photoUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        var dto = new CreateMaterialPhotoDto
        {
            MaterialId = materialId,
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
