using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IWeightMeasurementService
{
    Task<PagedResultDto<WeightMeasurementDto>> GetMeasurementsAsync(string organizationId, SearchFilterDto filter);
    Task<WeightMeasurementDto?> GetMeasurementByIdAsync(Guid id, string organizationId);
    Task<WeightMeasurementDto?> GetMeasurementByTicketAsync(string ticketReference, string organizationId);
    Task<WeightMeasurementDto> CreateMeasurementAsync(CreateWeightMeasurementDto createDto, string organizationId, string userId);
    Task<WeightMeasurementDto?> UpdateMeasurementAsync(Guid id, UpdateWeightMeasurementDto updateDto, string organizationId, string userId);
    Task<bool> DeleteMeasurementAsync(Guid id, string organizationId, string userId);
    Task<List<WeightMeasurementDto>> GetPendingMeasurementsAsync(string organizationId);
    Task<List<WeightMeasurementDto>> GetMeasurementsByVehicleAsync(string vehicleRegistration, string organizationId);
    Task<List<WeightMeasurementDto>> GetMeasurementsByWeighbridgeAsync(string weighbridgeId, DateTime? fromDate, DateTime? toDate);
    Task<bool> ValidateWeightAsync(decimal weight, string weighbridgeId);
}