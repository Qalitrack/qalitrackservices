using DriverService.Core.Entities;

namespace DriverService.Core.Interfaces;

public interface IDriverDocumentRepository : IRepository<DriverDocument>
{
    Task<IEnumerable<DriverDocument>> GetByDriverIdAsync(string driverId);
    Task<IEnumerable<DriverDocument>> GetExpiringDocumentsAsync(int daysAhead = 30);
    Task<IEnumerable<DriverDocument>> GetDocumentsByTypeAsync(DocumentType documentType);
    Task<IEnumerable<DriverDocument>> GetUnverifiedDocumentsAsync();
}