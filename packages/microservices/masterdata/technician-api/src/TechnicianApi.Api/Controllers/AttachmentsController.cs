using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.FileUpload;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        IFileStorageService fileStorage,
        ILogger<AttachmentsController> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpPost("upload/{entityType}/{entityId}")]
    public async Task<ActionResult<Attachment>> UploadFile(
        string entityType,
        string entityId,
        [FromForm] FileUploadDto fileUpload)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var attachment = await _fileStorage.SaveFileAsync(
                fileUpload.File,
                entityType,
                entityId,
                userId,
                fileUpload.Description
            );

            return Ok(attachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachments(string entityType, string entityId)
    {
        try
        {
            var attachments = await _fileStorage.GetAttachmentsForEntityAsync(entityType, entityId);
            return Ok(attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments");
            return StatusCode(500, "An error occurred while retrieving attachments");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        try
        {
            var attachment = await _fileStorage.GetAttachmentAsync(id);
            if (attachment == null)
                return NotFound();

            var filePath = _fileStorage.GetFilePath(attachment.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server");

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, attachment.ContentType, attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, "An error occurred while downloading the file");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        try
        {
            var success = await _fileStorage.DeleteAttachmentAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, "An error occurred while deleting the file");
        }
    }
}
