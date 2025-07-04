using Microsoft.EntityFrameworkCore;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Infrastructure.Data;

namespace SupplierService.Infrastructure.Repositories;

public class SupplierPerformanceRepository : Repository<SupplierPerformance>, ISupplierPerformanceRepository
{
    public SupplierPerformanceRepository(SupplierDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierPerformance>> GetBySupplierIdAsync(string supplierId)
    {
        return await _dbSet
            .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }

    public async Task<SupplierPerformance?> GetBySupplierAndPeriodAsync(string supplierId, int year, int month)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SupplierId == supplierId && p.Year == year && p.Month == month && !p.IsDeleted);
    }

    public async Task<IEnumerable<SupplierPerformance>> GetByPeriodAsync(int year, int? month = null)
    {
        var query = _dbSet.Where(p => p.Year == year && !p.IsDeleted);
        
        if (month.HasValue)
        {
            query = query.Where(p => p.Month == month.Value);
        }
        
        return await query.ToListAsync();
    }

    public async Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null)
    {
        var query = _dbSet.Where(p => p.SupplierId == supplierId && p.OverallRating.HasValue && !p.IsDeleted);
        
        if (months.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddMonths(-months.Value);
            query = query.Where(p => p.CreatedAt >= cutoffDate);
        }
        
        return await query.AverageAsync(p => p.OverallRating);
    }

    public override async Task<IEnumerable<SupplierPerformance>> GetAllAsync()
    {
        return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
    }

    public override async Task<SupplierPerformance?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }
}