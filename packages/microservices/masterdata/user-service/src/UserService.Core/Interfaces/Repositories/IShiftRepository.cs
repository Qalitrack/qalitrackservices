using UserService.Core.Entities;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Enums;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IShiftRepository : IRepository<Shift>
    {
        // Existing methods for backward compatibi
        Task<IEnumerable<Shift>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<Shift?> GetByIdAsync(string id);
        new Task<Shift> CreateAsync(Shift shift);
        Task<Shift?> UpdateEnhancedAsync(UpdateShiftRequest request, string updatedBy);
        new Task<bool> DeleteAsync(string id);
        Task<bool> IsShiftActiveAsync(string shiftId);
        Task<PagedResult<Shift>> GetDeletedPagedAsync(PaginationParameters parameters);

        // New enhanced scheduling methods
        Task<int> CountAsync();
        Task<IEnumerable<Shift>> GetShiftsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Shift>> GetActiveRecurringShiftsAsync();
        Task<IEnumerable<Shift>> GetShiftsByStatusAsync(ShiftStatus status);
        Task<IEnumerable<Shift>> GetShiftsByFilterAsync(ShiftFilterRequest filter);
    }
}