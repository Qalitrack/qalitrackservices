using Microsoft.Extensions.Logging;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class ReceiptService : IReceiptService
{
    private readonly IRepository<Receipt> _receiptRepository;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        IRepository<Receipt> receiptRepository,
        ILogger<ReceiptService> logger)
    {
        _receiptRepository = receiptRepository;
        _logger = logger;
    }

    public async Task<Receipt> CreateAsync(string expenseId, string imageUrl, string? note, string userId)
    {
        var receipt = new Receipt
        {
            ExpenseId = expenseId,
            ImageUrl = imageUrl,
            Note = note,
            UserId = userId
        };

        return await _receiptRepository.CreateAsync(receipt);
    }

    public async Task<Receipt?> GetByIdAsync(string id)
    {
        return await _receiptRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Receipt>> GetByExpenseIdAsync(string expenseId)
    {
        return await _receiptRepository.FindAsync(r => r.ExpenseId == expenseId);
    }

    public async Task<object> ExtractDetailsAsync(string id)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt == null)
        {
            throw new InvalidOperationException($"Receipt with ID {id} not found");
        }

        // TODO: Implement OCR processing here
        var ocrData = new
        {
            merchant = "Placeholder - OCR Not Implemented",
            date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            total = 0.00m,
            items = new[] { "OCR processing not yet implemented" }
        };

        receipt.ReceiptDetailsJson = System.Text.Json.JsonSerializer.Serialize(ocrData);
        await _receiptRepository.UpdateAsync(receipt);

        _logger.LogInformation("OCR extraction placeholder called for receipt {ReceiptId}", id);

        return ocrData;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _receiptRepository.DeleteAsync(id);
    }
}
