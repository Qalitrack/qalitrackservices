using Microsoft.EntityFrameworkCore;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Transaction.Infrastructure.Data;

namespace Transaction.Infrastructure.Repositories;

public class TransactionSettingsRepository : ITransactionSettingsRepository
{
    private readonly TransactionDbContext _context;

    public TransactionSettingsRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionSettings?> GetCurrentSettingsAsync()
    {
        return await _context.Settings
            .FirstOrDefaultAsync(s => s.Id == TransactionSettings.SingletonId);
    }

    public async Task<TransactionSettings> SaveSettingsAsync(TransactionSettings settings)
    {
        settings.Id = TransactionSettings.SingletonId;
        settings.UpdatedAt = DateTime.UtcNow;

        var existing = await _context.Settings
            .FirstOrDefaultAsync(s => s.Id == TransactionSettings.SingletonId);

        if (existing == null)
        {
            settings.CreatedAt = DateTime.UtcNow;
            _context.Settings.Add(settings);
            try
            {
                await _context.SaveChangesAsync();
                return settings;
            }
            catch (DbUpdateException)
            {
                // Another request may have created the row concurrently
                // between our check and this insert — detach our attempt and
                // fall through to update the row that actually won the race.
                // If that's not what happened, rethrow.
                _context.Entry(settings).State = EntityState.Detached;
                existing = await _context.Settings.FirstOrDefaultAsync(s => s.Id == TransactionSettings.SingletonId);
                if (existing == null)
                {
                    throw;
                }
            }
        }

        _context.Entry(existing).CurrentValues.SetValues(settings);
        await _context.SaveChangesAsync();
        return existing;
    }
}
