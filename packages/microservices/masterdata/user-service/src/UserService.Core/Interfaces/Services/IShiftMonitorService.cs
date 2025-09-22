using System;
using System.Threading.Tasks;

namespace UserService.Core.Interfaces.Services
{
    public interface IShiftMonitorService
    {
        /// <summary>
        /// Handles user logout process including attendance tracking and token revocation
        /// </summary>
        /// <param name="userId">The ID of the user to log out</param>
        /// <param name="logoutTime">The time when the user logged out</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task HandleUserLogoutAsync(string userId, DateTime logoutTime);
        
        /// <summary>
        /// Monitors and processes shift instances that have ended
        /// </summary>
        Task MonitorStrictShiftsAsync();
        
        /// <summary>
        /// Monitors and updates shift statuses
        /// </summary>
        Task MonitorShiftStatusAsync();
    }
}
