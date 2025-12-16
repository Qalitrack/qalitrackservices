using System.IO;
using System.Threading.Tasks;

namespace Transaction.Core.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to the storage service
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">The name to give the uploaded file</param>
    /// <param name="containerName">The container/folder to store the file in</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName = "transaction-images");
    
    /// <summary>
    /// Deletes a file from the storage service
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteFileAsync(string fileUrl);
}
