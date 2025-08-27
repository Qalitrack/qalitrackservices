using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories;

public interface IUserShiftRepository : IRepository<UserShift>
{
    new Task<IEnumerable<UserShift>> GetAllAsync();  // Get all user shifts
    new Task<UserShift> CreateAsync(UserShift userShift);  // Assign a user to a shift
    Task<bool> IsUserAssignedToShiftAsync(string userId, string shiftId);
    Task<IEnumerable<UserShift>> GetShiftsForUserAsync(string? userId, string? shiftId);
    Task<IEnumerable<UserShift>> GetUsersAssignedToShiftAsync(string shiftId); // Get users assigned to a shift
    Task<bool> DeleteAsync(string id, string shiftId);
}