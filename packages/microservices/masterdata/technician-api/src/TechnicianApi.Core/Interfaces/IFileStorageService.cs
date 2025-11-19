using Microsoft.AspNetCore.Http;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface IFileStorageService
{
    Task<Attachment> SaveFileAsync(
        IFormFile file, 
        string entityType,  // e.g., "PerDiemReturnForm", "AdvanceReturnForm", "Claim"
        string entityId,    // The ID of the entity this attachment belongs to
        string userId,      // ID of the user uploading the file
        string? description = null);
        
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> DeleteAttachmentAsync(Guid attachmentId);
    string GetFileUrl(string filePath);
    string GetFilePath(string fileName);
    
    // Get all attachments for a specific entity
    Task<IEnumerable<Attachment>> GetAttachmentsForEntityAsync(string entityType, string entityId);
    
    // Get a single attachment by ID
    Task<Attachment?> GetAttachmentAsync(Guid attachmentId);
}
