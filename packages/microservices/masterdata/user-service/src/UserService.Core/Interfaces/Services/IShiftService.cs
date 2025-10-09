using UserService.Core.DTOs.Shift;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs;

namespace UserService.Core.Interfaces.Services
{
    public interface IShiftService
    {
        // Updated method to support pagination
        Task<PagedResult<ShiftDto>> GetAllAsync(PaginationParameters parameters);
        
        Task<ShiftResponse?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<bool> AssignUserToShiftAsync(string userId, string shiftId);
        Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId);
        Task<object?> IsUserAssignedToShiftAsync(string userId, string shiftId);
        Task<MassAssignShiftResultDto> MassAssignShiftToRoleAsync(string roleId, string shiftId);
        Task<MassAssignShiftResultDto> MassRemoveUsersFromShiftByRoleAsync(string roleId, string shiftId);
        Task<UsersAssignedToShiftDto> GetUsersAssignedToShiftAsync(string? shiftId);
        Task<IEnumerable<ShiftDto>> GetShiftsForUserAsync(string userId);
        Task<PagedResult<ShiftDto>> GetDeletedPagedAsync(PaginationParameters parameters);
        Task<ShiftResponse> CreateEnhancedAsync(CreateShiftRequest request);
        Task<ShiftResponse?> UpdateEnhancedAsync(string id, UpdateShiftRequest request);
        Task<PagedResult<UserShiftDto>> GetDeletedUserShiftsPagedAsync(PaginationParameters parameters);
    }
}