using Abuso.Core.Entities;

namespace Abuso.Core.Interfaces;

public interface IAbusoRepository : IRepository<Abuso.Core.Entities.Abuso>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Abuso.Core.Entities.Abuso?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}