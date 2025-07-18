using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class HistoricalAnalysisRepository : Repository<HistoricalAnalysis>, IHistoricalAnalysisRepository
{
    public HistoricalAnalysisRepository(WeightDataContext context) : base(context)
    {
    }

    public async Task<List<HistoricalAnalysis>> GetByWeighbridgeIdAsync(string weighbridgeId, string organizationId)
    {
        return await _context.Set<HistoricalAnalysis>()
            .Where(a => a.WeighbridgeId == weighbridgeId && a.OrganizationId == organizationId)
            .OrderByDescending(a => a.AnalysisDate)
            .ToListAsync();
    }

    public async Task<List<HistoricalAnalysis>> GetByAnalysisTypeAsync(AnalysisType type, string organizationId)
    {
        return await _context.Set<HistoricalAnalysis>()
            .Where(a => a.Type == type && a.OrganizationId == organizationId)
            .OrderByDescending(a => a.AnalysisDate)
            .ToListAsync();
    }

    public async Task<List<HistoricalAnalysis>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, string organizationId)
    {
        return await _context.Set<HistoricalAnalysis>()
            .Where(a => a.PeriodStart >= fromDate && a.PeriodEnd <= toDate && a.OrganizationId == organizationId)
            .OrderByDescending(a => a.AnalysisDate)
            .ToListAsync();
    }

    public async Task<HistoricalAnalysis?> GetLatestAnalysisAsync(string weighbridgeId, AnalysisType type, string organizationId)
    {
        return await _context.Set<HistoricalAnalysis>()
            .Where(a => a.WeighbridgeId == weighbridgeId && a.Type == type && a.OrganizationId == organizationId)
            .OrderByDescending(a => a.AnalysisDate)
            .FirstOrDefaultAsync();
    }
}