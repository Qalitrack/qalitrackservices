using WeighbridgeService.Core.DTOs;
using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeService
{
    // Weighbridge Management
    Task<WeighbridgeDto> RegisterWeighbridgeAsync(RegisterWeighbridgeRequest request);
    Task<WeighbridgeDto?> GetWeighbridgeByIdAsync(string id);
    Task<WeighbridgeDto?> GetWeighbridgeByCodeAsync(string code);
    Task<IEnumerable<WeighbridgeDto>> GetAllWeighbridgesAsync();
    Task<IEnumerable<WeighbridgeDto>> GetActiveWeighbridgesAsync();
    Task<IEnumerable<WeighbridgeDto>> GetWeighbridgesByStatusAsync(WeighbridgeStatus status);
    Task<WeighbridgeDto> UpdateWeighbridgeAsync(string id, UpdateWeighbridgeRequest request);
    Task DeleteWeighbridgeAsync(string id);
    
    // Configuration Management
    Task<WeighbridgeConfigurationDto?> GetConfigurationAsync(string weighbridgeId);
    Task<WeighbridgeConfigurationDto> UpdateConfigurationAsync(string weighbridgeId, UpdateWeighbridgeConfigurationRequest request);
    
    // Calibration Management
    Task<IEnumerable<WeighbridgeCalibrationDto>> GetCalibrationHistoryAsync(string weighbridgeId);
    Task<WeighbridgeCalibrationDto> ScheduleCalibrationAsync(string weighbridgeId, ScheduleCalibrationRequest request);
    Task<WeighbridgeCalibrationDto> RecordCalibrationAsync(string calibrationId, RecordCalibrationRequest request);
    Task<IEnumerable<WeighbridgeCalibrationDto>> GetScheduledCalibrationsAsync();
    Task<IEnumerable<WeighbridgeCalibrationDto>> GetOverdueCalibrationsAsync();
    
    // Maintenance Management
    Task<IEnumerable<WeighbridgeMaintenanceDto>> GetMaintenanceScheduleAsync(string weighbridgeId);
    Task<WeighbridgeMaintenanceDto> ScheduleMaintenanceAsync(string weighbridgeId, ScheduleMaintenanceRequest request);
    Task<WeighbridgeMaintenanceDto> UpdateMaintenanceAsync(string maintenanceId, UpdateMaintenanceRequest request);
    Task<IEnumerable<WeighbridgeMaintenanceDto>> GetScheduledMaintenanceAsync();
    Task<IEnumerable<WeighbridgeMaintenanceDto>> GetOverdueMaintenanceAsync();
    
    // Operator Management
    Task<IEnumerable<WeighbridgeOperatorDto>> GetAssignedOperatorsAsync(string weighbridgeId);
    Task<WeighbridgeOperatorDto> AssignOperatorAsync(string weighbridgeId, AssignOperatorRequest request);
    Task UnassignOperatorAsync(string weighbridgeId, string operatorId);
    Task<IEnumerable<WeighbridgeOperatorDto>> GetOperatorsNeedingTrainingAsync();
    
    // Capacity Management
    Task<WeighbridgeCapacityDto?> GetCurrentCapacityAsync(string weighbridgeId);
    Task<WeighbridgeCapacityDto> UpdateCapacityAsync(string weighbridgeId, UpdateCapacityRequest request);
    Task<IEnumerable<WeighbridgeDto>> GetAvailableWeighbridgesAsync(DateTime requestedTime);
    
    // Location Management
    Task<WeighbridgeLocationDto?> GetLocationAsync(string weighbridgeId);
    Task<WeighbridgeLocationDto> UpdateLocationAsync(string weighbridgeId, CreateWeighbridgeLocationRequest request);
    
    // Hardware Integration
    Task<WeighbridgeHardwareStatusDto> GetHardwareStatusAsync(string weighbridgeId);
    Task<WeighbridgeControlResultDto> ExecuteHardwareControlAsync(string weighbridgeId, WeighbridgeControlCommandDto command);
    Task<WeighbridgeTestResultDto> TestHardwareConnectionAsync(string weighbridgeId);
    Task<WeighbridgeUpdateResultDto> PerformRemoteUpdateAsync(string weighbridgeId, WeighbridgeRemoteUpdateDto update);
}