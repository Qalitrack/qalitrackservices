using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface IDriverActivityService
{
    Task LogActivityAsync(string driverId, DriverActivityType activityType, string? activityData = null, string? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<DriverActivityResponseDto>> GetDriverActivitiesAsync(string driverId, int limit = 100);
}
