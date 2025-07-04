using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterContractRepository : IRepository<TransporterContract>
{
    Task<IEnumerable<TransporterContract>> GetContractsByTransporterIdAsync(string transporterId);
    Task<IEnumerable<TransporterContract>> GetContractsByTypeAsync(string transporterId, ContractType type);
    Task<IEnumerable<TransporterContract>> GetActiveContractsAsync(string transporterId);
    Task<IEnumerable<TransporterContract>> GetExpiringContractsAsync(string transporterId, int daysAhead = 30);
    Task<IEnumerable<TransporterContract>> GetExpiredContractsAsync(string transporterId);
    Task<TransporterContract?> GetByContractNumberAsync(string contractNumber);
    Task<IEnumerable<TransporterContract>> GetContractsByClientAsync(string clientName);
}