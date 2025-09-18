namespace UserService.Core.Interfaces.Services
{
    public interface IShiftLoginRestrictionService
    {
        /// <summary>
        /// Checks if a user can login based on shift restrictions.
        /// This method ONLY checks restrictions and does NOT handle attendance.
        /// </summary>
        /// <param name="userId">The user ID to check.</param>
        /// <returns>A tuple indicating if login is allowed and the reason.</returns>
        Task<(bool IsAllowed, string Reason)> CanUserLoginAsync(string userId);

        /// <summary>
        /// Handles attendance after successful login completion.
        /// This should be called only after the user has fully completed login (including 2FA).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if attendance was handled, false otherwise.</returns>
        Task<bool> HandleLoginAttendanceAsync(string userId);

        /// <summary>
        /// Handles user logout and attendance (auto clock-out for strict shifts).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="currentDateTime">The current date and time for recording logout.</param>
        Task HandleUserLogoutAsync(string userId, DateTime currentDateTime);
    }
}