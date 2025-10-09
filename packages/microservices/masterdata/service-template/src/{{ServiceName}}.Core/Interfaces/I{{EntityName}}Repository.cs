using {{ServiceName}}.Core.Entities;

namespace {{ServiceName}}.Core.Interfaces;

public interface I{{EntityName}}Repository : IRepository<{{ServiceName}}.Core.Entities.{{EntityName}}>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<{{ServiceName}}.Core.Entities.{{EntityName}}?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}