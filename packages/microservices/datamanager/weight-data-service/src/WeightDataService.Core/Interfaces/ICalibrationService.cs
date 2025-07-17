using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface ICalibrationService
{
    Task<CalibrationRecordDto> CreateCalibrationRecordAsync(CreateCalibrationRecordDto createDto, string organizationId, string userId);
    Task<CalibrationRecordDto?> GetCalibrationRecordAsync(Guid id, string organizationId);
    Task<List<CalibrationRecordDto>> GetCalibrationHistoryAsync(string weighbridgeId, string organizationId);
    Task<CalibrationStatusDto> GetCalibrationStatusAsync(string weighbridgeId, string organizationId);
    Task<CalibrationDriftDto> DetectDriftAsync(string weighbridgeId, decimal currentWeight, decimal referenceWeight);
    Task<bool> PerformAutomaticCalibrationAsync(string weighbridgeId, string organizationId);
    Task<List<CalibrationRecordDto>> GetCalibrationsDueAsync(string organizationId);
    Task<bool> ScheduleCalibrationAsync(string weighbridgeId, DateTime scheduledDate, string organizationId);
}