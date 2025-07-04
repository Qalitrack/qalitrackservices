using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Interfaces;

public interface IWeightMeasurementRepository : IRepository<WeightMeasurement>
{
    Task<IEnumerable<WeightMeasurement>> GetByOrganizationAsync(string organizationId, int skip = 0, int take = 50);
    Task<IEnumerable<WeightMeasurement>> GetByWeighbridgeAsync(string weighbridgeId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<WeightMeasurement>> GetByVehicleRegistrationAsync(string vehicleRegistration, string organizationId);
    Task<IEnumerable<WeightMeasurement>> GetPendingMeasurementsAsync(string organizationId);
    Task<WeightMeasurement?> GetByTicketReferenceAsync(string ticketReference, string organizationId);
    Task<IEnumerable<WeightMeasurement>> SearchAsync(string organizationId, string? searchTerm = null, 
        MeasurementStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, 
        int skip = 0, int take = 50);
    Task<int> GetCountByOrganizationAsync(string organizationId);
}