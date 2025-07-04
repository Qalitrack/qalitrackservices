using DriverService.Core.Entities;

namespace DriverService.Core.Interfaces;

public interface IDriverLicenseRepository : IRepository<DriverLicense>
{
    Task<DriverLicense?> GetByDriverIdAsync(string driverId);
    Task<DriverLicense?> GetByLicenseNumberAsync(string licenseNumber);
    Task<IEnumerable<DriverLicense>> GetExpiringLicensesAsync(int daysAhead = 30);
    Task<IEnumerable<DriverLicense>> GetLicensesByStatusAsync(LicenseStatus status);
    Task<IEnumerable<DriverLicense>> GetExpiredLicensesAsync();
}