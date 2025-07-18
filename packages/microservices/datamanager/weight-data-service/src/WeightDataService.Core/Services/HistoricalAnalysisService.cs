using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class HistoricalAnalysisService : IHistoricalAnalysisService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HistoricalAnalysisService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<HistoricalAnalysisDto> CreateAnalysisAsync(CreateHistoricalAnalysisDto createDto, string organizationId, string userId)
    {
        // Get measurements from the period
        var measurements = await _unitOfWork.WeightMeasurements.FindAsync(
            m => m.WeighbridgeId == createDto.WeighbridgeId &&
                 m.MeasurementDateTime >= createDto.PeriodStart &&
                 m.MeasurementDateTime <= createDto.PeriodEnd &&
                 m.OrganizationId == organizationId &&
                 (string.IsNullOrEmpty(createDto.VehicleRegistration) || m.VehicleRegistration == createDto.VehicleRegistration) &&
                 (string.IsNullOrEmpty(createDto.ProductType) || m.ProductType == createDto.ProductType) &&
                 !m.IsDeleted);

        var weights = measurements.Select(m => m.Weight).ToList();
        if (!weights.Any())
        {
            throw new InvalidOperationException("No measurements found for the specified criteria");
        }

        var totalWeight = weights.Sum();
        var averageWeight = weights.Average();
        var minWeight = weights.Min();
        var maxWeight = weights.Max();
        var variance = weights.Select(w => Math.Pow((double)(w - averageWeight), 2)).Average();
        var standardDeviation = (decimal)Math.Sqrt(variance);

        // Calculate trend
        var trendData = CalculateTrend(measurements.OrderBy(m => m.MeasurementDateTime).ToList());

        var analysis = new HistoricalAnalysis
        {
            AnalysisId = Guid.NewGuid().ToString(),
            WeighbridgeId = createDto.WeighbridgeId,
            VehicleRegistration = createDto.VehicleRegistration,
            ProductType = createDto.ProductType,
            PeriodStart = createDto.PeriodStart,
            PeriodEnd = createDto.PeriodEnd,
            Type = Enum.Parse<AnalysisType>(createDto.Type),
            TotalWeight = totalWeight,
            AverageWeight = averageWeight,
            MinWeight = minWeight,
            MaxWeight = maxWeight,
            StandardDeviation = standardDeviation,
            TrendSlope = trendData.slope,
            TrendR2 = trendData.r2,
            MeasurementCount = weights.Count(),
            TrendCategory = GetTrendCategory(trendData.slope, trendData.r2),
            OrganizationId = organizationId,
            CreatedBy = userId
        };

        await _unitOfWork.HistoricalAnalyses.AddAsync(analysis);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<HistoricalAnalysisDto>(analysis);
    }

    public async Task<HistoricalAnalysisDto?> GetAnalysisAsync(Guid id, string organizationId)
    {
        var analysis = await _unitOfWork.HistoricalAnalyses.GetByIdAsync(id);
        if (analysis?.OrganizationId != organizationId) return null;

        return _mapper.Map<HistoricalAnalysisDto>(analysis);
    }

    public async Task<List<HistoricalAnalysisDto>> GetAnalysesAsync(string organizationId, string? weighbridgeId = null, string? vehicleRegistration = null)
    {
        var analyses = await _unitOfWork.HistoricalAnalyses.FindAsync(
            a => a.OrganizationId == organizationId &&
                 (string.IsNullOrEmpty(weighbridgeId) || a.WeighbridgeId == weighbridgeId) &&
                 (string.IsNullOrEmpty(vehicleRegistration) || a.VehicleRegistration == vehicleRegistration));

        return _mapper.Map<List<HistoricalAnalysisDto>>(analyses);
    }

    public async Task<TrendAnalysisDto> GetTrendAnalysisAsync(string weighbridgeId, DateTime fromDate, DateTime toDate, string? vehicleRegistration = null, string? productType = null)
    {
        var measurements = await _unitOfWork.WeightMeasurements.FindAsync(
            m => m.WeighbridgeId == weighbridgeId &&
                 m.MeasurementDateTime >= fromDate &&
                 m.MeasurementDateTime <= toDate &&
                 (string.IsNullOrEmpty(vehicleRegistration) || m.VehicleRegistration == vehicleRegistration) &&
                 (string.IsNullOrEmpty(productType) || m.ProductType == productType) &&
                 !m.IsDeleted);

        var trendData = CalculateTrend(measurements.OrderBy(m => m.MeasurementDateTime).ToList());
        var anomalies = await DetectAnomaliesAsync(weighbridgeId, fromDate, toDate);

        return new TrendAnalysisDto
        {
            WeighbridgeId = weighbridgeId,
            VehicleRegistration = vehicleRegistration,
            ProductType = productType,
            PeriodStart = fromDate,
            PeriodEnd = toDate,
            TrendSlope = trendData.slope,
            TrendR2 = trendData.r2,
            TrendDirection = GetTrendDirection(trendData.slope),
            TrendStrength = GetTrendStrength(trendData.r2),
            Anomalies = anomalies
        };
    }

    public async Task<List<AnomalyDto>> DetectAnomaliesAsync(string weighbridgeId, DateTime fromDate, DateTime toDate, decimal threshold = 2.0m)
    {
        var measurements = await _unitOfWork.WeightMeasurements.FindAsync(
            m => m.WeighbridgeId == weighbridgeId &&
                 m.MeasurementDateTime >= fromDate &&
                 m.MeasurementDateTime <= toDate &&
                 !m.IsDeleted);

        var measurementsList = measurements.OrderBy(m => m.MeasurementDateTime).ToList();
        if (measurementsList.Count < 3) return new List<AnomalyDto>();

        var weights = measurements.Select(m => m.Weight).ToList();
        var mean = weights.Average();
        var variance = weights.Select(w => Math.Pow((double)(w - mean), 2)).Average();
        var standardDeviation = (decimal)Math.Sqrt(variance);

        var anomalies = new List<AnomalyDto>();

        foreach (var measurement in measurementsList)
        {
            var deviation = Math.Abs(measurement.Weight - mean);
            var zScore = standardDeviation > 0 ? deviation / standardDeviation : 0;

            if (zScore > threshold)
            {
                anomalies.Add(new AnomalyDto
                {
                    Timestamp = measurement.MeasurementDateTime,
                    Weight = measurement.Weight,
                    ExpectedWeight = mean,
                    Deviation = deviation,
                    AnomalyType = measurement.Weight > mean ? "High" : "Low",
                    Severity = zScore > 3 ? "Critical" : "Warning"
                });
            }
        }

        return anomalies;
    }

    public async Task<bool> ScheduleAutomaticAnalysisAsync(string weighbridgeId, AnalysisType type, string organizationId)
    {
        // Implementation for scheduling automatic analysis
        // This would integrate with a background job scheduler like:
        // 1. Hangfire for .NET background processing
        // 2. Quartz.NET for scheduled jobs
        // 3. Azure Functions with timer triggers
        // 4. AWS Lambda with EventBridge schedules

        // Example Hangfire implementation:
        // var jobId = BackgroundJob.Schedule(() => RunAutomaticAnalysisAsync(weighbridgeId, type, organizationId), 
        //                                   GetNextScheduleTime(type));

        // Example Quartz.NET implementation:
        // var job = JobBuilder.Create<AnalysisJob>()
        //     .WithIdentity($"analysis-{weighbridgeId}-{type}", "analysis")
        //     .UsingJobData("weighbridgeId", weighbridgeId)
        //     .UsingJobData("type", type.ToString())
        //     .UsingJobData("organizationId", organizationId)
        //     .Build();

        Console.WriteLine($"Automatic analysis scheduled for weighbridge {weighbridgeId}, type {type}");
        return await Task.FromResult(true);
    }

    public async Task RunScheduledAnalysesAsync()
    {
        // Implementation for running scheduled analyses
        // This would be called by a background service or scheduled job
        
        // Get all weighbridges that need analysis
        var weighbridges = await _unitOfWork.WeighbridgeStatuses.FindAsync(w => w.Status == MaintenanceStatus.Active);
        
        foreach (var weighbridge in weighbridges)
        {
            // Check if daily analysis is needed
            var lastDaily = await _unitOfWork.HistoricalAnalyses.GetLatestAnalysisAsync(
                weighbridge.WeighbridgeId, AnalysisType.Daily, weighbridge.OrganizationId);
            
            if (lastDaily == null || lastDaily.AnalysisDate.Date < DateTime.UtcNow.Date)
            {
                var createDto = new CreateHistoricalAnalysisDto
                {
                    WeighbridgeId = weighbridge.WeighbridgeId,
                    PeriodStart = DateTime.UtcNow.Date.AddDays(-1),
                    PeriodEnd = DateTime.UtcNow.Date,
                    Type = AnalysisType.Daily.ToString()
                };

                try
                {
                    await CreateAnalysisAsync(createDto, weighbridge.OrganizationId, "SYSTEM");
                    Console.WriteLine($"Daily analysis completed for weighbridge {weighbridge.WeighbridgeId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create daily analysis for weighbridge {weighbridge.WeighbridgeId}: {ex.Message}");
                }
            }
        }
    }

    public async Task<Dictionary<string, decimal>> GetWeightStatisticsAsync(string weighbridgeId, DateTime fromDate, DateTime toDate, string organizationId)
    {
        var measurements = await _unitOfWork.WeightMeasurements.FindAsync(
            m => m.WeighbridgeId == weighbridgeId &&
                 m.MeasurementDateTime >= fromDate &&
                 m.MeasurementDateTime <= toDate &&
                 m.OrganizationId == organizationId &&
                 !m.IsDeleted);

        var weights = measurements.Select(m => m.Weight).ToList();
        if (!weights.Any()) return new Dictionary<string, decimal>();

        var mean = weights.Average();
        var variance = weights.Select(w => Math.Pow((double)(w - mean), 2)).Average();

        return new Dictionary<string, decimal>
        {
            ["Total"] = weights.Sum(),
            ["Average"] = mean,
            ["Minimum"] = weights.Min(),
            ["Maximum"] = weights.Max(),
            ["StandardDeviation"] = (decimal)Math.Sqrt(variance),
            ["Count"] = weights.Count(),
            ["Range"] = weights.Max() - weights.Min(),
            ["Median"] = CalculateMedian(weights)
        };
    }

    private decimal CalculateMedian(List<decimal> weights)
    {
        var sorted = weights.OrderBy(w => w).ToList();
        var count = sorted.Count;
        
        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private (decimal slope, decimal r2) CalculateTrend(List<WeightMeasurement> measurements)
    {
        if (measurements.Count < 2) return (0, 0);

        var n = measurements.Count;
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXY = 0.0;
        var sumX2 = 0.0;
        var sumY2 = 0.0;

        for (int i = 0; i < n; i++)
        {
            var x = i; // Use index as X (time series)
            var y = (double)measurements[i].Weight;

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
            sumY2 += y * y;
        }

        var denominator = n * sumX2 - sumX * sumX;
        if (Math.Abs(denominator) < 1e-10) return (0, 0); // Avoid division by zero

        var slope = (n * sumXY - sumX * sumY) / denominator;
        
        var numerator = n * sumXY - sumX * sumY;
        var denominatorR = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
        
        var correlation = denominatorR != 0 ? numerator / denominatorR : 0;
        var r2 = correlation * correlation;

        return ((decimal)slope, (decimal)r2);
    }

    private string GetTrendCategory(decimal slope, decimal r2)
    {
        if (r2 < 0.5m) return "No Trend";
        if (slope > 0.1m) return "Increasing";
        if (slope < -0.1m) return "Decreasing";
        return "Stable";
    }

    private string GetTrendDirection(decimal slope)
    {
        if (slope > 0.1m) return "Increasing";
        if (slope < -0.1m) return "Decreasing";
        return "Stable";
    }

    private string GetTrendStrength(decimal r2)
    {
        if (r2 >= 0.8m) return "Strong";
        if (r2 >= 0.5m) return "Moderate";
        if (r2 >= 0.3m) return "Weak";
        return "None";
    }
}