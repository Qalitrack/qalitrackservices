using AutoMapper;
using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Entities;
using AnalyticsService.Core.Interfaces;

namespace AnalyticsService.Core.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IAnalyticsEngine _analyticsEngine;
    private readonly IMetricsCalculator _metricsCalculator;
    private readonly IMapper _mapper;

    public AnalyticsService(
        IAnalyticsRepository analyticsRepository,
        IAnalyticsEngine analyticsEngine,
        IMetricsCalculator metricsCalculator,
        IMapper mapper)
    {
        _analyticsRepository = analyticsRepository;
        _analyticsEngine = analyticsEngine;
        _metricsCalculator = metricsCalculator;
        _mapper = mapper;
    }

    public async Task<AnalyticsDto> CreateAnalyticsAsync(CreateAnalyticsRequest request)
    {
        var analytics = new Analytics
        {
            AnalyticsName = request.AnalyticsName,
            AnalyticsType = request.AnalyticsType,
            OrganizationId = request.OrganizationId,
            Description = request.Description,
            AggregationType = request.AggregationType,
            TimeGranularity = request.TimeGranularity,
            DataStartDate = request.DataStartDate,
            DataEndDate = request.DataEndDate,
            DataSourcesList = request.DataSources,
            FilterCriteriaObject = request.FilterCriteria,
            IsRealTime = request.IsRealTime,
            IsScheduled = request.IsScheduled,
            RefreshIntervalMinutes = request.RefreshIntervalMinutes,
            BusinessCategory = request.BusinessCategory,
            BusinessOwner = request.BusinessOwner,
            Priority = request.Priority,
            BusinessPurpose = request.BusinessPurpose,
            DataRetentionDays = request.DataRetentionDays,
            AutoArchive = request.AutoArchive,
            Configuration = request.Configuration,
            Metadata = request.Metadata,
            Status = AnalyticsStatus.Draft
        };

        if (request.IsScheduled && request.RefreshIntervalMinutes.HasValue)
        {
            analytics.NextRefreshDate = DateTime.UtcNow.AddMinutes(request.RefreshIntervalMinutes.Value);
        }

        var createdAnalytics = await _analyticsRepository.AddAsync(analytics);
        return _mapper.Map<AnalyticsDto>(createdAnalytics);
    }

    public async Task<AnalyticsDto?> GetAnalyticsAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        return analytics == null ? null : _mapper.Map<AnalyticsDto>(analytics);
    }

    public async Task<AnalyticsDto> UpdateAnalyticsAsync(string analyticsId, UpdateAnalyticsRequest request)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null)
            throw new ArgumentException("Analytics not found", nameof(analyticsId));

        if (!string.IsNullOrEmpty(request.AnalyticsName))
            analytics.AnalyticsName = request.AnalyticsName;

        if (!string.IsNullOrEmpty(request.Description))
            analytics.Description = request.Description;

        if (request.AnalyticsType.HasValue)
            analytics.AnalyticsType = request.AnalyticsType.Value;

        if (request.Status.HasValue)
            analytics.Status = request.Status.Value;

        if (request.AggregationType.HasValue)
            analytics.AggregationType = request.AggregationType.Value;

        if (request.TimeGranularity.HasValue)
            analytics.TimeGranularity = request.TimeGranularity.Value;

        if (request.DataStartDate.HasValue)
            analytics.DataStartDate = request.DataStartDate;

        if (request.DataEndDate.HasValue)
            analytics.DataEndDate = request.DataEndDate;

        if (request.DataSources != null)
            analytics.DataSourcesList = request.DataSources;

        if (request.FilterCriteria != null)
            analytics.FilterCriteriaObject = request.FilterCriteria;

        if (request.IsRealTime.HasValue)
            analytics.IsRealTime = request.IsRealTime.Value;

        if (request.IsScheduled.HasValue)
            analytics.IsScheduled = request.IsScheduled.Value;

        if (request.RefreshIntervalMinutes.HasValue)
        {
            analytics.RefreshIntervalMinutes = request.RefreshIntervalMinutes.Value;
            if (analytics.IsScheduled)
            {
                analytics.NextRefreshDate = DateTime.UtcNow.AddMinutes(request.RefreshIntervalMinutes.Value);
            }
        }

        if (!string.IsNullOrEmpty(request.BusinessCategory))
            analytics.BusinessCategory = request.BusinessCategory;

        if (!string.IsNullOrEmpty(request.BusinessOwner))
            analytics.BusinessOwner = request.BusinessOwner;

        if (request.Priority.HasValue)
            analytics.Priority = request.Priority.Value;

        if (!string.IsNullOrEmpty(request.BusinessPurpose))
            analytics.BusinessPurpose = request.BusinessPurpose;

        if (request.DataRetentionDays.HasValue)
            analytics.DataRetentionDays = request.DataRetentionDays;

        if (request.AutoArchive.HasValue)
            analytics.AutoArchive = request.AutoArchive.Value;

        if (request.Configuration != null)
            analytics.Configuration = request.Configuration;

        if (request.Metadata != null)
            analytics.Metadata = request.Metadata;

        analytics.UpdatedAt = DateTime.UtcNow;

        var updatedAnalytics = await _analyticsRepository.UpdateAsync(analytics);
        return _mapper.Map<AnalyticsDto>(updatedAnalytics);
    }

    public async Task<bool> DeleteAnalyticsAsync(string analyticsId)
    {
        return await _analyticsRepository.DeleteByIdAsync(analyticsId);
    }

    public async Task<List<AnalyticsDto>> GetAnalyticsByOrganizationAsync(string organizationId)
    {
        var analyticsList = await _analyticsRepository.GetByOrganizationAsync(organizationId);
        return _mapper.Map<List<AnalyticsDto>>(analyticsList);
    }

    public async Task<List<AnalyticsDto>> GetAnalyticsByTypeAsync(AnalyticsType analyticsType)
    {
        var analyticsList = await _analyticsRepository.GetByTypeAsync(analyticsType);
        return _mapper.Map<List<AnalyticsDto>>(analyticsList);
    }

    public async Task<List<AnalyticsDto>> GetAnalyticsByStatusAsync(AnalyticsStatus status)
    {
        var analyticsList = await _analyticsRepository.GetByStatusAsync(status);
        return _mapper.Map<List<AnalyticsDto>>(analyticsList);
    }

    public async Task<AnalyticsResultDto> ExecuteAnalyticsAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null)
            throw new ArgumentException("Analytics not found", nameof(analyticsId));

        analytics.Status = AnalyticsStatus.Processing;
        analytics.LastCalculationDate = DateTime.UtcNow;
        await _analyticsRepository.UpdateAsync(analytics);

        try
        {
            var startTime = DateTime.UtcNow;

            // Execute analytics through the engine
            var result = await _analyticsEngine.ExecuteAnalyticsAsync(analytics);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Update analytics with results
            analytics.Status = AnalyticsStatus.Completed;
            analytics.LastRefreshDate = DateTime.UtcNow;
            analytics.LastProcessingDuration = duration;
            analytics.TotalRecordsProcessed = result.RecordsProcessed;
            analytics.Results = result.Data;
            analytics.CalculationErrors = null;

            if (analytics.IsScheduled && analytics.RefreshIntervalMinutes.HasValue)
            {
                analytics.NextRefreshDate = DateTime.UtcNow.AddMinutes(analytics.RefreshIntervalMinutes.Value);
            }

            // Update average processing duration
            if (analytics.AverageProcessingDuration.HasValue)
            {
                analytics.AverageProcessingDuration = TimeSpan.FromMilliseconds(
                    (analytics.AverageProcessingDuration.Value.TotalMilliseconds + duration.TotalMilliseconds) / 2);
            }
            else
            {
                analytics.AverageProcessingDuration = duration;
            }

            await _analyticsRepository.UpdateAsync(analytics);

            return new AnalyticsResultDto
            {
                AnalyticsId = analyticsId,
                AnalyticsName = analytics.AnalyticsName,
                ExecutionDate = startTime,
                Status = "Completed",
                ProcessingDuration = duration,
                RecordsProcessed = result.RecordsProcessed,
                Data = result.Data,
                Metadata = result.Metadata
            };
        }
        catch (Exception ex)
        {
            analytics.Status = AnalyticsStatus.Failed;
            analytics.CalculationErrors = ex.Message;
            await _analyticsRepository.UpdateAsync(analytics);

            return new AnalyticsResultDto
            {
                AnalyticsId = analyticsId,
                AnalyticsName = analytics.AnalyticsName,
                ExecutionDate = DateTime.UtcNow,
                Status = "Failed",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AnalyticsResultDto> ExecuteCustomAnalyticsAsync(CustomAnalyticsRequest request)
    {
        try
        {
            var result = await _analyticsEngine.ExecuteCustomAnalyticsAsync(request);
            
            return new AnalyticsResultDto
            {
                AnalyticsName = request.Name,
                ExecutionDate = DateTime.UtcNow,
                Status = "Completed",
                RecordsProcessed = result.RecordsProcessed,
                Data = result.Data,
                Metadata = result.Metadata
            };
        }
        catch (Exception ex)
        {
            return new AnalyticsResultDto
            {
                AnalyticsName = request.Name,
                ExecutionDate = DateTime.UtcNow,
                Status = "Failed",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<AnalyticsResultDto>> ExecuteBulkAnalyticsAsync(List<string> analyticsIds)
    {
        var results = new List<AnalyticsResultDto>();

        foreach (var analyticsId in analyticsIds)
        {
            var result = await ExecuteAnalyticsAsync(analyticsId);
            results.Add(result);
        }

        return results;
    }

    public async Task<AnalyticsResultDto> RefreshAnalyticsAsync(string analyticsId)
    {
        return await ExecuteAnalyticsAsync(analyticsId);
    }

    public async Task<bool> EnableRealTimeAnalyticsAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null) return false;

        analytics.IsRealTime = true;
        analytics.RefreshIntervalMinutes = 1; // 1 minute for real-time
        analytics.UpdatedAt = DateTime.UtcNow;

        await _analyticsRepository.UpdateAsync(analytics);
        return true;
    }

    public async Task<bool> DisableRealTimeAnalyticsAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null) return false;

        analytics.IsRealTime = false;
        analytics.RefreshIntervalMinutes = null;
        analytics.UpdatedAt = DateTime.UtcNow;

        await _analyticsRepository.UpdateAsync(analytics);
        return true;
    }

    public async Task<List<AnalyticsDto>> GetRealTimeAnalyticsAsync(string organizationId)
    {
        var realTimeAnalytics = await _analyticsRepository.GetRealTimeAnalyticsAsync(organizationId);
        return _mapper.Map<List<AnalyticsDto>>(realTimeAnalytics);
    }

    public async Task<RealTimeDataDto> GetRealTimeDataAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null || !analytics.IsRealTime)
            throw new ArgumentException("Real-time analytics not found or not enabled", nameof(analyticsId));

        var data = await _analyticsEngine.GetRealTimeDataAsync(analytics);
        
        return new RealTimeDataDto
        {
            AnalyticsId = analyticsId,
            Timestamp = DateTime.UtcNow,
            Data = data,
            IsLive = true
        };
    }

    public async Task<bool> ScheduleAnalyticsAsync(string analyticsId, ScheduleRequest schedule)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null) return false;

        analytics.IsScheduled = true;
        analytics.RefreshIntervalMinutes = schedule.IntervalMinutes;
        analytics.NextRefreshDate = schedule.StartDate ?? DateTime.UtcNow.AddMinutes(schedule.IntervalMinutes);
        analytics.UpdatedAt = DateTime.UtcNow;

        await _analyticsRepository.UpdateAsync(analytics);
        return true;
    }

    public async Task<bool> UnscheduleAnalyticsAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null) return false;

        analytics.IsScheduled = false;
        analytics.RefreshIntervalMinutes = null;
        analytics.NextRefreshDate = null;
        analytics.UpdatedAt = DateTime.UtcNow;

        await _analyticsRepository.UpdateAsync(analytics);
        return true;
    }

    public async Task<List<ScheduledAnalyticsDto>> GetScheduledAnalyticsAsync(string organizationId)
    {
        var scheduledAnalytics = await _analyticsRepository.GetScheduledAnalyticsAsync(organizationId);
        
        return scheduledAnalytics.Select(a => new ScheduledAnalyticsDto
        {
            AnalyticsId = a.Id,
            AnalyticsName = a.AnalyticsName,
            NextRunDate = a.NextRefreshDate ?? DateTime.UtcNow,
            IntervalMinutes = a.RefreshIntervalMinutes ?? 60,
            LastRunDate = a.LastRefreshDate,
            Status = a.Status.ToString()
        }).ToList();
    }

    public async Task<bool> ExecuteScheduledAnalyticsAsync()
    {
        var dueAnalytics = await _analyticsRepository.GetDueScheduledAnalyticsAsync();
        
        foreach (var analytics in dueAnalytics)
        {
            try
            {
                await ExecuteAnalyticsAsync(analytics.Id);
            }
            catch (Exception ex)
            {
                // Log error but continue with other analytics
                analytics.CalculationErrors = ex.Message;
                await _analyticsRepository.UpdateAsync(analytics);
            }
        }

        return true;
    }

    public async Task<AnalyticsPerformanceDto> GetAnalyticsPerformanceAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null)
            throw new ArgumentException("Analytics not found", nameof(analyticsId));

        return new AnalyticsPerformanceDto
        {
            AnalyticsId = analyticsId,
            AnalyticsName = analytics.AnalyticsName,
            AverageProcessingDuration = analytics.AverageProcessingDuration,
            LastProcessingDuration = analytics.LastProcessingDuration,
            TotalRecordsProcessed = analytics.TotalRecordsProcessed,
            MemoryUsageBytes = analytics.MemoryUsedBytes,
            CpuUsagePercentage = analytics.CpuUsagePercentage,
            LastCalculationDate = analytics.LastCalculationDate,
            SuccessRate = CalculateSuccessRate(analytics)
        };
    }

    public async Task<List<AnalyticsPerformanceDto>> GetPerformanceSummaryAsync(string organizationId)
    {
        var analyticsList = await _analyticsRepository.GetByOrganizationAsync(organizationId);
        
        return analyticsList.Select(a => new AnalyticsPerformanceDto
        {
            AnalyticsId = a.Id,
            AnalyticsName = a.AnalyticsName,
            AverageProcessingDuration = a.AverageProcessingDuration,
            LastProcessingDuration = a.LastProcessingDuration,
            TotalRecordsProcessed = a.TotalRecordsProcessed,
            SuccessRate = CalculateSuccessRate(a)
        }).ToList();
    }

    public async Task<bool> OptimizeAnalyticsAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null) return false;

        // Implement optimization logic
        var optimized = await _analyticsEngine.OptimizeAnalyticsAsync(analytics);
        
        if (optimized)
        {
            analytics.UpdatedAt = DateTime.UtcNow;
            await _analyticsRepository.UpdateAsync(analytics);
        }

        return optimized;
    }

    public async Task<List<DataSourceDto>> GetAvailableDataSourcesAsync()
    {
        return await _analyticsEngine.GetAvailableDataSourcesAsync();
    }

    public async Task<bool> ValidateDataSourceAsync(string dataSource)
    {
        return await _analyticsEngine.ValidateDataSourceAsync(dataSource);
    }

    public async Task<DataQualityDto> CheckDataQualityAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null)
            throw new ArgumentException("Analytics not found", nameof(analyticsId));

        return await _analyticsEngine.CheckDataQualityAsync(analytics);
    }

    public async Task<DataLineageDto> GetDataLineageAsync(string analyticsId)
    {
        var analytics = await _analyticsRepository.GetByIdAsync(analyticsId);
        if (analytics == null)
            throw new ArgumentException("Analytics not found", nameof(analyticsId));

        return await _analyticsEngine.GetDataLineageAsync(analytics);
    }

    // Helper methods
    private decimal CalculateSuccessRate(Analytics analytics)
    {
        // This would be implemented based on execution history
        // For now, return a simple calculation
        if (analytics.Status == AnalyticsStatus.Completed)
            return 100m;
        if (analytics.Status == AnalyticsStatus.Failed)
            return 0m;
        return 50m; // Partial or in progress
    }
}