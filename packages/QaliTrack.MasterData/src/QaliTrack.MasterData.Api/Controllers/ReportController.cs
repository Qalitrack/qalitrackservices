using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Report.DTOs;
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
    public async Task<ActionResult<ApiResponse<PagedResult<ReportSummaryDto>>>> GetReports(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Reports
                .Select(r => new ReportSummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    ReportType = r.ReportType,
                    Category = r.Category,
                    Status = r.Status,
                    OutputFormat = r.OutputFormat,
                    IsPublic = r.IsPublic,
                    RequiresApproval = r.RequiresApproval,
                    TemplateCount = r.Templates.Count,
                    ScheduleCount = r.Schedules.Count,
                    ExecutionCount = r.Executions.Count,
                    LastExecutionTime = r.Executions.OrderByDescending(e => e.StartTime).FirstOrDefault() != null ? r.Executions.OrderByDescending(e => e.StartTime).First().StartTime : null,
                    LastExecutionStatus = r.Executions.OrderByDescending(e => e.StartTime).FirstOrDefault() != null ? r.Executions.OrderByDescending(e => e.StartTime).First().Status : "",
                    CreatedAt = r.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ReportSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ReportSummaryDto>>.ErrorResponse("Error retrieving reports", ex.Message));
        }
    }

    /// <summary>
    /// Get report by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReportDetailDto>>> GetReport(Guid id)
    {
        try
        {
            var report = await _context.Reports
                .Where(r => r.Id == id)
                .Select(r => new ReportDetailDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    ReportType = r.ReportType,
                    Category = r.Category,
                    Status = r.Status,
                    DataSource = r.DataSource,
                    Parameters = r.Parameters,
                    OutputFormat = r.OutputFormat,
                    IsPublic = r.IsPublic,
                    RequiresApproval = r.RequiresApproval,
                    ExecutionTimeoutMinutes = r.ExecutionTimeoutMinutes,
                    Tags = r.Tags,
                    OrganizationId = r.OrganizationId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    HasTemplates = r.Templates.Any(),
                    HasSchedules = r.Schedules.Any(),
                    HasExecutions = r.Executions.Any(),
                    HasPermissions = r.Permissions.Any()
                })
                .FirstOrDefaultAsync();

            if (report == null)
            {
                return NotFound(ApiResponse<ReportDetailDto>.ErrorResponse("Report not found"));
            }

            return Ok(ApiResponse<ReportDetailDto>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportDetailDto>.ErrorResponse("Error retrieving report", ex.Message));
        }
    }

    /// <summary>
    /// Create new report
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReportDetailDto>>> CreateReport(CreateReportDto dto)
    {
        try
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description ?? string.Empty,
                ReportType = dto.ReportType,
                Category = dto.Category,
                Status = dto.Status,
                DataSource = dto.DataSource,
                Parameters = dto.Parameters,
                OutputFormat = dto.OutputFormat,
                IsPublic = dto.IsPublic,
                RequiresApproval = dto.RequiresApproval,
                ExecutionTimeoutMinutes = dto.ExecutionTimeoutMinutes,
                Tags = dto.Tags,
                OrganizationId = dto.OrganizationId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var responseDto = new ReportDetailDto
            {
                Id = report.Id,
                Name = report.Name,
                Code = report.Code,
                Description = report.Description,
                ReportType = report.ReportType,
                Category = report.Category,
                Status = report.Status,
                DataSource = report.DataSource,
                Parameters = report.Parameters,
                OutputFormat = report.OutputFormat,
                IsPublic = report.IsPublic,
                RequiresApproval = report.RequiresApproval,
                ExecutionTimeoutMinutes = report.ExecutionTimeoutMinutes,
                Tags = report.Tags,
                OrganizationId = report.OrganizationId,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                HasTemplates = false,
                HasSchedules = false,
                HasExecutions = false,
                HasPermissions = false
            };

            return CreatedAtAction(nameof(GetReport), 
                new { id = report.Id }, 
                ApiResponse<ReportDetailDto>.SuccessResponse(responseDto, "Report created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportDetailDto>.ErrorResponse("Error creating report", ex.Message));
        }
    }

    /// <summary>
    /// Update report
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ReportDetailDto>>> UpdateReport(Guid id, UpdateReportDto dto)
    {
        try
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(ApiResponse<ReportDetailDto>.ErrorResponse("Report not found"));
            }

            report.Name = dto.Name;
            report.ReportType = dto.ReportType;
            report.Category = dto.Category;
            report.Status = dto.Status;
            report.DataSource = dto.DataSource;
            report.Description = dto.Description ?? string.Empty;
            report.Parameters = dto.Parameters;
            report.OutputFormat = dto.OutputFormat;
            report.IsPublic = dto.IsPublic;
            report.RequiresApproval = dto.RequiresApproval;
            report.ExecutionTimeoutMinutes = dto.ExecutionTimeoutMinutes;
            report.Tags = dto.Tags;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = await _context.Reports
                .Where(r => r.Id == id)
                .Select(r => new ReportDetailDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    ReportType = r.ReportType,
                    Category = r.Category,
                    Status = r.Status,
                    DataSource = r.DataSource,
                    Parameters = r.Parameters,
                    OutputFormat = r.OutputFormat,
                    IsPublic = r.IsPublic,
                    RequiresApproval = r.RequiresApproval,
                    ExecutionTimeoutMinutes = r.ExecutionTimeoutMinutes,
                    Tags = r.Tags,
                    OrganizationId = r.OrganizationId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    HasTemplates = r.Templates.Any(),
                    HasSchedules = r.Schedules.Any(),
                    HasExecutions = r.Executions.Any(),
                    HasPermissions = r.Permissions.Any()
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<ReportDetailDto>.SuccessResponse(responseDto!, "Report updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportDetailDto>.ErrorResponse("Error updating report", ex.Message));
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
    public async Task<ActionResult<ApiResponse<ReportExecutionDto>>> ExecuteReport(Guid id, [FromBody] ReportExecutionRequestDto request)
    {
        try
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(ApiResponse<ReportExecutionDto>.ErrorResponse("Report not found"));
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

            var responseDto = new ReportExecutionDto
            {
                Id = execution.Id,
                ReportId = execution.ReportId,
                ExecutionId = execution.ExecutionId,
                StartTime = execution.StartTime,
                EndTime = execution.EndTime,
                Status = execution.Status,
                Parameters = execution.Parameters,
                OutputPath = execution.OutputPath,
                ErrorMessage = execution.ErrorMessage,
                RecordCount = execution.RecordCount,
                FileSizeBytes = execution.FileSizeBytes,
                ExecutionTimeSeconds = execution.ExecutionTimeSeconds,
                ExecutedBy = execution.ExecutedBy,
                IsScheduled = execution.IsScheduled,
                ScheduleId = execution.ScheduleId,
                CreatedAt = execution.CreatedAt,
                IsCompleted = execution.Status == "Completed",
                HasError = !string.IsNullOrEmpty(execution.ErrorMessage),
                FormattedFileSize = execution.FileSizeBytes > 0 ? $"{execution.FileSizeBytes / 1024.0 / 1024.0:F2} MB" : "0 MB"
            };

            return Ok(ApiResponse<ReportExecutionDto>.SuccessResponse(responseDto, "Report execution started"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportExecutionDto>.ErrorResponse("Error executing report", ex.Message));
        }
    }

    /// <summary>
    /// Get report executions
    /// </summary>
    [HttpGet("{id}/executions")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportExecutionDto>>>> GetReportExecutions(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ReportExecutions
                .Where(e => e.ReportId == id)
                .Select(e => new ReportExecutionDto
                {
                    Id = e.Id,
                    ReportId = e.ReportId,
                    ExecutionId = e.ExecutionId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Status = e.Status,
                    Parameters = e.Parameters,
                    OutputPath = e.OutputPath,
                    ErrorMessage = e.ErrorMessage,
                    RecordCount = e.RecordCount,
                    FileSizeBytes = e.FileSizeBytes,
                    ExecutionTimeSeconds = e.ExecutionTimeSeconds,
                    ExecutedBy = e.ExecutedBy,
                    IsScheduled = e.IsScheduled,
                    ScheduleId = e.ScheduleId,
                    CreatedAt = e.CreatedAt,
                    IsCompleted = e.Status == "Completed",
                    HasError = !string.IsNullOrEmpty(e.ErrorMessage),
                    FormattedFileSize = e.FileSizeBytes > 0 ? $"{e.FileSizeBytes / 1024.0 / 1024.0:F2} MB" : "0 MB"
                })
                .OrderByDescending(e => e.StartTime)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ReportExecutionDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ReportExecutionDto>>.ErrorResponse("Error retrieving report executions", ex.Message));
        }
    }

    /// <summary>
    /// Get report templates
    /// </summary>
    [HttpGet("{id}/templates")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportTemplateDto>>>> GetReportTemplates(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ReportTemplates
                .Where(t => t.ReportId == id)
                .Select(t => new ReportTemplateDto
                {
                    Id = t.Id,
                    ReportId = t.ReportId,
                    Name = t.Name,
                    TemplateContent = t.TemplateContent,
                    TemplateEngine = t.TemplateEngine,
                    OutputFormat = t.OutputFormat,
                    HeaderContent = t.HeaderContent,
                    FooterContent = t.FooterContent,
                    StyleSheet = t.StyleSheet,
                    IsDefault = t.IsDefault,
                    Version = t.Version,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ReportTemplateDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ReportTemplateDto>>.ErrorResponse("Error retrieving report templates", ex.Message));
        }
    }

    /// <summary>
    /// Add template to report
    /// </summary>
    [HttpPost("{id}/templates")]
    public async Task<ActionResult<ApiResponse<ReportTemplateDto>>> CreateReportTemplate(Guid id, CreateReportTemplateDto dto)
    {
        try
        {
            if (!await ReportExists(id))
            {
                return NotFound(ApiResponse<ReportTemplateDto>.ErrorResponse("Report not found"));
            }

            var template = new ReportTemplate
            {
                Id = Guid.NewGuid(),
                ReportId = id,
                Name = dto.Name,
                TemplateContent = dto.TemplateContent,
                TemplateEngine = dto.TemplateEngine,
                OutputFormat = dto.OutputFormat,
                HeaderContent = dto.HeaderContent,
                FooterContent = dto.FooterContent,
                StyleSheet = dto.StyleSheet,
                IsDefault = dto.IsDefault,
                Version = dto.Version
            };

            _context.ReportTemplates.Add(template);
            await _context.SaveChangesAsync();

            var responseDto = new ReportTemplateDto
            {
                Id = template.Id,
                ReportId = template.ReportId,
                Name = template.Name,
                TemplateContent = template.TemplateContent,
                TemplateEngine = template.TemplateEngine,
                OutputFormat = template.OutputFormat,
                HeaderContent = template.HeaderContent,
                FooterContent = template.FooterContent,
                StyleSheet = template.StyleSheet,
                IsDefault = template.IsDefault,
                Version = template.Version,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };

            return Ok(ApiResponse<ReportTemplateDto>.SuccessResponse(responseDto, "Report template created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportTemplateDto>.ErrorResponse("Error creating report template", ex.Message));
        }
    }

    /// <summary>
    /// Get report schedules
    /// </summary>
    [HttpGet("{id}/schedules")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportScheduleDto>>>> GetReportSchedules(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ReportSchedules
                .Where(s => s.ReportId == id)
                .Select(s => new ReportScheduleDto
                {
                    Id = s.Id,
                    ReportId = s.ReportId,
                    Name = s.Name,
                    CronExpression = s.CronExpression,
                    Frequency = s.Frequency,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    NextRunTime = s.NextRunTime,
                    LastRunTime = s.LastRunTime,
                    Status = s.Status,
                    IsEnabled = s.IsEnabled,
                    Parameters = s.Parameters,
                    Recipients = s.Recipients,
                    DeliveryMethod = s.DeliveryMethod,
                    RetryAttempts = s.RetryAttempts,
                    MaxRetries = s.MaxRetries,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    IsActive = s.Status == "Active" && s.IsEnabled,
                    IsOverdue = s.NextRunTime.HasValue && s.NextRunTime < DateTime.UtcNow
                })
                .OrderByDescending(s => s.CreatedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ReportScheduleDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ReportScheduleDto>>.ErrorResponse("Error retrieving report schedules", ex.Message));
        }
    }

    /// <summary>
    /// Add schedule to report
    /// </summary>
    [HttpPost("{id}/schedules")]
    public async Task<ActionResult<ApiResponse<ReportScheduleDto>>> CreateReportSchedule(Guid id, CreateReportScheduleDto dto)
    {
        try
        {
            if (!await ReportExists(id))
            {
                return NotFound(ApiResponse<ReportScheduleDto>.ErrorResponse("Report not found"));
            }

            var schedule = new ReportSchedule
            {
                Id = Guid.NewGuid(),
                ReportId = id,
                Name = dto.Name,
                CronExpression = dto.CronExpression,
                Frequency = dto.Frequency,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                IsEnabled = dto.IsEnabled,
                Parameters = dto.Parameters,
                Recipients = dto.Recipients,
                DeliveryMethod = dto.DeliveryMethod,
                RetryAttempts = dto.RetryAttempts,
                MaxRetries = dto.MaxRetries
            };

            _context.ReportSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var responseDto = new ReportScheduleDto
            {
                Id = schedule.Id,
                ReportId = schedule.ReportId,
                Name = schedule.Name,
                CronExpression = schedule.CronExpression,
                Frequency = schedule.Frequency,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                NextRunTime = schedule.NextRunTime,
                LastRunTime = schedule.LastRunTime,
                Status = schedule.Status,
                IsEnabled = schedule.IsEnabled,
                Parameters = schedule.Parameters,
                Recipients = schedule.Recipients,
                DeliveryMethod = schedule.DeliveryMethod,
                RetryAttempts = schedule.RetryAttempts,
                MaxRetries = schedule.MaxRetries,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt,
                IsActive = schedule.Status == "Active" && schedule.IsEnabled,
                IsOverdue = schedule.NextRunTime.HasValue && schedule.NextRunTime < DateTime.UtcNow
            };

            return Ok(ApiResponse<ReportScheduleDto>.SuccessResponse(responseDto, "Report schedule created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportScheduleDto>.ErrorResponse("Error creating report schedule", ex.Message));
        }
    }

    /// <summary>
    /// Get report permissions
    /// </summary>
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportPermissionDto>>>> GetReportPermissions(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ReportPermissions
                .Where(p => p.ReportId == id)
                .Select(p => new ReportPermissionDto
                {
                    Id = p.Id,
                    ReportId = p.ReportId,
                    UserId = p.UserId,
                    UserEmail = p.UserEmail,
                    Permission = p.Permission,
                    CanView = p.CanView,
                    CanExecute = p.CanExecute,
                    CanSchedule = p.CanSchedule,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete,
                    ExpiryDate = p.ExpiryDate,
                    GrantedBy = p.GrantedBy,
                    CreatedAt = p.CreatedAt,
                    IsExpired = p.ExpiryDate.HasValue && p.ExpiryDate < DateTime.Today
                })
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ReportPermissionDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ReportPermissionDto>>.ErrorResponse("Error retrieving report permissions", ex.Message));
        }
    }

    /// <summary>
    /// Add permission to report
    /// </summary>
    [HttpPost("{id}/permissions")]
    public async Task<ActionResult<ApiResponse<ReportPermissionDto>>> CreateReportPermission(Guid id, CreateReportPermissionDto dto)
    {
        try
        {
            if (!await ReportExists(id))
            {
                return NotFound(ApiResponse<ReportPermissionDto>.ErrorResponse("Report not found"));
            }

            var permission = new ReportPermission
            {
                Id = Guid.NewGuid(),
                ReportId = id,
                UserId = dto.UserId,
                UserEmail = dto.UserEmail,
                Permission = dto.Permission,
                CanView = dto.CanView,
                CanExecute = dto.CanExecute,
                CanSchedule = dto.CanSchedule,
                CanEdit = dto.CanEdit,
                CanDelete = dto.CanDelete,
                ExpiryDate = dto.ExpiryDate,
                GrantedBy = dto.GrantedBy
            };

            _context.ReportPermissions.Add(permission);
            await _context.SaveChangesAsync();

            var responseDto = new ReportPermissionDto
            {
                Id = permission.Id,
                ReportId = permission.ReportId,
                UserId = permission.UserId,
                UserEmail = permission.UserEmail,
                Permission = permission.Permission,
                CanView = permission.CanView,
                CanExecute = permission.CanExecute,
                CanSchedule = permission.CanSchedule,
                CanEdit = permission.CanEdit,
                CanDelete = permission.CanDelete,
                ExpiryDate = permission.ExpiryDate,
                GrantedBy = permission.GrantedBy,
                CreatedAt = permission.CreatedAt,
                IsExpired = permission.ExpiryDate.HasValue && permission.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<ReportPermissionDto>.SuccessResponse(responseDto, "Report permission created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportPermissionDto>.ErrorResponse("Error creating report permission", ex.Message));
        }
    }

    private async Task<bool> ReportExists(Guid id)
    {
        return await _context.Reports.AnyAsync(e => e.Id == id);
    }
}

