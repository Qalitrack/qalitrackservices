using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AnalyticsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(string organizationId, AnalyticsRequestDto request)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var measurements = await _unitOfWork.WeightMeasurements.SearchAsync(
            organizationId, null, null, fromDate, toDate, 0, int.MaxValue);

        var weighbridges = await _unitOfWork.WeighbridgeStatuses.GetByOrganizationAsync(organizationId);

        var measurementsList = measurements.ToList();
        
        var summary = new AnalyticsSummaryDto
        {
            TotalMeasurements = measurementsList.Count,
            PendingMeasurements = measurementsList.Count(m => m.Status == MeasurementStatus.Pending),
            ValidatedMeasurements = measurementsList.Count(m => m.Status == MeasurementStatus.Validated),
            CorrectedMeasurements = measurementsList.Count(m => m.Status == MeasurementStatus.Corrected),
            TotalWeight = measurementsList.Sum(m => m.Weight),
            AverageWeight = measurementsList.Any() ? measurementsList.Average(m => m.Weight) : 0,
            ActiveWeighbridges = weighbridges.Count(w => w.Status == MaintenanceStatus.Active),
            MaintenanceWeighbridges = weighbridges.Count(w => w.Status == MaintenanceStatus.Maintenance)
        };

        if (request.IncludeWeighbridgeUsage)
        {
            summary.WeighbridgeUsage = await GetWeighbridgeUsageAsync(organizationId, fromDate, toDate);
        }

        if (request.IncludeDailyBreakdown)
        {
            summary.DailyWeights = await GetDailyWeightSummaryAsync(organizationId, fromDate, toDate);
        }

        return summary;
    }

    public async Task<List<WeighbridgeUsageDto>> GetWeighbridgeUsageAsync(string organizationId, DateTime? fromDate, DateTime? toDate)
    {
        var measurements = await _unitOfWork.WeightMeasurements.SearchAsync(
            organizationId, null, null, fromDate, toDate, 0, int.MaxValue);

        var weighbridges = await _unitOfWork.WeighbridgeStatuses.GetByOrganizationAsync(organizationId);

        var usage = measurements
            .GroupBy(m => m.WeighbridgeId)
            .Select(g => new WeighbridgeUsageDto
            {
                WeighbridgeId = g.Key,
                Name = weighbridges.FirstOrDefault(w => w.WeighbridgeId == g.Key)?.Name ?? "Unknown",
                MeasurementCount = g.Count(),
                TotalWeight = g.Sum(m => m.Weight),
                AverageWeight = g.Average(m => m.Weight)
            })
            .OrderByDescending(u => u.MeasurementCount)
            .ToList();

        return usage;
    }

    public async Task<List<DailyWeightDto>> GetDailyWeightSummaryAsync(string organizationId, DateTime? fromDate, DateTime? toDate)
    {
        var measurements = await _unitOfWork.WeightMeasurements.SearchAsync(
            organizationId, null, null, fromDate, toDate, 0, int.MaxValue);

        var dailyWeights = measurements
            .GroupBy(m => m.MeasurementDateTime.Date)
            .Select(g => new DailyWeightDto
            {
                Date = g.Key,
                MeasurementCount = g.Count(),
                TotalWeight = g.Sum(m => m.Weight),
                AverageWeight = g.Average(m => m.Weight)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return dailyWeights;
    }
}