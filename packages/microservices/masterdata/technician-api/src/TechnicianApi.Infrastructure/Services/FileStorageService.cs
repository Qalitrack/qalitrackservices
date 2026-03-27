using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly IRepository<Attachment> _attachmentRepository;
    private const string UploadsFolder = "Uploads";

    public FileStorageService(
        IWebHostEnvironment env,
        IConfiguration configuration,
        IRepository<Attachment> attachmentRepository)
    {
        _env = env;
        _configuration = configuration;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<Attachment> SaveFileAsync(
        IFormFile file, 
        string entityType, 
        string entityId, 
        string userId,
        string? description = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded");

        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, UploadsFolder, entityType);
        Directory.CreateDirectory(uploadsPath);

        // Generate unique file name
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);
        var relativePath = Path.Combine(entityType, fileName).Replace("\\", "/");

        // Save file
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Create and save attachment
        var attachment = new Attachment
        {
            FileName = file.FileName,
            FilePath = relativePath,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedBy = userId,
            Description = description,
            EntityType = entityType,
            EntityId = entityId
        };

        await _attachmentRepository.CreateAsync(attachment);

        return attachment;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = GetFilePath(filePath);
        
        if (!File.Exists(fullPath)) 
            return false;
            
        await Task.Run(() => File.Delete(fullPath));
        return true;
    }
    
    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId.ToString());
        if (attachment == null)
            return false;

        // Delete the physical file
        var deleted = await DeleteFileAsync(attachment.FilePath);

        if (deleted)
        {
            // Remove the database record
            await _attachmentRepository.DeleteAsync(attachmentId.ToString());
            return true;
        }

        return false;
    }
    
    public async Task<IEnumerable<Attachment>> GetAttachmentsForEntityAsync(string entityType, string entityId)
    {
        var attachments = await _attachmentRepository.FindAsync(a => a.EntityType == entityType && a.EntityId == entityId);
        return attachments.OrderByDescending(a => a.UploadedAt);
    }
    
    public async Task<Attachment?> GetAttachmentAsync(Guid attachmentId)
    {
        return await _attachmentRepository.GetByIdAsync(attachmentId.ToString());
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
            
        var baseUrl = _configuration["BaseUrl"] ?? "https://yourdomain.com";
        return $"{baseUrl}/{UploadsFolder}/{filePath}".Replace("\\", "/");
    }

    public string GetFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
            
        return Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, UploadsFolder, filePath);
    }
}
