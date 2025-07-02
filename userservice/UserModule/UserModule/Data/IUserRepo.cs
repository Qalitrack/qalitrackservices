namespace UserModule.Data;
using UserModule.Models;
public interface IUserRepo
{
   Task<User> AddAsync(User user);
   Task<User?> GetByIdAsync(Guid id);
   Task<User?> GetByUsernameAsync(string username);
   Task<User?> GetByEmailAsync(string email);
   Task<IEnumerable<User>> GetAllAsync();
   Task UpdateAsync(User user);
   Task DeleteAsync(Guid id);
   Task<bool> ExistsByUsernameAsync(string username);
   Task<bool> ExistsByEmailAsync(string email);
   Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
   Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department);
}