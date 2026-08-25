using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories;

public interface IEmailSettingsRepository
{
    Task<EmailSettings?> GetCurrentSettingsAsync();
    Task UpdateSettingsAsync(EmailSettings settings);
}
