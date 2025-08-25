using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<bool> IsNameAvailableAsync(string name);
}