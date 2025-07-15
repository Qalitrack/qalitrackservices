using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IShiftRepository : IRepository<Shift>  // Inheriting from IRepository for basic CRUD operations
    {
        // Get all shifts (already provided by IRepository)
        Task<IEnumerable<Shift>> GetAllAsync();

        // Get a specific shift by ID (already provided by IRepository)
        Task<Shift?> GetByIdAsync(string id);

        // Create a new shift (already provided by IRepository)
        Task<Shift> CreateAsync(Shift shift);

        // Update an existing shift (already provided by IRepository)
        Task<Shift?> UpdateAsync(Shift shift);

        // Delete a shift by ID (already provided by IRepository)
        Task<bool> DeleteAsync(string id);

        // Check if a shift is active based on current time and shift start/end time
        Task<bool> IsShiftActiveAsync(string shiftId);

        // Assign a user to a shift
        Task<bool> AssignUserToShiftAsync(string userId, string shiftId);

        // Remove a user from a shift
        Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId);

    }
}