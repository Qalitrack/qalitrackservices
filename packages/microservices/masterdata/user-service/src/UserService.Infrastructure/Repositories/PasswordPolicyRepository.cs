using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class PasswordPolicyRepository(UserServiceDbContext context) : IPasswordPolicyRepository
    {
        private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<PasswordPolicy?> GetCurrentPolicyAsync()
        {
            // Assume only one active policy exists, or fetch the latest by UpdatedAt
            return await _context.PasswordPolicies
                .OrderByDescending(p => p.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdatePolicyAsync(PasswordPolicy? policy)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            policy.Id = policy.Id ?? Guid.NewGuid().ToString();
            policy.UpdatedAt = DateTime.UtcNow;

            var existingPolicy = await _context.PasswordPolicies
                .FirstOrDefaultAsync(p => p.Id == policy.Id);

            if (existingPolicy == null)
            {
                _context.PasswordPolicies.Add(policy);
            }
            else
            {
                _context.Entry(existingPolicy).CurrentValues.SetValues(policy);
            }

            await _context.SaveChangesAsync();
        }
    }
}