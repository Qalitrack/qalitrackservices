using DriverService.Core.Entities;

namespace DriverService.Core.Interfaces;

public interface IDriverProfileRepository : IRepository<DriverProfile>
{
    Task<DriverProfile?> GetByDriverIdAsync(string driverId);
    Task<IEnumerable<DriverProfile>> GetTopPerformersAsync(int count = 10);
    Task<IEnumerable<DriverProfile>> GetDriversByExperienceAsync(int minYears, int maxYears);
}