using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IHistoricalAnalysisService
{
    Task<HistoricalAnalysisDto> CreateAnalysisAsync(CreateHistoricalAnalysisDto createDto, string organizationId, string userId);
    Task<HistoricalAnalysisDto?> GetAnalysisAsync(Guid id, string organizationId);
    Task<List<HistoricalAnalysisDto>> GetAnalysesAsync(string organizationId, string? weighbridgeId = null, string? vehicleRegistration = null);
    Task<TrendAnalysisDto> GetTrendAnalysisAsync(string weighbridgeId, DateTime fromDate, DateTime toDate, string? vehicleRegistration = null, string? productType = null);
    Task<List<AnomalyDto>> DetectAnomaliesAsync(string weighbridgeId, DateTime fromDate, DateTime toDate, decimal threshold = 2.0m);
    Task<bool> ScheduleAutomaticAnalysisAsync(string weighbridgeId, AnalysisType type, string organizationId);
    Task RunScheduledAnalysesAsync();
    Task<Dictionary<string, decimal>> GetWeightStatisticsAsync(string weighbridgeId, DateTime fromDate, DateTime toDate, string organizationId);
}