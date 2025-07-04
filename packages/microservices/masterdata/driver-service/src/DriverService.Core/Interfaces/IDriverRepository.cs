using DriverService.Core.Entities;

namespace DriverService.Core.Interfaces;

public interface IDriverRepository : IRepository<Driver>
{
    Task<IEnumerable<Driver>> GetActiveDriversAsync();
    Task<IEnumerable<Driver>> GetDriversByStatusAsync(DriverStatus status);
    Task<IEnumerable<Driver>> GetDriversWithExpiringLicensesAsync(int daysAhead = 30);
    Task<IEnumerable<Driver>> SearchDriversAsync(string searchTerm);
    Task<Driver?> GetDriverByEmployeeIdAsync(string employeeId);
    Task<Driver?> GetDriverByEmailAsync(string email);
    Task<Driver?> GetDriverWithLicenseAsync(string driverId);
    Task<Driver?> GetDriverWithProfileAsync(string driverId);
    Task<Driver?> GetDriverWithAllDetailsAsync(string driverId);
}