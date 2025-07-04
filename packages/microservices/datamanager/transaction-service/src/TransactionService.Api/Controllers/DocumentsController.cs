using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : BaseController
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Get documents for a transaction
    /// </summary>
    [HttpGet("transaction/{transactionId}")]
    public async Task<IActionResult> GetDocuments(string transactionId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsAsync(transactionId);
            return HandleResult(documents);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Upload a document for a transaction
    /// </summary>
    [HttpPost("transaction/{transactionId}/upload")]
    public async Task<IActionResult> UploadDocument(
        string transactionId,
        IFormFile file,
        [FromForm] string documentType,
        [FromForm] string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            var document = await _documentService.UploadDocumentAsync(transactionId, file, documentType, description);
            return Ok(new ApiResponse<TransactionDocumentDto>
            {
                Success = true,
                Message = "Document uploaded successfully",
                Data = document
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Download a document
    /// </summary>
    [HttpGet("{documentId}/download")]
    public async Task<IActionResult> DownloadDocument(string documentId)
    {
        try
        {
            var (content, contentType, fileName) = await _documentService.DownloadDocumentAsync(documentId);
            return File(content, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Document not found"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteDocument(string documentId)
    {
        try
        {
            var result = await _documentService.DeleteDocumentAsync(documentId);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Document not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Document deleted successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get latest version of a document type for a transaction
    /// </summary>
    [HttpGet("transaction/{transactionId}/type/{documentType}/latest")]
    public async Task<IActionResult> GetLatestVersion(string transactionId, string documentType)
    {
        try
        {
            var document = await _documentService.GetLatestVersionAsync(transactionId, documentType);
            return HandleResult(document, "Document not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate if required documents are present for a transaction
    /// </summary>
    [HttpPost("transaction/{transactionId}/validate/{documentType}")]
    public async Task<IActionResult> ValidateDocument(string transactionId, string documentType)
    {
        try
        {
            var isValid = await _documentService.ValidateDocumentAsync(transactionId, documentType);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = isValid ? "Document validation passed" : "Document validation failed",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}