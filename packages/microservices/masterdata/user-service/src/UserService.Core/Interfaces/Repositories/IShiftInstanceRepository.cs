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

    // Shift instance specific methods
    /// <summary>
    /// Gets all incomplete shift instances that should have been completed by the specified time
    /// </summary>
    /// <param name="endTime">The cutoff time for shift end times</param>
    /// <param name="batchSize">Number of records to process in each batch (default: 1000)</param>
    /// <returns>List of shift instances that are past their scheduled end time but still in progress</returns>
    Task<IEnumerable<ShiftInstance>> GetIncompleteInstancesBeforeAsync(DateTime endTime, int batchSize = 1000);
    Task<IEnumerable<ShiftInstance>> GetInstancesByShiftIdAsync(string shiftId);
    Task<IEnumerable<ShiftInstance>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAsync(ShiftInstanceStatus status);
    Task<IEnumerable<ShiftInstance>> GetUpcomingInstancesAsync(int days = 7);
    Task<IEnumerable<ShiftInstance>> GenerateShiftInstancesAsync(string shiftId, DateTime startDate, DateTime endDate);
    Task<bool> CancelShiftInstanceAsync(string shiftInstanceId, string reason);
    Task AddRangeAsync(List<ShiftInstance> batch);
    Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAndTimeAsync(ShiftInstanceStatus status, DateTime startTime, DateTime endTime);
    
    /// <summary>
    /// Gets all scheduled shift instances that should have started by the specified time but haven't been started yet
    /// </summary>
    /// <param name="endTime">The cutoff time to check against</param>
    /// <returns>List of shift instances that are past their scheduled start time but still in Scheduled status</returns>
    Task<IEnumerable<ShiftInstance>> GetScheduledInstancesBeforeAsync(DateTime endTime);
}
