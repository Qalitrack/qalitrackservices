using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface ITechnicianRepository : IRepository<TechnicianApi.Core.Entities.Technician>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<TechnicianApi.Core.Entities.Technician?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}