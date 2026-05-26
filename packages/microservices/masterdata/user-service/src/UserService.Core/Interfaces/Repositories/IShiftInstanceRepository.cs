using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Core.Interfaces.Repositories;

public interface IShiftInstanceRepository : IRepository<ShiftInstance>
{
    new Task<IEnumerable<ShiftInstance>> GetAllAsync();
    Task<ShiftInstance?> GetByIdAsync(string id);
    new Task<ShiftInstance> CreateAsync(ShiftInstance shiftInstance);
    new Task<ShiftInstance?> UpdateAsync(ShiftInstance shiftInstance);
    new Task<bool> DeleteAsync(string id);
    
    Task<IEnumerable<ShiftInstance>> GetInstancesByShiftIdAsync(string shiftId);
    Task<IEnumerable<ShiftInstance>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAsync(ShiftInstanceStatus status);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusesAsync(IEnumerable<ShiftInstanceStatus> statuses);
    Task<IEnumerable<ShiftInstance>> GetUpcomingInstancesAsync(int days = 7);
    Task<bool> CancelShiftInstanceAsync(string shiftInstanceId, string reason);
    Task AddRangeAsync(List<ShiftInstance> batch);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAndTimeAsync(ShiftInstanceStatus status, DateTime startTime, DateTime endTime);

    // New methods for better exception date handling
    Task<IEnumerable<ShiftInstance>> GenerateShiftInstancesAsync(string shiftId, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Cleans up (soft-deletes) any ShiftInstances that fall on the Shift's exception dates.
    /// Should be called whenever ExceptionDates are updated on a Shift.
    /// </summary>
    Task<int> CleanupInstancesOnExceptionDatesAsync(string shiftId);
}