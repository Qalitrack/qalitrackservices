using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface ITransactionRepository : IRepository<WeighingTransaction>
{
    Task<WeighingTransaction?> GetWithWorkflowAsync(string id);
    Task<WeighingTransaction?> GetWithChargesAsync(string id);
    Task<WeighingTransaction?> GetWithDocumentsAsync(string id);
    Task<WeighingTransaction?> GetCompleteAsync(string id);
    Task<IEnumerable<WeighingTransaction>> GetByVehicleIdAsync(string vehicleId);
    Task<IEnumerable<WeighingTransaction>> GetByOrganizationIdAsync(string organizationId);
    Task<IEnumerable<WeighingTransaction>> GetByStatusAsync(TransactionStatus status);
    Task<IEnumerable<WeighingTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<string> GenerateTransactionNumberAsync();
}