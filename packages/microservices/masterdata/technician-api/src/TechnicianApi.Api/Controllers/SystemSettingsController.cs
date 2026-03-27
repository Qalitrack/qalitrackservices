using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.SystemSettings;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _service;

    public SystemSettingsController(ISystemSettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _service.GetSettingsAsync();
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSystemSettingsDto dto)
    {
        var result = await _service.UpdateSettingsAsync(dto);
        return Ok(result);
    }
}
