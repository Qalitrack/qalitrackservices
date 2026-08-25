using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class EmailSettingsRepository(UserServiceDbContext context) : IEmailSettingsRepository
    {
        private readonly UserServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<EmailSettings?> GetCurrentSettingsAsync()
        {
            return await _context.EmailSettings
                .FirstOrDefaultAsync(s => s.Id == EmailSettings.SingletonId);
        }

        public async Task UpdateSettingsAsync(EmailSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.Id = EmailSettings.SingletonId;
            settings.UpdatedAt = DateTime.UtcNow;

            var existing = await _context.EmailSettings
                .FirstOrDefaultAsync(s => s.Id == EmailSettings.SingletonId);

            if (existing == null)
            {
                _context.EmailSettings.Add(settings);
                try
                {
                    await _context.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateException)
                {
                    // Another request may have created the row concurrently
                    // between our check and this insert — detach our attempt
                    // and fall through to update the row that actually won
                    // the race. If that's not what happened, rethrow.
                    _context.Entry(settings).State = EntityState.Detached;
                    existing = await _context.EmailSettings.FirstOrDefaultAsync(s => s.Id == EmailSettings.SingletonId);
                    if (existing == null)
                    {
                        throw;
                    }
                }
            }

            _context.Entry(existing).CurrentValues.SetValues(settings);
            await _context.SaveChangesAsync();
        }
    }
}
