using TestService.Core.Entities;

namespace TestService.Core.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Item?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}