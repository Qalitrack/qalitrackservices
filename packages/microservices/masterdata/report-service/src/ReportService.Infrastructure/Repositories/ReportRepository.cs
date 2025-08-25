using Microsoft.EntityFrameworkCore;
using ReportService.Core.Entities;
using ReportService.Core.Interfaces;
using ReportService.Infrastructure.Data;

namespace ReportService.Infrastructure.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(ReportServiceDbContext context) : base(context)
    {
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return !await _dbSet.AnyAsync(e => e.ReportName.ToLower() == name.ToLower() && !e.IsDeleted);
    }

    public async Task<Report?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.ReportName.ToLower() == name.ToLower() && !e.IsDeleted);
    }
    
    // TODO: Add domain-specific repository methods here
}