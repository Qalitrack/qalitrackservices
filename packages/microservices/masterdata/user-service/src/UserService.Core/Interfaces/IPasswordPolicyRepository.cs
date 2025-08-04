using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IPasswordPolicyRepository
{
    Task<PasswordPolicy?> GetCurrentPolicyAsync();
    Task UpdatePolicyAsync(PasswordPolicy? policy);
}