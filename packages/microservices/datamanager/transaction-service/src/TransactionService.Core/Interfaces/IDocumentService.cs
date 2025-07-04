using Microsoft.AspNetCore.Http;
using TransactionService.Core.DTOs;

namespace TransactionService.Core.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<TransactionDocumentDto>> GetDocumentsAsync(string transactionId);
    Task<TransactionDocumentDto> UploadDocumentAsync(string transactionId, IFormFile file, string documentType, string? description = null);
    Task<(byte[] content, string contentType, string fileName)> DownloadDocumentAsync(string documentId);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<TransactionDocumentDto?> GetLatestVersionAsync(string transactionId, string documentType);
    Task<bool> ValidateDocumentAsync(string transactionId, string documentType);
}