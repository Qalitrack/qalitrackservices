using DriverService.Core.Entities;

namespace DriverService.Core.Interfaces;

public interface IDriverViolationRepository : IRepository<DriverViolation>
{
    Task<IEnumerable<DriverViolation>> GetByDriverIdAsync(string driverId);
    Task<IEnumerable<DriverViolation>> GetBySeverityAsync(ViolationSeverity severity);
    Task<IEnumerable<DriverViolation>> GetUnpaidViolationsAsync();
    Task<IEnumerable<DriverViolation>> GetRecentViolationsAsync(int days = 30);
}