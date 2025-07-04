using Microsoft.AspNetCore.Mvc;
using OrganizationService.Core.DTOs;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;

namespace OrganizationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrganizationDto>>> GetOrganizations()
    {
        var organizations = await _organizationService.GetAllOrganizationsAsync();
        return Ok(organizations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganization(string id)
    {
        var organization = await _organizationService.GetOrganizationByIdAsync(id);
        if (organization == null)
        {
            return NotFound();
        }
        return Ok(organization);
    }

    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganizationByCode(string code)
    {
        var organization = await _organizationService.GetOrganizationByCodeAsync(code);
        if (organization == null)
        {
            return NotFound();
        }
        return Ok(organization);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> CreateOrganization(CreateOrganizationRequest request)
    {
        try
        {
            var organization = await _organizationService.CreateOrganizationAsync(request);
            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrganizationDto>> UpdateOrganization(string id, UpdateOrganizationRequest request)
    {
        try
        {
            var organization = await _organizationService.UpdateOrganizationAsync(id, request);
            return Ok(organization);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(string id)
    {
        try
        {
            await _organizationService.DeleteOrganizationAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/hierarchy")]
    public async Task<ActionResult<List<OrganizationDto>>> GetOrganizationHierarchy(string id)
    {
        var hierarchy = await _organizationService.GetOrganizationHierarchyAsync(id);
        return Ok(hierarchy);
    }

    [HttpGet("{parentId}/children")]
    public async Task<ActionResult<List<OrganizationDto>>> GetChildOrganizations(string parentId)
    {
        var children = await _organizationService.GetChildOrganizationsAsync(parentId);
        return Ok(children);
    }

    [HttpGet("{id}/users")]
    public async Task<ActionResult<List<OrganizationUserDto>>> GetOrganizationUsers(string id)
    {
        var users = await _organizationService.GetOrganizationUsersAsync(id);
        return Ok(users);
    }

    [HttpPost("{id}/users")]
    public async Task<ActionResult<OrganizationUserDto>> CreateOrganizationUser(string id, CreateOrganizationUserRequest request)
    {
        try
        {
            var user = await _organizationService.CreateOrganizationUserAsync(id, request);
            return CreatedAtAction(nameof(GetOrganizationUser), new { organizationId = id, userId = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{organizationId}/users/{userId}")]
    public async Task<ActionResult<OrganizationUserDto>> GetOrganizationUser(string organizationId, string userId)
    {
        var user = await _organizationService.GetOrganizationUserByIdAsync(organizationId, userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPut("{organizationId}/users/{userId}")]
    public async Task<ActionResult<OrganizationUserDto>> UpdateOrganizationUser(string organizationId, string userId, UpdateOrganizationUserRequest request)
    {
        try
        {
            var user = await _organizationService.UpdateOrganizationUserAsync(organizationId, userId, request);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{organizationId}/users/{userId}")]
    public async Task<IActionResult> DeleteOrganizationUser(string organizationId, string userId)
    {
        try
        {
            await _organizationService.DeleteOrganizationUserAsync(organizationId, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/users/invite")]
    public async Task<ActionResult<OrganizationUserDto>> InviteUser(string id, [FromBody] InviteUserRequest request)
    {
        try
        {
            var user = await _organizationService.InviteUserAsync(id, request.Email, request.Role);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/settings")]
    public async Task<ActionResult<OrganizationSettingsDto>> GetOrganizationSettings(string id)
    {
        var settings = await _organizationService.GetOrganizationSettingsAsync(id);
        if (settings == null)
        {
            return NotFound();
        }
        return Ok(settings);
    }

    [HttpPut("{id}/settings")]
    public async Task<ActionResult<OrganizationSettingsDto>> UpdateOrganizationSettings(string id, UpdateOrganizationSettingsRequest request)
    {
        try
        {
            var settings = await _organizationService.UpdateOrganizationSettingsAsync(id, request);
            return Ok(settings);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public class InviteUserRequest
{
    public string Email { get; set; } = string.Empty;
    public OrganizationRole Role { get; set; }
}