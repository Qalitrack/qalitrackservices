using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptService _service;
    private readonly IFileStorageService _fileStorage;

    public ReceiptsController(
        IReceiptService service,
        IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] string expenseId, IFormFile image, [FromForm] string? note)
    {
        var userId = User.Identity?.Name ?? "system";

        // Upload the receipt image
        var attachment = await _fileStorage.SaveFileAsync(image, "Receipt", expenseId, userId, note ?? "Receipt");
        var imageUrl = _fileStorage.GetFileUrl(attachment.FilePath);

        // Create receipt record
        var created = await _service.CreateAsync(expenseId, imageUrl, note, userId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var receipt = await _service.GetByIdAsync(id);
        return receipt == null ? NotFound() : Ok(receipt);
    }

    [HttpGet("expense/{expenseId}")]
    public async Task<IActionResult> GetByExpenseId(string expenseId)
    {
        var receipts = await _service.GetByExpenseIdAsync(expenseId);
        return Ok(receipts);
    }

    [HttpPost("{id}/extract-details")]
    public async Task<IActionResult> ExtractDetails(string id)
    {
        try
        {
            var ocrData = await _service.ExtractDetailsAsync(id);
            return Ok(ocrData);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
