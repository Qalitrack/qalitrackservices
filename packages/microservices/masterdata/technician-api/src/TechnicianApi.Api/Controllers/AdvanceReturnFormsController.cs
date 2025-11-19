using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.AdvanceReturn;
using TechnicianApi.Core.DTOs.FileUpload;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.DTOs.Attachment;

namespace TechnicianApi.Api.Controllers;

[Route("/advance-return-forms")]
public class AdvanceReturnFormsController : BaseController
{
    private readonly IAdvanceReturnFormService _service;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<AdvanceReturnFormsController> _logger;

    public AdvanceReturnFormsController(
        IAdvanceReturnFormService service,
        IFileStorageService fileStorage,
        ILogger<AdvanceReturnFormsController> logger)
    {
        _service = service;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var form = await _service.GetByIdAsync(id);
        return form == null ? NotFound("Advance return form not found") : Ok(form);
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetByAssignmentId(string assignmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByAssignmentIdAsync(assignmentId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdvanceReturnFormDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Created(created, "Advance return form created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAdvanceReturnFormDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated == null ? NotFound("Advance return form not found") : Ok(updated, "Advance return form updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? Ok("Advance return form deleted successfully") : NotFound("Advance return form not found");
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, [FromQuery] string approvedBy, [FromQuery] string? comments = null)
    {
        var approved = await _service.ApproveAsync(id, approvedBy, comments);
        return approved == null ? NotFound("Advance return form not found or cannot be approved") : Ok(approved, "Advance return form approved successfully");
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id, [FromQuery] string rejectedBy, [FromQuery] string rejectionReason)
    {
        var rejected = await _service.RejectAsync(id, rejectedBy, rejectionReason);
        return rejected == null ? NotFound("Advance return form not found or cannot be rejected") : Ok(rejected, "Advance return form rejected");
    }

    [HttpPost("{id}/attachments")]
    [Authorize]
    public async Task<IActionResult> UploadAttachment(
        string id,
        [FromForm] FileUploadDto fileUpload)
    {
        try
        {
            var form = await _service.GetByIdAsync(id);
            if (form == null)
                return NotFound("Advance return form not found");

            var userId = User.Identity?.Name ?? "system";
            var attachment = await _fileStorage.SaveFileAsync(
                fileUpload.File,
                nameof(AdvanceReturnForm),
                id,
                userId,
                fileUpload.Description
            );

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var attachmentDto = new AttachmentDto
            {
                Id = Guid.Parse(attachment.Id),
                FileName = attachment.FileName,
                FileUrl = $"{baseUrl}/api/advance-return-forms/attachments/{attachment.Id}",
                ContentType = attachment.ContentType,
                Description = attachment.Description,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt
            };

            return CreatedAtAction(
                nameof(GetAttachment),
                new { id, attachmentId = attachment.Id },
                attachmentDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    [HttpGet("{id}/attachments")]
    public async Task<IActionResult> GetAttachments(string id)
    {
        var form = await _service.GetByIdAsync(id);
        if (form == null)
            return NotFound("Advance return form not found");

        return Ok(form.Attachments);
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
}
