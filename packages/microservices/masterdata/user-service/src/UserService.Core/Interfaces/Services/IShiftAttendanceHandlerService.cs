
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Services
{
    public interface IShiftAttendanceHandlerService
    {
        Task<bool> HandleLoginAttendanceAsync(string userId, Shift shift, DateTime loginTime);
        Task<bool> HandleLogoutAttendanceAsync(string userId, Shift shift, DateTime logoutTime);
        Task<bool> HandleEarlyArrivalAttendanceAsync(string userId, Shift shift, DateTime arrivalTime, string shiftInstanceId, double minutesEarly);
    }
}