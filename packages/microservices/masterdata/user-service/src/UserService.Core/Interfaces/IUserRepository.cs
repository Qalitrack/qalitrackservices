using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserRepository : IRepository<UserService.Core.Entities.User>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<UserService.Core.Entities.User?> GetByNameAsync(string name);
    
    // TODO: Add domain-specific repository methods here
}