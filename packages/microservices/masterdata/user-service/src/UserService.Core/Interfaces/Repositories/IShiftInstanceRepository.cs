using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Core.Interfaces.Repositories;
public interface IShiftInstanceRepository : IRepository<ShiftInstance>
{
    Task<IEnumerable<ShiftInstance>> GetAllAsync();
    Task<ShiftInstance?> GetByIdAsync (string id);
    Task<ShiftInstance> CreateAsync(ShiftInstance shiftInstance);
    Task<ShiftInstance?> UpdateAsync(ShiftInstance shiftInstance);
    Task<bool> DeleteAsync(string id);
    
    Task<IEnumerable<ShiftInstance>> GetInstancesByShiftIdAsync(string shiftId);
    Task<IEnumerable<ShiftInstance>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAsync(ShiftInstanceStatus status);
    Task<IEnumerable<ShiftInstance>> GetUpcomingInstancesAsync(int days = 7);
    Task<bool> CancelShiftInstanceAsync(string shiftInstanceId, string reason);
    Task AddRangeAsync(List<ShiftInstance> batch);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAndTimeAsync(ShiftInstanceStatus status, DateTime startTime, DateTime endTime);
    
    
}
