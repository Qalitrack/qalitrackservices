using Microsoft.AspNetCore.Http;

namespace TechnicianApi.Core.DTOs.FileUpload;

public class FileUploadDto
{
    public IFormFile File { get; set; } = null!;
    public string? Description { get; set; }
}
