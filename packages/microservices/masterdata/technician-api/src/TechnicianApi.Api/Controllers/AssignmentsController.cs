using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.DTOs.FileUpload;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.DTOs.Attachment;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<AssignmentsController> _logger;

    public AssignmentsController(
        IAssignmentService service,
        IFileStorageService fileStorage,
        ILogger<AssignmentsController> logger)
    {
        _service = service;
        _fileStorage = fileStorage;
        _logger = logger;
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
        [FromQuery] int pageSize = 10,
        [FromQuery] string? technicianId = null,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, technicianId, status);
        return Ok(result);
    }

    [HttpGet("technician/{technicianId}")]
    public async Task<IActionResult> GetByTechnicianId(string technicianId)
    {
        var result = await _service.GetByTechnicianIdAsync(technicianId);
        return Ok(result);
    }

    [HttpGet("manager/{managerId}")]
    public async Task<IActionResult> GetByManagerId(string managerId)
    {
        var result = await _service.GetByManagerIdAsync(managerId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAssignmentDto dto)
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

    [HttpPost("{id}/attachments")]
    [Authorize]
    public async Task<IActionResult> UploadAttachment(
        string id,
        [FromForm] FileUploadDto fileUpload)
    {
        try
        {
            var assignment = await _service.GetByIdAsync(id);
            if (assignment == null)
                return NotFound("Assignment not found");

            var userId = User.Identity?.Name ?? "system";
            var attachment = await _fileStorage.SaveFileAsync(
                fileUpload.File,
                nameof(Assignment),
                id,
                userId,
                fileUpload.Description
            );

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var attachmentDto = new AttachmentDto
            {
                Id = Guid.Parse(attachment.Id),
                FileName = attachment.FileName,
                FileUrl = $"{baseUrl}/api/assignments/attachments/{attachment.Id}",
                ContentType = attachment.ContentType,
                Description = attachment.Description,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt
            };

            return CreatedAtAction(
                nameof(GetAttachment),
                new { assignmentId = id, attachmentId = attachment.Id },
                attachmentDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    [HttpGet("{assignmentId}/attachments")]
    public async Task<IActionResult> GetAttachments(string assignmentId)
    {
        var assignment = await _service.GetByIdAsync(assignmentId);
        if (assignment == null)
            return NotFound("Assignment not found");

        return Ok(assignment.Attachments);
    }

    [HttpGet("attachments/{attachmentId}")]
    public async Task<IActionResult> GetAttachment(Guid attachmentId)
    {
        var attachment = await _fileStorage.GetAttachmentAsync(attachmentId);
        if (attachment == null)
            return NotFound();

        var filePath = _fileStorage.GetFilePath(attachment.FilePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found");

        var fileStream = System.IO.File.OpenRead(filePath);
        return File(fileStream, attachment.ContentType, attachment.FileName);
    }

    [HttpDelete("attachments/{attachmentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteAttachment(Guid attachmentId)
    {
        var success = await _fileStorage.DeleteAttachmentAsync(attachmentId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> Accept(string id, [FromQuery] string technicianId)
    {
        var result = await _service.AcceptAssignmentAsync(id, technicianId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/decline")]
    public async Task<IActionResult> Decline(string id, [FromQuery] string technicianId)
    {
        var result = await _service.DeclineAssignmentAsync(id, technicianId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id, [FromQuery] string technicianId)
    {
        var result = await _service.StartAssignmentAsync(id, technicianId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(string id, [FromQuery] string technicianId)
    {
        var result = await _service.CompleteAssignmentAsync(id, technicianId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/assign-technician")]
    public async Task<IActionResult> AssignTechnician(string id, [FromQuery] string technicianId)
    {
        var result = await _service.AssignTechnicianAsync(id, technicianId);
        return result == null ? NotFound("Assignment or technician not found") : Ok(result);
    }

    [HttpPost("{id}/unassign-technician")]
    public async Task<IActionResult> UnassignTechnician(string id, [FromQuery] string technicianId)
    {
        var result = await _service.UnassignTechnicianAsync(id, technicianId);
        return result == null ? NotFound("Assignment or technician not found") : Ok(result);
    }
}
