using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class ShiftService : IShiftService
{
    public async Task<IEnumerable<ShiftDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<ShiftDto?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<ShiftDto> CreateAsync(CreateShiftDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<ShiftDto?> UpdateAsync(string id, UpdateShiftDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsShiftActive(string shiftId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AssignUserToShiftAsync(string userId, string shiftId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> ValidateUserCredentials(string email, string password)
    {
        throw new NotImplementedException();
    }
}