using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IHistoricalAnalysisRepository : IRepository<HistoricalAnalysis>
{
    Task<List<HistoricalAnalysis>> GetByWeighbridgeIdAsync(string weighbridgeId, string organizationId);
    Task<List<HistoricalAnalysis>> GetByAnalysisTypeAsync(AnalysisType type, string organizationId);
    Task<List<HistoricalAnalysis>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, string organizationId);
    Task<HistoricalAnalysis?> GetLatestAnalysisAsync(string weighbridgeId, AnalysisType type, string organizationId);
}