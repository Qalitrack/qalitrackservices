using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Report?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}