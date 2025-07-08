using UserModule.Dtos.Shift;

namespace UserModule.Services;

public interface IShiftService
    {
        // Shift CRUD Operations
        Task<IEnumerable<ShiftReadDto>> GetAllShiftsAsync();
        Task<ShiftReadDto?> GetShiftByIdAsync(Guid id);
        Task<ShiftReadDto> CreateShiftAsync(ShiftCreateDto shiftCreateDto);
        Task<bool> UpdateShiftAsync(Guid id, ShiftCreateDto shiftUpdateDto);
        Task<bool> DeleteShiftAsync(Guid id);
        
        // Shift Mode Operations
        Task<bool> UpdateShiftModeAsync(Guid shiftId, string mode);
        
        // User Authentication and Authorization
        Task<bool> CanUserSignInAsync(Guid userId, Guid shiftId);
        Task<(bool CanLogin, string Message)> CanUserLoginAsync(Guid userId);
        Task<bool> IsUserInActiveShiftAsync(Guid userId);
        Task<bool> IsStrictModeEnabledAsync();
        
        // Admin Operations
        Task SetStrictModeAsync(bool isStrictMode);
        Task LogoutUsersAfterShiftEndAsync();
        
        // Utility Methods
        Task LogAuditEventAsync(Guid? userId, string action, string description);
        bool IsTimeInShift(TimeSpan currentTime, TimeSpan shiftStartTime, TimeSpan shiftEndTime);
    }
    

            
    
    