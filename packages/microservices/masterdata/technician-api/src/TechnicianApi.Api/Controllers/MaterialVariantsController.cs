using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class MaterialVariantsController : ControllerBase
{
    private readonly IMaterialVariantService _service;

    public MaterialVariantsController(IMaterialVariantService service)
    {
        _service = service;
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
        [FromQuery] string? materialId = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, materialId);
        return Ok(result);
    }

    [HttpGet("material/{materialId}")]
    public async Task<IActionResult> GetByMaterialId(string materialId)
    {
        var result = await _service.GetByMaterialIdAsync(materialId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialVariantDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateMaterialVariantDto dto)
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
}
