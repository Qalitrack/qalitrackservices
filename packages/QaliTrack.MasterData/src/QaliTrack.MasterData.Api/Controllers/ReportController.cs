using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Report.Entities;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Report Module")]
public class ReportController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public ReportController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get reports with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of reports</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Report>>>> GetReports(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Reports
                .Include(r => r.Templates)
                .Include(r => r.Schedules)
                .Include(r => r.Permissions)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<Report>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<Report>>.ErrorResponse("Error retrieving reports", ex.Message));
        }
    }

    /// <summary>
    /// Get report by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Report>>> GetReport(Guid id)
    {
        try
        {
            var report = await _context.Reports
                .Include(r => r.Templates)
                .Include(r => r.Schedules)
                .Include(r => r.Executions.OrderByDescending(e => e.StartTime).Take(10))
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound(ApiResponse<Report>.ErrorResponse("Report not found"));
            }

            return Ok(ApiResponse<Report>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Report>.ErrorResponse("Error retrieving report", ex.Message));
        }
    }

    /// <summary>
    /// Create new report
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Report>>> CreateReport(Report report)
    {
        try
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), 
                new { id = report.Id }, 
                ApiResponse<Report>.SuccessResponse(report, "Report created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Report>.ErrorResponse("Error creating report", ex.Message));
        }
    }

    /// <summary>
    /// Update report
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Report>>> UpdateReport(Guid id, Report report)
    {
        if (id != report.Id)
        {
            return BadRequest(ApiResponse<Report>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Report>.SuccessResponse(report, "Report updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ReportExists(id))
            {
                return NotFound(ApiResponse<Report>.ErrorResponse("Report not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Report>.ErrorResponse("Error updating report", ex.Message));
        }
    }

    /// <summary>
    /// Delete report (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReport(Guid id)
    {
        try
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(ApiResponse.CreateError("Report not found"));
            }

            report.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Report deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting report", ex.Message));
        }
    }

    /// <summary>
    /// Execute report
    /// </summary>
    [HttpPost("{id}/execute")]
    public async Task<ActionResult<ApiResponse<ReportExecution>>> ExecuteReport(Guid id, [FromBody] ReportExecutionRequest request)
    {
        try
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(ApiResponse<ReportExecution>.ErrorResponse("Report not found"));
            }

            var execution = new ReportExecution
            {
                Id = Guid.NewGuid(),
                ExecutionId = Guid.NewGuid().ToString(),
                ReportId = id,
                StartTime = DateTime.UtcNow,
                Status = "Running",
                Parameters = request.Parameters ?? string.Empty,
                ExecutedBy = request.ExecutedBy ?? "System",
                IsScheduled = false
            };

            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<ReportExecution>.SuccessResponse(execution, "Report execution started"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportExecution>.ErrorResponse("Error executing report", ex.Message));
        }
    }

    /// <summary>
    /// Get report executions
    /// </summary>
    [HttpGet("{id}/executions")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportExecution>>>> GetReportExecutions(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ReportExecutions
                .Where(e => e.ReportId == id)
                .Include(e => e.Report)
                .Include(e => e.Schedule)
                .OrderByDescending(e => e.StartTime)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ReportExecution>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ReportExecution>>.ErrorResponse("Error retrieving report executions", ex.Message));
        }
    }

    private async Task<bool> ReportExists(Guid id)
    {
        return await _context.Reports.AnyAsync(e => e.Id == id);
    }
}

public class ReportExecutionRequest
{
    public string? Parameters { get; set; }
    public string? ExecutedBy { get; set; }
}