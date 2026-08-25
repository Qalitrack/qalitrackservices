using Transaction.Core.Entities;

namespace Transaction.Core.Interfaces;

public interface ITransactionSettingsRepository
{
    Task<TransactionSettings?> GetCurrentSettingsAsync();

    // Upserts the singleton settings row, retrying as an update if a
    // concurrent request already created it (see TransactionSettings.SingletonId).
    Task<TransactionSettings> SaveSettingsAsync(TransactionSettings settings);
}
