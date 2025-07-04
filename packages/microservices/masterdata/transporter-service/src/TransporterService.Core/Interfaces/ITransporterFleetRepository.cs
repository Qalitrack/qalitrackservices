using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterFleetRepository : IRepository<TransporterFleet>
{
    Task<IEnumerable<TransporterFleet>> GetFleetByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterFleet>> GetAvailableVehiclesAsync(string transporterId);
    Task<IEnumerable<TransporterFleet>> GetVehiclesByStatusAsync(string transporterId, VehicleStatus status);
    Task<IEnumerable<TransporterFleet>> GetVehiclesByTypeAsync(string transporterId, VehicleType type);
    Task<TransporterFleet?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<IEnumerable<TransporterFleet>> GetVehiclesDueForMaintenanceAsync(string transporterId);
    Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, string? excludeId = null);
}