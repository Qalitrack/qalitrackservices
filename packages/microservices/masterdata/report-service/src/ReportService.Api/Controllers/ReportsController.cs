using Microsoft.AspNetCore.Mvc;
using ReportService.Core.DTOs;
using ReportService.Core.Interfaces;

namespace ReportService.Api.Controllers;

[Route("api/[controller]")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reports
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var reports = await _reportService.GetAllAsync();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all reports");
            return InternalServerError("An error occurred while retrieving reports");
        }
    }

    /// <summary>
    /// Get report by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var report = await _reportService.GetByIdAsync(id);
            if (report == null)
            {
                return NotFound("Report not found");
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report with id {Id}", id);
            return InternalServerError("An error occurred while retrieving report");
        }
    }

    /// <summary>
    /// Create a new report
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportDto request)
    {
        try
        {
            var report = await _reportService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            return InternalServerError("An error occurred while creating report");
        }
    }

    /// <summary>
    /// Update an existing report
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateReportDto request)
    {
        try
        {
            var report = await _reportService.UpdateAsync(id, request);
            if (report == null)
            {
                return NotFound("Report not found");
            }

            return Ok(report, "Report updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report with id {Id}", id);
            return InternalServerError("An error occurred while updating report");
        }
    }

    /// <summary>
    /// Delete a report
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _reportService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Report not found");
            }

            return Ok<object?>(null, "Report deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report with id {Id}", id);
            return InternalServerError("An error occurred while deleting report");
        }
    }

    /// <summary>
    /// Check if report name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _reportService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking report name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}