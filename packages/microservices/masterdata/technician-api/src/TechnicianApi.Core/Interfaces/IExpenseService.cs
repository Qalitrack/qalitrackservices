using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IExpenseService
{
    Task<ExpenseResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<ExpenseResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? tripId = null);
    Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto);
    Task<bool> DeleteAsync(string id);
}
