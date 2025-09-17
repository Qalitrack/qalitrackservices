using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;

namespace UserService.Core.Interfaces.Services;

public interface IShiftNotificationService
{
    Task<bool> SendShiftNotificationsAsync(
        IEnumerable<(string Email, string FullName)> users,
        string shiftInstanceId,
        DateTime startTime,
        DateTime endTime,
        string shiftName,
        NotificationType type,
        string? reason = null,
        string? changes = null);
}