using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface IReceiptService
{
    Task<Receipt> CreateAsync(string expenseId, string imageUrl, string? note, string userId);
    Task<Receipt?> GetByIdAsync(string id);
    Task<IEnumerable<Receipt>> GetByExpenseIdAsync(string expenseId);
    Task<object> ExtractDetailsAsync(string id);
    Task<bool> DeleteAsync(string id);
}
