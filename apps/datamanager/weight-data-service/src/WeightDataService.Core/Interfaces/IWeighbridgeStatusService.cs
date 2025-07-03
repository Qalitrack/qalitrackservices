using WeightDataService.Core.DTOs;

namespace WeightDataService.Core.Interfaces;

public interface IWeighbridgeStatusService
{
    Task<List<WeighbridgeStatusDto>> GetWeighbridgesAsync(string organizationId);
    Task<WeighbridgeStatusDto?> GetWeighbridgeByIdAsync(string weighbridgeId, string organizationId);
    Task<WeighbridgeStatusDto> CreateWeighbridgeAsync(CreateWeighbridgeStatusDto createDto, string organizationId, string userId);
    Task<WeighbridgeStatusDto?> UpdateWeighbridgeStatusAsync(string weighbridgeId, UpdateWeighbridgeStatusDto updateDto, string organizationId, string userId);
    Task<List<WeighbridgeStatusDto>> GetActiveWeighbridgesAsync(string organizationId);
    Task<List<WeighbridgeStatusDto>> GetWeighbridgesRequiringCalibrationAsync(string organizationId);
    Task<bool> IsWeighbridgeAvailableAsync(string weighbridgeId, string organizationId);
    Task<WeighbridgeStatusDto?> UpdateCurrentWeightAsync(string weighbridgeId, decimal currentWeight, string organizationId);
}