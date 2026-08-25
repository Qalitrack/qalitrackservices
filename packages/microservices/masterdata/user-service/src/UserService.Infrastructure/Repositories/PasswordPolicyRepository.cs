using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class PasswordPolicyRepository(UserServiceDbContext context) : IPasswordPolicyRepository
    {
        private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<PasswordPolicy?> GetCurrentPolicyAsync()
        {
            return await _context.PasswordPolicies
                .FirstOrDefaultAsync(p => p.Id == PasswordPolicy.SingletonId);
        }

        public async Task UpdatePolicyAsync(PasswordPolicy? policy)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            policy.Id = PasswordPolicy.SingletonId;
            policy.UpdatedAt = DateTime.UtcNow;

            var existingPolicy = await _context.PasswordPolicies
                .FirstOrDefaultAsync(p => p.Id == PasswordPolicy.SingletonId);

            if (existingPolicy == null)
            {
                _context.PasswordPolicies.Add(policy);
                try
                {
                    await _context.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateException)
                {
                    // Another request may have created the row concurrently —
                    // detach our attempt and fall through to update the row
                    // that actually won the race. If that's not what
                    // happened, rethrow.
                    _context.Entry(policy).State = EntityState.Detached;
                    existingPolicy = await _context.PasswordPolicies.FirstOrDefaultAsync(p => p.Id == PasswordPolicy.SingletonId);
                    if (existingPolicy == null)
                    {
                        throw;
                    }
                }
            }

            _context.Entry(existingPolicy).CurrentValues.SetValues(policy);
            await _context.SaveChangesAsync();
        }
    }
}