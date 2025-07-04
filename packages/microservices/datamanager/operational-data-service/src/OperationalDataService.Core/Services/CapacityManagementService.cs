using AutoMapper;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;
using OperationalDataService.Core.Interfaces;
using CapacityForecastDto = OperationalDataService.Core.DTOs.CapacityForecast;
using CapacityRecommendationDto = OperationalDataService.Core.DTOs.CapacityRecommendationDto;
using LoadBalancingMetricDto = OperationalDataService.Core.DTOs.LoadBalancingMetricDto;

namespace OperationalDataService.Core.Services;

public class CapacityManagementService : ICapacityManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMasterDataIntegrationService _masterDataService;

    public CapacityManagementService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMasterDataIntegrationService masterDataService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _masterDataService = masterDataService;
    }

    public async Task<WeighbridgeCapacity> GetCurrentCapacityAsync(string weighbridgeId)
    {
        // Try to get from local database first
        var localCapacity = await _unitOfWork.Repository<CapacityManagement>()
            .FirstOrDefaultAsync(c => c.WeighbridgeId == weighbridgeId);

        if (localCapacity != null && localCapacity.LastUpdated > DateTime.UtcNow.AddMinutes(-5))
        {
            return _mapper.Map<WeighbridgeCapacity>(localCapacity);
        }

        // Get from master data service if local data is stale or not available
        var masterCapacity = await _masterDataService.GetWeighbridgeStatusFromMasterDataAsync(weighbridgeId);
        if (masterCapacity != null)
        {
            // Update local database
            if (localCapacity != null)
            {
                localCapacity.CurrentLoad = masterCapacity.CurrentLoad;
                localCapacity.UtilizationRate = masterCapacity.UtilizationRate;
                localCapacity.VehiclesInQueue = masterCapacity.VehiclesInQueue;
                localCapacity.EstimatedWaitTime = masterCapacity.EstimatedWaitTime;
                localCapacity.LastUpdated = DateTime.UtcNow;

                await _unitOfWork.Repository<CapacityManagement>().UpdateAsync(localCapacity);
                await _unitOfWork.SaveChangesAsync();
            }

            return masterCapacity;
        }

        // Fall back to local data even if stale
        if (localCapacity != null)
        {
            return _mapper.Map<WeighbridgeCapacity>(localCapacity);
        }

        throw new InvalidOperationException($"No capacity data available for weighbridge {weighbridgeId}");
    }

    public async Task<List<WeighbridgeAvailability>> GetAvailableWeighbridgesAsync(DateTime requestedTime, string organizationId)
    {
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId && w.Status == OperationalStatus.Active);

        var availabilities = new List<WeighbridgeAvailability>();

        foreach (var weighbridge in weighbridges)
        {
            var capacity = await GetCurrentCapacityAsync(weighbridge.WeighbridgeId);
            
            var availability = new WeighbridgeAvailability
            {
                WeighbridgeId = weighbridge.WeighbridgeId,
                Name = weighbridge.Name,
                RequestedTime = requestedTime,
                UtilizationRate = capacity.UtilizationRate,
                EstimatedWaitTime = capacity.EstimatedWaitTime
            };

            // Determine availability
            if (capacity.UtilizationRate < 0.8m) // Less than 80% utilization
            {
                availability.IsAvailable = true;
                availability.AvailableFrom = requestedTime;
                availability.Reason = AvailabilityReason.Available;
            }
            else if (capacity.UtilizationRate < 1.0m) // At capacity but not overloaded
            {
                availability.IsAvailable = true;
                availability.AvailableFrom = requestedTime.Add(capacity.EstimatedWaitTime);
                availability.EstimatedWaitTime = capacity.EstimatedWaitTime;
                availability.Reason = AvailabilityReason.Available;
                availability.Notes = "Some wait time expected";
            }
            else
            {
                availability.IsAvailable = false;
                availability.Reason = AvailabilityReason.AtCapacity;
                availability.Notes = "Operating at full capacity";
            }

            // Check for maintenance schedules
            var maintenanceSchedules = await _unitOfWork.Repository<MaintenanceSchedule>()
                .FindAsync(m => m.WeighbridgeId == weighbridge.WeighbridgeId && 
                               m.ScheduledDate.Date == requestedTime.Date &&
                               m.Status == MaintenanceStatus.Scheduled);

            if (maintenanceSchedules.Any())
            {
                availability.IsAvailable = false;
                availability.Reason = AvailabilityReason.Maintenance;
                availability.Restrictions.Add("Scheduled maintenance");
            }

            availabilities.Add(availability);
        }

        return availabilities;
    }

    public async Task<CapacityForecastDto> ForecastCapacityAsync(string weighbridgeId, int forecastHours)
    {
        var currentCapacity = await GetCurrentCapacityAsync(weighbridgeId);
        
        var forecast = new CapacityForecastDto
        {
            WeighbridgeId = weighbridgeId,
            ForecastDate = DateTime.UtcNow,
            ForecastHours = forecastHours,
            CreatedAt = DateTime.UtcNow
        };

        // Get historical data for pattern analysis
        var historicalData = await GetHistoricalCapacityData(weighbridgeId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        
        // Generate hourly forecasts
        for (int hour = 1; hour <= forecastHours; hour++)
        {
            var forecastTime = DateTime.UtcNow.AddHours(hour);
            var hourlyForecast = GenerateHourlyForecast(currentCapacity, historicalData, forecastTime);
            forecast.HourlyForecasts.Add(hourlyForecast);
        }

        // Determine overall trend
        forecast.Trend = DetermineTrend(forecast.HourlyForecasts);
        
        // Calculate confidence based on data quality and patterns
        forecast.Confidence = CalculateForecastConfidence(historicalData, forecast.HourlyForecasts);

        // Add assumptions
        forecast.Assumptions.Add("Based on historical patterns from last 30 days");
        forecast.Assumptions.Add("Weather conditions remain normal");
        forecast.Assumptions.Add("No major disruptions or events");

        return forecast;
    }

    public async Task<LoadBalancingResult> BalanceLoadAsync(List<string> weighbridgeIds, string organizationId)
    {
        var weighbridges = new List<WeighbridgeCapacity>();
        
        foreach (var weighbridgeId in weighbridgeIds)
        {
            var capacity = await GetCurrentCapacityAsync(weighbridgeId);
            weighbridges.Add(capacity);
        }

        // Get pending transactions that need weighbridge assignment
        var pendingTransactions = await GetPendingTransactions(organizationId);

        var result = new LoadBalancingResult
        {
            Strategy = LoadBalancingStrategy.CapacityBased,
            CalculatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddHours(1)
        };

        // Apply load balancing algorithm
        foreach (var transaction in pendingTransactions)
        {
            var optimalWeighbridge = SelectOptimalWeighbridge(weighbridges, transaction);
            
            if (optimalWeighbridge != null)
            {
                var assignment = new WeighbridgeAssignment
                {
                    TransactionId = transaction.Id,
                    WeighbridgeId = optimalWeighbridge.WeighbridgeId,
                    EstimatedStartTime = CalculateEstimatedStartTime(optimalWeighbridge),
                    EstimatedEndTime = CalculateEstimatedEndTime(optimalWeighbridge, transaction.EstimatedDuration),
                    Priority = DetermineAssignmentPriority(transaction),
                    Reason = "Optimal capacity utilization",
                    Score = CalculateAssignmentScore(optimalWeighbridge, transaction)
                };

                result.Assignments.Add(assignment);

                // Update weighbridge projected load
                optimalWeighbridge.CurrentLoad += 1;
                optimalWeighbridge.UtilizationRate = (decimal)optimalWeighbridge.CurrentLoad / optimalWeighbridge.MaxCapacity;
            }
        }

        // Calculate improvement metrics
        result.ImprovementScore = CalculateImprovementScore(weighbridges, result.Assignments);
        result.Recommendations = GenerateLoadBalancingRecommendations(weighbridges, result);

        // Add load balancing metrics
        result.Metrics = CalculateLoadBalancingMetrics(weighbridges, result.Assignments);

        return result;
    }

    public async Task<List<WeighbridgeCapacity>> GetAllCapacitiesAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId);

        var capacities = new List<WeighbridgeCapacity>();

        foreach (var weighbridge in weighbridges)
        {
            try
            {
                var capacity = await GetCurrentCapacityAsync(weighbridge.WeighbridgeId);
                capacities.Add(capacity);
            }
            catch (Exception)
            {
                // If individual weighbridge capacity cannot be retrieved, continue with others
                continue;
            }
        }

        return capacities;
    }

    public async Task<CapacityAnalytics> GetCapacityAnalyticsAsync(string weighbridgeId, TimeRange period)
    {
        var capacityData = await GetHistoricalCapacityData(weighbridgeId, period.StartDate, period.EndDate);
        
        var analytics = new CapacityAnalytics
        {
            WeighbridgeId = weighbridgeId,
            AnalysisDate = DateTime.UtcNow,
            Period = period
        };

        if (capacityData.Any())
        {
            analytics.AverageUtilization = capacityData.Average(c => c.UtilizationRate);
            analytics.PeakUtilization = capacityData.Max(c => c.UtilizationRate);
            analytics.LowestUtilization = capacityData.Min(c => c.UtilizationRate);
            
            var peakData = capacityData.First(c => c.UtilizationRate == analytics.PeakUtilization);
            analytics.PeakTime = peakData.MeasurementDate;
            
            var lowestData = capacityData.First(c => c.UtilizationRate == analytics.LowestUtilization);
            analytics.LowestTime = lowestData.MeasurementDate;
        }

        // Analyze patterns
        analytics.Patterns = AnalyzeCapacityPatterns(capacityData);
        
        // Identify bottlenecks
        analytics.Bottlenecks = IdentifyBottlenecks(capacityData);
        
        // Generate recommendations
        analytics.Recommendations = GenerateCapacityRecommendations(analytics);

        return analytics;
    }

    public async Task UpdateCapacityAsync(string weighbridgeId, int currentLoad, decimal utilizationRate)
    {
        var capacity = await _unitOfWork.Repository<CapacityManagement>()
            .FirstOrDefaultAsync(c => c.WeighbridgeId == weighbridgeId);

        if (capacity == null)
        {
            capacity = new CapacityManagement
            {
                Id = Guid.NewGuid().ToString(),
                WeighbridgeId = weighbridgeId,
                OrganizationId = "default", // This should be determined from context
                MeasurementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<CapacityManagement>().AddAsync(capacity);
        }

        capacity.CurrentLoad = currentLoad;
        capacity.UtilizationRate = utilizationRate;
        capacity.LastUpdated = DateTime.UtcNow;
        capacity.UpdatedAt = DateTime.UtcNow;

        if (capacity.Id == null)
        {
            await _unitOfWork.Repository<CapacityManagement>().AddAsync(capacity);
        }
        else
        {
            await _unitOfWork.Repository<CapacityManagement>().UpdateAsync(capacity);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<CapacityBottleneck>> IdentifyBottlenecksAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId);

        var bottlenecks = new List<CapacityBottleneck>();

        foreach (var weighbridge in weighbridges)
        {
            var capacity = await GetCurrentCapacityAsync(weighbridge.WeighbridgeId);
            
            if (capacity.UtilizationRate > 0.9m) // Over 90% utilization
            {
                var bottleneck = new CapacityBottleneck
                {
                    Location = weighbridge.Name,
                    Description = $"High utilization rate: {capacity.UtilizationRate:P1}",
                    Type = BottleneckType.Capacity,
                    ImpactScore = capacity.UtilizationRate * 100,
                    FirstObserved = DateTime.UtcNow, // This should be tracked historically
                    LastObserved = DateTime.UtcNow,
                    Frequency = 1, // This should be calculated from historical data
                    Causes = DetermineBottleneckCauses(capacity),
                    PotentialSolutions = GenerateBottleneckSolutions(capacity)
                };

                bottlenecks.Add(bottleneck);
            }
        }

        return bottlenecks;
    }

    public async Task<List<CapacityRecommendationDto>> GetCapacityRecommendationsAsync(string weighbridgeId)
    {
        var capacity = await GetCurrentCapacityAsync(weighbridgeId);
        var analytics = await GetCapacityAnalyticsAsync(weighbridgeId, new TimeRange 
        { 
            StartDate = DateTime.UtcNow.AddDays(-7), 
            EndDate = DateTime.UtcNow 
        });

        return GenerateCapacityRecommendations(analytics);
    }

    public async Task<WeighbridgeAssignment> AssignOptimalWeighbridgeAsync(string transactionId, List<string> availableWeighbridges)
    {
        var capacities = new List<WeighbridgeCapacity>();
        
        foreach (var weighbridgeId in availableWeighbridges)
        {
            var capacity = await GetCurrentCapacityAsync(weighbridgeId);
            capacities.Add(capacity);
        }

        var mockTransaction = new PendingTransaction 
        { 
            Id = transactionId, 
            EstimatedDuration = TimeSpan.FromMinutes(15) 
        };

        var optimalWeighbridge = SelectOptimalWeighbridge(capacities, mockTransaction);
        
        if (optimalWeighbridge == null)
        {
            throw new InvalidOperationException("No suitable weighbridge found for assignment");
        }

        return new WeighbridgeAssignment
        {
            TransactionId = transactionId,
            WeighbridgeId = optimalWeighbridge.WeighbridgeId,
            EstimatedStartTime = CalculateEstimatedStartTime(optimalWeighbridge),
            EstimatedEndTime = CalculateEstimatedEndTime(optimalWeighbridge, mockTransaction.EstimatedDuration),
            Priority = AssignmentPriority.Normal,
            Reason = "Optimal capacity and wait time",
            Score = CalculateAssignmentScore(optimalWeighbridge, mockTransaction)
        };
    }

    public async Task<bool> ReserveCapacityAsync(string weighbridgeId, DateTime startTime, TimeSpan duration)
    {
        // This would typically involve creating a reservation record
        // For now, we'll implement a simple check
        var capacity = await GetCurrentCapacityAsync(weighbridgeId);
        
        if (capacity.UtilizationRate >= 1.0m)
        {
            return false; // At capacity, cannot reserve
        }

        // In a real implementation, you would create a reservation record
        return true;
    }

    public async Task<bool> ReleaseCapacityAsync(string weighbridgeId, DateTime startTime)
    {
        // This would typically involve removing a reservation record
        // For now, we'll return true as a placeholder
        return true;
    }

    public async Task<List<HourlyCapacityForecast>> GetDetailedForecastAsync(string weighbridgeId, DateTime date)
    {
        var forecast = await ForecastCapacityAsync(weighbridgeId, 24);
        
        return forecast.HourlyForecasts
            .Where(f => f.Hour.Date == date.Date)
            .ToList();
    }

    public async Task RecalculateCapacityMetricsAsync(string weighbridgeId)
    {
        var capacity = await _unitOfWork.Repository<CapacityManagement>()
            .FirstOrDefaultAsync(c => c.WeighbridgeId == weighbridgeId);

        if (capacity != null)
        {
            // Recalculate utilization rate
            if (capacity.MaxCapacity > 0)
            {
                capacity.UtilizationRate = (decimal)capacity.CurrentLoad / capacity.MaxCapacity;
            }

            // Recalculate estimated wait time
            capacity.EstimatedWaitTime = CalculateWaitTime(capacity);

            capacity.LastUpdated = DateTime.UtcNow;
            capacity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<CapacityManagement>().UpdateAsync(capacity);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    // Private helper methods
    private async Task<List<CapacityManagement>> GetHistoricalCapacityData(string weighbridgeId, DateTime startDate, DateTime endDate)
    {
        return (await _unitOfWork.Repository<CapacityManagement>()
            .FindAsync(c => c.WeighbridgeId == weighbridgeId && 
                           c.MeasurementDate >= startDate && 
                           c.MeasurementDate <= endDate))
            .OrderBy(c => c.MeasurementDate)
            .ToList();
    }

    private HourlyCapacityForecast GenerateHourlyForecast(WeighbridgeCapacity currentCapacity, List<CapacityManagement> historicalData, DateTime forecastTime)
    {
        // Simple forecasting algorithm - in reality, this would use machine learning
        var hourOfDay = forecastTime.Hour;
        var dayOfWeek = forecastTime.DayOfWeek;

        // Get historical data for same hour and day of week
        var similarPeriods = historicalData
            .Where(h => h.MeasurementDate.Hour == hourOfDay && h.MeasurementDate.DayOfWeek == dayOfWeek)
            .ToList();

        var forecast = new HourlyCapacityForecast
        {
            Hour = forecastTime,
            Confidence = similarPeriods.Count > 5 ? ForecastConfidence.High : ForecastConfidence.Medium
        };

        if (similarPeriods.Any())
        {
            forecast.PredictedLoad = (int)Math.Round(similarPeriods.Average(s => s.CurrentLoad));
            forecast.PredictedUtilization = similarPeriods.Average(s => s.UtilizationRate);
            forecast.PredictedWaitTime = TimeSpan.FromMinutes(similarPeriods.Average(s => s.EstimatedWaitTime.TotalMinutes));
        }
        else
        {
            // Use current values as baseline
            forecast.PredictedLoad = currentCapacity.CurrentLoad;
            forecast.PredictedUtilization = currentCapacity.UtilizationRate;
            forecast.PredictedWaitTime = currentCapacity.EstimatedWaitTime;
            forecast.Confidence = ForecastConfidence.Low;
        }

        forecast.Factors.Add($"Based on {similarPeriods.Count} similar historical periods");

        return forecast;
    }

    private CapacityTrend DetermineTrend(List<HourlyCapacityForecast> forecasts)
    {
        if (forecasts.Count < 2) return CapacityTrend.Stable;

        var utilisationTrend = forecasts.Last().PredictedUtilization - forecasts.First().PredictedUtilization;

        return utilisationTrend switch
        {
            > 0.1m => CapacityTrend.Increasing,
            < -0.1m => CapacityTrend.Decreasing,
            _ => CapacityTrend.Stable
        };
    }

    private ForecastConfidence CalculateForecastConfidence(List<CapacityManagement> historicalData, List<HourlyCapacityForecast> forecasts)
    {
        if (historicalData.Count < 10) return ForecastConfidence.Low;
        if (historicalData.Count < 50) return ForecastConfidence.Medium;
        
        return ForecastConfidence.High;
    }

    private async Task<List<PendingTransaction>> GetPendingTransactions(string organizationId)
    {
        // This would typically query a transactions service or database
        // For now, return a mock list
        return new List<PendingTransaction>
        {
            new() { Id = "tx1", EstimatedDuration = TimeSpan.FromMinutes(15), Priority = "Normal" },
            new() { Id = "tx2", EstimatedDuration = TimeSpan.FromMinutes(20), Priority = "High" },
            new() { Id = "tx3", EstimatedDuration = TimeSpan.FromMinutes(10), Priority = "Normal" }
        };
    }

    private WeighbridgeCapacity? SelectOptimalWeighbridge(List<WeighbridgeCapacity> weighbridges, PendingTransaction transaction)
    {
        return weighbridges
            .Where(w => w.UtilizationRate < 1.0m) // Not at full capacity
            .OrderBy(w => w.UtilizationRate) // Prefer less utilized weighbridges
            .ThenBy(w => w.EstimatedWaitTime) // Then by wait time
            .FirstOrDefault();
    }

    private DateTime CalculateEstimatedStartTime(WeighbridgeCapacity weighbridge)
    {
        return DateTime.UtcNow.Add(weighbridge.EstimatedWaitTime);
    }

    private DateTime CalculateEstimatedEndTime(WeighbridgeCapacity weighbridge, TimeSpan transactionDuration)
    {
        return CalculateEstimatedStartTime(weighbridge).Add(transactionDuration);
    }

    private AssignmentPriority DetermineAssignmentPriority(PendingTransaction transaction)
    {
        return transaction.Priority switch
        {
            "High" => AssignmentPriority.High,
            "Critical" => AssignmentPriority.Critical,
            "Emergency" => AssignmentPriority.Emergency,
            _ => AssignmentPriority.Normal
        };
    }

    private decimal CalculateAssignmentScore(WeighbridgeCapacity weighbridge, PendingTransaction transaction)
    {
        decimal score = 100;
        
        // Penalize for high utilization
        score -= weighbridge.UtilizationRate * 50;
        
        // Penalize for long wait times
        score -= (decimal)weighbridge.EstimatedWaitTime.TotalMinutes;
        
        return Math.Max(0, score);
    }

    private decimal CalculateImprovementScore(List<WeighbridgeCapacity> weighbridges, List<WeighbridgeAssignment> assignments)
    {
        // Calculate how much the load balancing improves overall utilization
        var totalCapacity = weighbridges.Sum(w => w.MaxCapacity);
        var totalLoad = weighbridges.Sum(w => w.CurrentLoad) + assignments.Count;
        var overallUtilization = (decimal)totalLoad / totalCapacity;
        
        // Score based on balanced utilization (penalize extremes)
        var utilizationVariance = weighbridges.Select(w => w.UtilizationRate).Variance();
        return Math.Max(0, 100 - (utilizationVariance * 100));
    }

    private string GenerateLoadBalancingRecommendations(List<WeighbridgeCapacity> weighbridges, LoadBalancingResult result)
    {
        var recommendations = new List<string>();
        
        var overUtilizedCount = weighbridges.Count(w => w.UtilizationRate > 0.8m);
        var underUtilizedCount = weighbridges.Count(w => w.UtilizationRate < 0.3m);
        
        if (overUtilizedCount > 0)
        {
            recommendations.Add($"{overUtilizedCount} weighbridge(s) are over-utilized. Consider redistributing load.");
        }
        
        if (underUtilizedCount > 0)
        {
            recommendations.Add($"{underUtilizedCount} weighbridge(s) are under-utilized. Route more traffic to these locations.");
        }
        
        return string.Join(" ", recommendations);
    }

    private List<LoadBalancingMetricDto> CalculateLoadBalancingMetrics(List<WeighbridgeCapacity> weighbridges, List<WeighbridgeAssignment> assignments)
    {
        var metrics = new List<LoadBalancingMetricDto>
        {
            new()
            {
                MetricName = "Average Utilization",
                Value = weighbridges.Average(w => w.UtilizationRate),
                MeasuredAt = DateTime.UtcNow,
                Unit = "Percentage",
                Type = DTOs.MetricType.Utilization
            },
            new()
            {
                MetricName = "Total Assignments",
                Value = assignments.Count,
                MeasuredAt = DateTime.UtcNow,
                Unit = "Count",
                Type = DTOs.MetricType.Throughput
            },
            new()
            {
                MetricName = "Average Wait Time",
                Value = (decimal)weighbridges.Average(w => w.EstimatedWaitTime.TotalMinutes),
                MeasuredAt = DateTime.UtcNow,
                Unit = "Minutes",
                Type = DTOs.MetricType.WaitTime
            }
        };

        return metrics;
    }

    private List<CapacityPattern> AnalyzeCapacityPatterns(List<CapacityManagement> capacityData)
    {
        var patterns = new List<CapacityPattern>();
        
        // Analyze hourly patterns
        var hourlyPattern = capacityData
            .GroupBy(c => c.MeasurementDate.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new { Hour = g.Key, AvgUtilization = g.Average(c => c.UtilizationRate) })
            .ToList();
        
        var peakHours = hourlyPattern
            .Where(h => h.AvgUtilization > 0.7m)
            .Select(h => TimeSpan.FromHours(h.Hour))
            .ToList();
        
        if (peakHours.Any())
        {
            patterns.Add(new CapacityPattern
            {
                Name = "Peak Hours",
                Description = "Hours with consistently high utilization",
                Type = PatternType.Hourly,
                RecurringTimes = peakHours,
                AverageImpact = peakHours.Count > 0 ? hourlyPattern.Where(h => peakHours.Contains(TimeSpan.FromHours(h.Hour))).Average(h => h.AvgUtilization) : 0,
                Confidence = 0.8m
            });
        }
        
        return patterns;
    }

    private List<CapacityBottleneck> IdentifyBottlenecks(List<CapacityManagement> capacityData)
    {
        var bottlenecks = new List<CapacityBottleneck>();
        
        var highUtilizationPeriods = capacityData.Where(c => c.UtilizationRate > 0.9m).ToList();
        
        if (highUtilizationPeriods.Any())
        {
            bottlenecks.Add(new CapacityBottleneck
            {
                Location = "Weighbridge Operations",
                Description = "Frequent high utilization periods detected",
                Type = BottleneckType.Capacity,
                ImpactScore = highUtilizationPeriods.Average(h => h.UtilizationRate) * 100,
                FirstObserved = highUtilizationPeriods.Min(h => h.MeasurementDate),
                LastObserved = highUtilizationPeriods.Max(h => h.MeasurementDate),
                Frequency = highUtilizationPeriods.Count,
                Causes = new List<string> { "High demand", "Limited capacity", "Inefficient processing" },
                PotentialSolutions = new List<string> { "Increase capacity", "Optimize scheduling", "Improve efficiency" }
            });
        }
        
        return bottlenecks;
    }

    private List<CapacityRecommendationDto> GenerateCapacityRecommendations(CapacityAnalytics analytics)
    {
        var recommendations = new List<CapacityRecommendationDto>();
        
        if (analytics.AverageUtilization > 0.8m)
        {
            recommendations.Add(new CapacityRecommendationDto
            {
                Title = "High Utilization Alert",
                Description = "Average utilization is above 80%. Consider capacity expansion or load balancing.",
                Type = RecommendationType.Capacity,
                Priority = RecommendationPriority.High,
                EstimatedImpact = 0.2m,
                Benefits = new List<string> { "Reduced wait times", "Improved customer satisfaction", "Higher throughput" },
                Risks = new List<string> { "Investment required", "Implementation time" }
            });
        }
        
        if (analytics.Bottlenecks.Any())
        {
            recommendations.Add(new CapacityRecommendationDto
            {
                Title = "Address Bottlenecks",
                Description = $"Identified {analytics.Bottlenecks.Count} bottleneck(s) that need attention.",
                Type = RecommendationType.Process,
                Priority = RecommendationPriority.High,
                EstimatedImpact = 0.15m,
                Benefits = new List<string> { "Eliminate constraints", "Improve flow", "Reduce delays" },
                Risks = new List<string> { "Process changes required", "Training needed" }
            });
        }
        
        return recommendations;
    }

    private List<string> DetermineBottleneckCauses(WeighbridgeCapacity capacity)
    {
        var causes = new List<string>();
        
        if (capacity.UtilizationRate > 0.9m)
            causes.Add("High demand exceeding capacity");
        
        if (capacity.EstimatedWaitTime.TotalMinutes > 30)
            causes.Add("Long processing times");
        
        if (capacity.VehiclesInQueue > 10)
            causes.Add("Queue management issues");
        
        return causes;
    }

    private List<string> GenerateBottleneckSolutions(WeighbridgeCapacity capacity)
    {
        var solutions = new List<string>();
        
        solutions.Add("Optimize scheduling to spread demand");
        solutions.Add("Implement express lanes for certain vehicle types");
        solutions.Add("Improve operational efficiency");
        solutions.Add("Consider additional weighbridge capacity");
        
        return solutions;
    }

    private TimeSpan CalculateWaitTime(CapacityManagement capacity)
    {
        // Simple calculation based on queue length and processing rate
        var avgProcessingTimeMinutes = capacity.AverageProcessingTime.TotalMinutes;
        if (avgProcessingTimeMinutes <= 0) avgProcessingTimeMinutes = 15; // Default 15 minutes
        
        var waitTimeMinutes = capacity.VehiclesInQueue * avgProcessingTimeMinutes;
        return TimeSpan.FromMinutes(waitTimeMinutes);
    }

    // Helper class for pending transactions
    private class PendingTransaction
    {
        public string Id { get; set; } = string.Empty;
        public TimeSpan EstimatedDuration { get; set; }
        public string Priority { get; set; } = "Normal";
    }
}

// Extension method for variance calculation
public static class EnumerableExtensions
{
    public static decimal Variance(this IEnumerable<decimal> values)
    {
        var valueList = values.ToList();
        if (valueList.Count <= 1) return 0;
        
        var mean = valueList.Average();
        var sumOfSquaredDifferences = valueList.Sum(val => (val - mean) * (val - mean));
        return sumOfSquaredDifferences / (valueList.Count - 1);
    }
}