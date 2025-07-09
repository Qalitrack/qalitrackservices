using Microsoft.AspNetCore.Mvc;
using TestServiceV4.Core.DTOs;
using TestServiceV4.Core.Interfaces;

namespace TestServiceV4.Api.Controllers;

[Route("api/[controller]")]
public class TestentitysController : BaseController
{
    private readonly ITestentityService _testentityService;
    private readonly ILogger<TestentitysController> _logger;

    public TestentitysController(ITestentityService testentityService, ILogger<TestentitysController> logger)
    {
        _testentityService = testentityService;
        _logger = logger;
    }

    /// <summary>
    /// Get all testentitys
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var testentitys = await _testentityService.GetAllAsync();
            return Ok(testentitys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all testentitys");
            return InternalServerError("An error occurred while retrieving testentitys");
        }
    }

    /// <summary>
    /// Get testentity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var testentity = await _testentityService.GetByIdAsync(id);
            if (testentity == null)
            {
                return NotFound("Testentity not found");
            }

            return Ok(testentity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting testentity with id {Id}", id);
            return InternalServerError("An error occurred while retrieving testentity");
        }
    }

    /// <summary>
    /// Create a new testentity
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestentityDto request)
    {
        try
        {
            var testentity = await _testentityService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = testentity.Id }, testentity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating testentity");
            return InternalServerError("An error occurred while creating testentity");
        }
    }

    /// <summary>
    /// Update an existing testentity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTestentityDto request)
    {
        try
        {
            var testentity = await _testentityService.UpdateAsync(id, request);
            if (testentity == null)
            {
                return NotFound("Testentity not found");
            }

            return Ok(testentity, "Testentity updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating testentity with id {Id}", id);
            return InternalServerError("An error occurred while updating testentity");
        }
    }

    /// <summary>
    /// Delete a testentity
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _testentityService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Testentity not found");
            }

            return Ok<object?>(null, "Testentity deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting testentity with id {Id}", id);
            return InternalServerError("An error occurred while deleting testentity");
        }
    }

    /// <summary>
    /// Check if testentity name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _testentityService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking testentity name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}