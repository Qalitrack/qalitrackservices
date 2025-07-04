using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Interfaces;

public interface IWeighbridgeMaintenanceRepository : IRepository<WeighbridgeMaintenance>
{
    Task<IEnumerable<WeighbridgeMaintenance>> GetByWeighbridgeIdAsync(string weighbridgeId);
    Task<IEnumerable<WeighbridgeMaintenance>> GetByStatusAsync(MaintenanceStatus status);
    Task<IEnumerable<WeighbridgeMaintenance>> GetByTypeAsync(MaintenanceType type);
    Task<IEnumerable<WeighbridgeMaintenance>> GetByPriorityAsync(MaintenancePriority priority);
    Task<IEnumerable<WeighbridgeMaintenance>> GetScheduledMaintenanceAsync();
    Task<IEnumerable<WeighbridgeMaintenance>> GetOverdueMaintenanceAsync();
    Task<IEnumerable<WeighbridgeMaintenance>> GetUpcomingMaintenanceAsync(int days);
}