using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface ISpecificationRepository : IRepository<Specification>
{
    Task<IEnumerable<Specification>> GetByProductIdAsync(string productId);
    Task<IEnumerable<Specification>> GetByTypeAsync(SpecificationType type);
    Task<IEnumerable<Specification>> GetByCategoryAsync(string category);
    Task<IEnumerable<Specification>> GetComplianceSpecificationsAsync(string productId);
    Task<IEnumerable<Specification>> GetRequiredSpecificationsAsync(string productId);
    Task<IEnumerable<Specification>> GetExpiredCertificationsAsync();
    Task<IEnumerable<Specification>> GetTestsDueAsync();
    Task<bool> IsProductCompliantAsync(string productId);
    Task<Specification?> GetByNameAsync(string productId, string name);
}