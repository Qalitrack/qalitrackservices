using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterLicenseRepository : IRepository<TransporterLicense>
{
    Task<IEnumerable<TransporterLicense>> GetLicensesByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterLicense>> GetLicensesByTypeAsync(string transporterId, LicenseType type);
    Task<IEnumerable<TransporterLicense>> GetActiveLicensesAsync(string transporterId);
    Task<IEnumerable<TransporterLicense>> GetExpiringLicensesAsync(string transporterId, int daysAhead = 30);
    Task<IEnumerable<TransporterLicense>> GetExpiredLicensesAsync(string transporterId);
    Task<TransporterLicense?> GetByLicenseNumberAsync(string licenseNumber);
}