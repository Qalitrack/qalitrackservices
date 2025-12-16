using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Transaction.Core.Interfaces;

namespace Transaction.Infrastructure.Services
{
    public class FileSystemStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly string _baseUrl;

        public FileSystemStorageService(IConfiguration configuration)
        {
            _basePath = configuration["FileStorage:BasePath"] ?? "wwwroot/uploads";
            _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/uploads";
            
            // Ensure the base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName = "transaction-images")
        {
            // Sanitize the container name and file name
            containerName = SanitizePath(containerName);
            fileName = SanitizeFileName(fileName);
            
            // Create container directory if it doesn't exist
            var containerPath = Path.Combine(_basePath, containerName);
            if (!Directory.Exists(containerPath))
            {
                Directory.CreateDirectory(containerPath);
            }
            
            // Generate a unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(containerPath, uniqueFileName);
            
            // Save the file
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(file);
            }
            
            // Return the URL to access the file
            return $"{_baseUrl.TrimEnd('/')}/{containerName}/{uniqueFileName}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Convert URL to local path
                var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
                var fullPath = Path.Combine(_basePath, relativePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
        
        private static string SanitizePath(string path)
        {
            var invalidChars = Path.GetInvalidPathChars();
            return string.Concat(path.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }
        
        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
