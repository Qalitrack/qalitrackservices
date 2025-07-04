using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IWeighbridgeStatusRepository : IRepository<WeighbridgeStatus>
{
    Task<WeighbridgeStatus?> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeStatus>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<WeighbridgeStatus>> GetActiveWeighbridgesAsync(string organizationId);
    Task<IEnumerable<WeighbridgeStatus>> GetByStatusAsync(MaintenanceStatus status, string organizationId);
    Task<IEnumerable<WeighbridgeStatus>> GetRequiringCalibrationAsync(string organizationId);
}