using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Claim;
using TechnicianApi.Core.DTOs.FileUpload;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Core.DTOs.Attachment;

namespace TechnicianApi.Api.Controllers;

[Route("/claims")]
public class ClaimsController : BaseController
{
    private readonly IClaimService _service;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(
        IClaimService service,
        IFileStorageService fileStorage,
        ILogger<ClaimsController> logger)
    {
        _service = service;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var claim = await _service.GetByIdAsync(id);
        return claim == null ? NotFound("Claim not found") : Ok(claim);
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetByAssignmentId(string assignmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByAssignmentIdAsync(assignmentId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClaimDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Created(created, "Claim created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateClaimDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated == null ? NotFound("Claim not found or cannot be updated") : Ok(updated, "Claim updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? Ok("Claim deleted successfully") : NotFound("Claim not found");
    }

    [HttpPost("{id}/attachments")]
    [Authorize]
    public async Task<IActionResult> UploadAttachment(
        string id,
        [FromForm] FileUploadDto fileUpload)
    {
        try
        {
            var claim = await _service.GetByIdAsync(id);
            if (claim == null)
                return NotFound("Claim not found");

            var userId = User.Identity?.Name ?? "system";
            var attachment = await _fileStorage.SaveFileAsync(
                fileUpload.File,
                nameof(Claim),
                id,
                userId,
                fileUpload.Description
            );

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var attachmentDto = new AttachmentDto
            {
                Id = Guid.Parse(attachment.Id),
                FileName = attachment.FileName,
                FileUrl = $"{baseUrl}/api/claims/attachments/{attachment.Id}",
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
        var claim = await _service.GetByIdAsync(id);
        if (claim == null)
            return NotFound("Claim not found");

        return Ok(claim.Attachments);
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

    [HttpPost("{id}/manager-approve")]
    public async Task<IActionResult> ManagerApprove(string id, [FromBody] ApproveClaimDto dto)
    {
        var approved = await _service.ManagerApproveAsync(id, dto);
        return approved == null ? NotFound("Claim not found or cannot be approved") : Ok(approved, "Claim approved by manager");
    }

    [HttpPost("{id}/manager-reject")]
    public async Task<IActionResult> ManagerReject(string id, [FromBody] RejectClaimDto dto)
    {
        var rejected = await _service.ManagerRejectAsync(id, dto);
        return rejected == null ? NotFound("Claim not found or cannot be rejected") : Ok(rejected, "Claim rejected by manager");
    }

    [HttpPost("{id}/cfo-approve")]
    public async Task<IActionResult> CfoApprove(string id, [FromBody] ApproveClaimDto dto)
    {
        var approved = await _service.CfoApproveAsync(id, dto);
        return approved == null ? NotFound("Claim not found or cannot be approved") : Ok(approved, "Claim approved by CFO");
    }

    [HttpPost("{id}/cfo-reject")]
    public async Task<IActionResult> CfoReject(string id, [FromBody] RejectClaimDto dto)
    {
        var rejected = await _service.CfoRejectAsync(id, dto);
        return rejected == null ? NotFound("Claim not found or cannot be rejected") : Ok(rejected, "Claim rejected by CFO");
    }

    [HttpPost("{id}/disburse")]
    public async Task<IActionResult> Disburse(string id, [FromBody] DisburseClaimDto dto)
    {
        var disbursed = await _service.DisburseAsync(id, dto);
        return disbursed == null ? NotFound("Claim not found or cannot be disbursed") : Ok(disbursed, "Claim disbursed successfully");
    }
}
