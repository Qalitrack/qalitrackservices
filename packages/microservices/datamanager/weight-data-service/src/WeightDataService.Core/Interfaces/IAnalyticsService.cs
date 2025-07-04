using WeightDataService.Core.DTOs;

namespace WeightDataService.Core.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(string organizationId, AnalyticsRequestDto request);
    Task<List<WeighbridgeUsageDto>> GetWeighbridgeUsageAsync(string organizationId, DateTime? fromDate, DateTime? toDate);
    Task<List<DailyWeightDto>> GetDailyWeightSummaryAsync(string organizationId, DateTime? fromDate, DateTime? toDate);
}