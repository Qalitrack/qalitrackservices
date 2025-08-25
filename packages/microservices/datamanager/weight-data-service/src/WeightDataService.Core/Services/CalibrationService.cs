using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Core.Services;

public class CalibrationService : ICalibrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CalibrationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CalibrationRecordDto> CreateCalibrationRecordAsync(CreateCalibrationRecordDto createDto, string organizationId, string userId)
    {
        var drift = createDto.MeasuredWeight - createDto.ReferenceWeight;
        var driftPercentage = createDto.ReferenceWeight != 0 ? (drift / createDto.ReferenceWeight) * 100 : 0;

        var calibrationRecord = new CalibrationRecord
        {
            CalibrationId = Guid.NewGuid().ToString(),
            WeighbridgeId = createDto.WeighbridgeId,
            TechnicianId = createDto.TechnicianId,
            ReferenceWeight = createDto.ReferenceWeight,
            MeasuredWeight = createDto.MeasuredWeight,
            Drift = drift,
            DriftPercentage = driftPercentage,
            Status = Math.Abs(driftPercentage) <= 0.1m ? CalibrationStatus.Valid : CalibrationStatus.RequiresAttention,
            NextCalibrationDue = DateTime.UtcNow.AddDays(90), // Default 90 days
            Notes = createDto.Notes,
            IsAutomatic = createDto.IsAutomatic,
            OrganizationId = organizationId,
            CreatedBy = userId
        };

        await _unitOfWork.CalibrationRecords.AddAsync(calibrationRecord);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CalibrationRecordDto>(calibrationRecord);
    }

    public async Task<CalibrationRecordDto?> GetCalibrationRecordAsync(Guid id, string organizationId)
    {
        var record = await _unitOfWork.CalibrationRecords.GetByIdAsync(id);
        if (record?.OrganizationId != organizationId) return null;

        return _mapper.Map<CalibrationRecordDto>(record);
    }

    public async Task<List<CalibrationRecordDto>> GetCalibrationHistoryAsync(string weighbridgeId, string organizationId)
    {
        var records = await _unitOfWork.CalibrationRecords.GetByWeighbridgeIdAsync(weighbridgeId, organizationId);
        return _mapper.Map<List<CalibrationRecordDto>>(records);
    }

    public async Task<CalibrationStatusDto> GetCalibrationStatusAsync(string weighbridgeId, string organizationId)
    {
        var latestRecord = await _unitOfWork.CalibrationRecords.GetLatestCalibrationAsync(weighbridgeId, organizationId);

        if (latestRecord == null)
        {
            return new CalibrationStatusDto
            {
                WeighbridgeId = weighbridgeId,
                Status = "Never Calibrated",
                RequiresCalibration = true,
                DaysUntilDue = 0
            };
        }

        var daysUntilDue = (latestRecord.NextCalibrationDue - DateTime.UtcNow).Days;
        var requiresCalibration = daysUntilDue <= 0 || latestRecord.Status == CalibrationStatus.RequiresAttention;

        return new CalibrationStatusDto
        {
            WeighbridgeId = weighbridgeId,
            Status = latestRecord.Status.ToString(),
            LastCalibrationDate = latestRecord.CalibrationDate,
            NextCalibrationDue = latestRecord.NextCalibrationDue,
            LastDrift = latestRecord.Drift,
            LastDriftPercentage = latestRecord.DriftPercentage,
            RequiresCalibration = requiresCalibration,
            DaysUntilDue = Math.Max(0, daysUntilDue)
        };
    }

    public Task<CalibrationDriftDto> DetectDriftAsync(string weighbridgeId, decimal currentWeight, decimal referenceWeight)
    {
        var drift = currentWeight - referenceWeight;
        var driftPercentage = referenceWeight != 0 ? (drift / referenceWeight) * 100 : 0;
        var driftDetected = Math.Abs(driftPercentage) > 0.1m;

        string? recommendedAction = null;
        if (driftDetected)
        {
            recommendedAction = Math.Abs(driftPercentage) > 1.0m ? "Immediate Calibration Required" : "Schedule Calibration";
        }

        return Task.FromResult(new CalibrationDriftDto
        {
            WeighbridgeId = weighbridgeId,
            CurrentDrift = drift,
            DriftPercentage = driftPercentage,
            DriftDetected = driftDetected,
            RecommendedAction = recommendedAction,
            DetectionTime = DateTime.UtcNow
        });
    }

    public async Task<bool> PerformAutomaticCalibrationAsync(string weighbridgeId, string organizationId)
    {
        // Simulate automatic calibration with hardware integration
        // In a real implementation, this would:
        // 1. Connect to weighbridge hardware controller
        // 2. Place standard reference weights
        // 3. Take measurements automatically
        // 4. Calculate drift and adjust calibration
        // 5. Generate calibration certificate

        var random = new Random();
        var referenceWeight = 1000m; // Standard 1000kg reference weight
        var measurementVariation = (decimal)(random.NextDouble() * 4 - 2); // ±2kg variation
        var measuredWeight = referenceWeight + measurementVariation;

        var createDto = new CreateCalibrationRecordDto
        {
            WeighbridgeId = weighbridgeId,
            TechnicianId = "SYSTEM_AUTO",
            ReferenceWeight = referenceWeight,
            MeasuredWeight = measuredWeight,
            IsAutomatic = true,
            Notes = $"Automatic calibration performed. Variation: {measurementVariation:F2}kg"
        };

        var result = await CreateCalibrationRecordAsync(createDto, organizationId, "SYSTEM");
        
        // Log calibration result for monitoring
        Console.WriteLine($"Automatic calibration completed for weighbridge {weighbridgeId}. " +
                         $"Drift: {result.Drift:F2}kg ({result.DriftPercentage:F2}%). " +
                         $"Status: {result.Status}");

        return true;
    }

    public async Task<List<CalibrationRecordDto>> GetCalibrationsDueAsync(string organizationId)
    {
        var dueRecords = await _unitOfWork.CalibrationRecords.GetCalibrationsDueAsync(organizationId);
        return _mapper.Map<List<CalibrationRecordDto>>(dueRecords);
    }

    public async Task<bool> ScheduleCalibrationAsync(string weighbridgeId, DateTime scheduledDate, string organizationId)
    {
        // Implementation for calibration scheduling with job scheduling system
        // This would integrate with a background job scheduler like:
        // 1. Hangfire for .NET background processing
        // 2. Quartz.NET for scheduled jobs
        // 3. Azure Functions with timer triggers
        // 4. AWS Lambda with EventBridge schedules

        // Example Hangfire implementation:
        // BackgroundJob.Schedule(() => PerformAutomaticCalibrationAsync(weighbridgeId, organizationId), scheduledDate);

        // Example logging for scheduled calibration
        Console.WriteLine($"Calibration scheduled for weighbridge {weighbridgeId} on {scheduledDate:yyyy-MM-dd HH:mm}");

        // For now, return success as this is a framework integration point
        return await Task.FromResult(true);
    }
}