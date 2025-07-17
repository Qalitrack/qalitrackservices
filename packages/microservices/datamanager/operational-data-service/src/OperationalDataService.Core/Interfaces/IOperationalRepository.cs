using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IOperationalRepository : IRepository<Operational>
{
    Task<List<Operational>> GetByOrganizationAsync(string organizationId);
    Task<List<Operational>> GetByStatusAsync(OperationStatus status);
    Task<List<Operational>> GetByTypeAsync(OperationType operationType);
    Task<List<Operational>> GetByResponsibleUserAsync(string userId);
    Task<List<Operational>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<Operational>> GetSubOperationsAsync(string parentOperationId);
    Task<List<Operational>> GetOperationsByPriorityAsync(int priority);
    Task<int> GetOperationCountByStatusAsync(OperationStatus status);
    Task<List<Operational>> GetExpiredOperationsAsync();
}