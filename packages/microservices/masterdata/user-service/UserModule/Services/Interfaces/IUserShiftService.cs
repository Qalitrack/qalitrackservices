using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserModule.Dtos.Shift;

namespace UserModule.Services.Interfaces
{
    public interface IUserShiftService
    {
        Task<IEnumerable<UserShiftReadDto>> GetAllUserShiftsAsync();
        Task<IEnumerable<UserShiftReadDto>> GetUserShiftsAsync(Guid userId);
        Task<IEnumerable<UserShiftReadDto>> GetUsersInShiftAsync(Guid shiftId);
        Task<UserShiftReadDto> AssignShiftToUserAsync(UserShiftAssignDto assignDto);
        Task<bool> UpdateUserShiftAsync(Guid assignmentId, UserShiftAssignDto updateDto);
        Task<bool> RemoveUserShiftAsync(Guid assignmentId);
        Task<UserShiftReadDto?> GetCurrentUserShiftAsync(Guid userId);
    }
}
