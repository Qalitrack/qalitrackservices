using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterDriverRepository : IRepository<TransporterDriver>
{
    Task<IEnumerable<TransporterDriver>> GetDriversByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterDriver>> GetActiveDriversAsync(string transporterId);
    Task<IEnumerable<TransporterDriver>> GetDriversByStatusAsync(string transporterId, DriverStatus status);
    Task<TransporterDriver?> GetByDriverIdAsync(string driverId);
    Task<TransporterDriver?> GetByLicenseNumberAsync(string licenseNumber);
    Task<IEnumerable<TransporterDriver>> GetDriversDueForMedicalCheckAsync(string transporterId);
    Task<bool> IsDriverAssignedAsync(string driverId);
}