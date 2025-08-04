using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IUserRoleRepository
    {
     
        Task<IEnumerable<UserRole>> GetAllAsync(CancellationToken cancellationToken = default); 
        Task<bool> AssignRoleToUserAsync(
           string userId, 
           string roleId, CancellationToken cancellationToken = default);
        Task<bool> RemoveRoleFromUserAsync(
            string userId, 
            string roleId, 
            CancellationToken cancellationToken = default);

        Task<int> RemoveRoleFromAllUsersAsync(string id);
    }
}