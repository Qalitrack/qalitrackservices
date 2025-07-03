using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly string _basePath;
    private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

    public DocumentService(
        IDocumentRepository documentRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
        
        // Ensure documents directory exists
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<IEnumerable<TransactionDocumentDto>> GetDocumentsAsync(string transactionId)
    {
        var documents = await _documentRepository.GetActiveByTransactionIdAsync(transactionId);
        return _mapper.Map<IEnumerable<TransactionDocumentDto>>(documents);
    }

    public async Task<TransactionDocumentDto> UploadDocumentAsync(string transactionId, IFormFile file, string documentType, string? description = null)
    {
        // Validate transaction exists
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with ID {transactionId} not found");

        // Validate file
        ValidateFile(file);

        // Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var transactionFolder = Path.Combine(_basePath, transactionId);
        
        // Ensure transaction folder exists
        if (!Directory.Exists(transactionFolder))
            Directory.CreateDirectory(transactionFolder);

        var filePath = Path.Combine(transactionFolder, fileName);

        // Get next version number for this document type
        var existingDocument = await _documentRepository.GetLatestVersionAsync(transactionId, documentType);
        var nextVersion = existingDocument?.Version + 1 ?? 1;

        // Deactivate previous versions of the same document type
        if (existingDocument != null)
        {
            var previousVersions = await _documentRepository.GetByTransactionIdAsync(transactionId);
            foreach (var doc in previousVersions.Where(d => d.DocumentType == documentType && d.IsActive))
            {
                doc.IsActive = false;
                await _documentRepository.UpdateAsync(doc);
            }
        }

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Calculate checksums
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var md5Hash = CalculateMd5Hash(fileBytes);
        var sha256Hash = CalculateSha256Hash(fileBytes);

        // Create document entity
        var document = new TransactionDocument
        {
            TransactionId = transactionId,
            FileName = fileName,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath,
            DocumentType = documentType,
            Description = description,
            Version = nextVersion,
            IsActive = true,
            ChecksumMd5 = md5Hash,
            ChecksumSha256 = sha256Hash
        };

        var createdDocument = await _documentRepository.AddAsync(document);
        return _mapper.Map<TransactionDocumentDto>(createdDocument);
    }

    public async Task<(byte[] content, string contentType, string fileName)> DownloadDocumentAsync(string documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
            throw new InvalidOperationException($"Document with ID {documentId} not found");

        if (!document.IsActive)
            throw new InvalidOperationException("Document is not active");

        if (!File.Exists(document.FilePath))
            throw new InvalidOperationException("Physical file not found on disk");

        var fileBytes = await File.ReadAllBytesAsync(document.FilePath);
        
        // Verify file integrity
        var currentMd5 = CalculateMd5Hash(fileBytes);
        if (currentMd5 != document.ChecksumMd5)
            throw new InvalidOperationException("File integrity check failed");

        return (fileBytes, document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
            return false;

        // Soft delete - mark as inactive and deleted
        document.IsActive = false;
        await _documentRepository.UpdateAsync(document);
        
        // Actually delete from repository
        var result = await _documentRepository.DeleteByIdAsync(documentId);

        // Optionally delete physical file (be careful with this in production)
        try
        {
            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }
        }
        catch
        {
            // Log error but don't fail the operation
            // In production, you might want to implement a cleanup job
        }

        return result;
    }

    public async Task<TransactionDocumentDto?> GetLatestVersionAsync(string transactionId, string documentType)
    {
        var document = await _documentRepository.GetLatestVersionAsync(transactionId, documentType);
        return document == null ? null : _mapper.Map<TransactionDocumentDto>(document);
    }

    public async Task<bool> ValidateDocumentAsync(string transactionId, string documentType)
    {
        var document = await _documentRepository.GetLatestVersionAsync(transactionId, documentType);
        if (document == null)
            return false;

        // Check if physical file exists
        if (!File.Exists(document.FilePath))
            return false;

        // Verify file integrity
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(document.FilePath);
            var currentMd5 = CalculateMd5Hash(fileBytes);
            return currentMd5 == document.ChecksumMd5;
        }
        catch
        {
            return false;
        }
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        if (file.Length > _maxFileSize)
            throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

        // Additional validation for content type
        if (string.IsNullOrEmpty(file.ContentType))
            throw new ArgumentException("File content type is required");

        // Basic content type validation
        var allowedContentTypes = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/jpg", 
            "image/png",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain"
        };

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException($"Content type '{file.ContentType}' is not allowed");
    }

    private string CalculateMd5Hash(byte[] input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(input);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string CalculateSha256Hash(byte[] input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(input);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}