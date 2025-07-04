using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface IProductComplianceRepository : IRepository<ProductCompliance>
{
    Task<IEnumerable<ProductCompliance>> GetByProductIdAsync(string productId);
    Task<IEnumerable<ProductCompliance>> GetExpiringComplianceAsync(DateTime beforeDate);
    Task<IEnumerable<ProductCompliance>> GetByComplianceTypeAsync(string complianceType);
}