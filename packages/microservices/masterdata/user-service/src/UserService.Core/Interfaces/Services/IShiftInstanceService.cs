using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Services;
public interface IShiftInstanceService
{
    // Basic CRUD operations
    Task<IEnumerable<ShiftInstanceResponse>> GetAllAsync();
    Task<ShiftInstanceResponse?> GetByIdAsync(string id);
    Task<ShiftInstanceResponse> CreateAsync(ShiftInstance shiftInstance);
    Task<ShiftInstanceResponse?> UpdateAsync(string id, ShiftInstance shiftInstance);
    Task<bool> DeleteAsync(string id);

    // Instance generation and management
    Task<IEnumerable<ShiftInstanceResponse>> GenerateInstancesAsync(ShiftInstanceGenerateRequest request);
    Task<IEnumerable<ShiftInstanceResponse>> GetInstancesByShiftAsync(string shiftId);
    Task<IEnumerable<ShiftInstanceResponse>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShiftInstanceResponse>> GetUpcomingInstancesAsync(int days = 7);
    Task<bool> CancelInstanceAsync(string shiftInstanceId, string reason);
    Task<IEnumerable<ShiftInstanceResponse>> UpdateShiftInstancesAsync(string shiftId, ShiftInstanceGenerateRequest request);
    // Status management
    Task<ShiftInstanceResponse?> GetCurrentActiveInstanceAsync(string shiftId);
}

