using TestServiceV4.Core.Entities;

namespace TestServiceV4.Core.Interfaces;

public interface ITestentityRepository : IRepository<Testentity>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Testentity?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}