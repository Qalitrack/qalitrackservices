using Masterdata.Core.Entities;

namespace Masterdata.Core.Interfaces;

public interface IBaseRepository : IRepository<Masterdata.Core.Entities.Base>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Masterdata.Core.Entities.Base?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}