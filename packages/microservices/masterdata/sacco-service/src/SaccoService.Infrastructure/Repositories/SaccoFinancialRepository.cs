using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoFinancialRepository : Repository<SaccoFinancial>, ISaccoFinancialRepository
{
    public SaccoFinancialRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<SaccoFinancial?> GetBySaccoIdAsync(string saccoId)
    {
        return await _dbSet
            .Include(f => f.Sacco)
            .FirstOrDefaultAsync(f => !f.IsDeleted && f.SaccoId == saccoId);
    }

    public async Task<IEnumerable<SaccoFinancial>> GetByFinancialYearAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(f => f.Sacco)
            .Where(f => !f.IsDeleted && 
                   f.FinancialYearStart >= startDate && 
                   f.FinancialYearEnd <= endDate)
            .OrderBy(f => f.FinancialYearStart)
            .ToListAsync();
    }
}