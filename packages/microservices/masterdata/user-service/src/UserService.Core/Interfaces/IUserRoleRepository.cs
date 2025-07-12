using UserService.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserService.Core.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetAllAsync();
        Task<UserRole?> GetByIdAsync(string id);
        Task<UserRole> CreateAsync(UserRole userRole);
        Task<UserRole?> UpdateAsync(UserRole userRole);
        Task<bool> DeleteAsync(string id);
        Task<bool> AssignRoleToUserAsync(string userId, string roleId);  // Assign role to user
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleId);  // Remove role from user
        // In IUserRepository.cs
        public interface IUserRepository
        {
            // ... other methods ...
            Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
        }
        
    }
}