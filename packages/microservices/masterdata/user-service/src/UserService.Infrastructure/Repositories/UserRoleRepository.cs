using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserService.Infrastructure.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(UserServiceDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserRole>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<UserRole?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleId)
        {
            throw new NotImplementedException();
        }
    }
}