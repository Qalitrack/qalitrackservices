using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;
    private readonly IFileStorageService _fileStorage;

    public ExpensesController(IExpenseService service, IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tripId = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, tripId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto, [FromServices] ITripService tripService)
    {
        var result = await _service.CreateAsync(dto);

        // Automatically recalculate trip total cost after adding expense
        await tripService.RecalculateTotalCostAsync(dto.TripId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromServices] ITripService tripService)
    {
        // Get expense before deleting to know which trip to update
        var expense = await _service.GetByIdAsync(id);
        var tripId = expense?.TripId;

        var result = await _service.DeleteAsync(id);

        // Recalculate trip cost after deleting expense
        if (result && tripId != null)
        {
            await tripService.RecalculateTotalCostAsync(tripId);
        }

        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/upload-receipt")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadReceipt(string id, IFormFile file)
    {
        var expense = await _service.GetByIdAsync(id);
        if (expense == null) return NotFound();

        var userId = User.Identity?.Name ?? "system";
        var attachment = await _fileStorage.SaveFileAsync(file, "Expense", id, userId, "Receipt");

        return Ok(new { url = _fileStorage.GetFileUrl(attachment.FilePath) });
    }

    [HttpGet("{id}/receipts")]
    public async Task<IActionResult> GetExpenseReceipts(string id)
    {
        var attachments = await _fileStorage.GetAttachmentsForEntityAsync("Expense", id);
        return Ok(attachments);
    }
}
