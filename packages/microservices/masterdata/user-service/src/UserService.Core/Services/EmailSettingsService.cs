using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    public class EmailSettingsService
    {
        private readonly IEmailSettingsRepository _settingsRepository;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly ILogger<EmailSettingsService> _logger;
        private const string CacheKey = "EmailSettings";

        public EmailSettingsService(
            IEmailSettingsRepository settingsRepository,
            IConfiguration configuration,
            ICacheService cacheService,
            ILogger<EmailSettingsService> logger)
        {
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmailSettings> GetSettingsAsync()
        {
            var cached = await _cacheService.GetAsync<EmailSettings>(CacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                var dbSettings = await _settingsRepository.GetCurrentSettingsAsync();
                if (dbSettings != null)
                {
                    await _cacheService.SetAsync(CacheKey, dbSettings, TimeSpan.FromHours(1));
                    return dbSettings;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve email settings from database");
            }

            // Bootstrap fallback for installs that haven't set anything via the UI yet —
            // seeds from the existing env-var/appsettings.json config so behavior doesn't
            // change until an admin actively overrides it.
            var emailSection = _configuration.GetSection("Email");
            var fallback = new EmailSettings
            {
                // Fixed id — see EmailSettings.SingletonId.
                Id = EmailSettings.SingletonId,
                SmtpHost = emailSection["SmtpHost"],
                SmtpPort = int.TryParse(emailSection["SmtpPort"], out var port) ? port : 587,
                SmtpUsername = emailSection["SmtpUsername"],
                SmtpPassword = emailSection["SmtpPassword"],
                FromEmail = emailSection["FromEmail"],
                FromName = emailSection["FromName"] ?? "QaliTrack System",
                EnableSsl = !bool.TryParse(emailSection["EnableSsl"], out var enableSsl) || enableSsl,
            };

            await _cacheService.SetAsync(CacheKey, fallback, TimeSpan.FromHours(1));
            return fallback;
        }

        public async Task<bool> IsConfiguredAsync()
        {
            var settings = await GetSettingsAsync();
            return !string.IsNullOrWhiteSpace(settings.SmtpHost)
                && !string.IsNullOrWhiteSpace(settings.SmtpUsername)
                && !string.IsNullOrWhiteSpace(settings.SmtpPassword);
        }

        // `newPassword` null/blank means "keep the currently saved password" —
        // the settings form never shows the real password back, so it can only
        // ever submit a new one or leave the field empty.
        public async Task<EmailSettings> UpdateSettingsAsync(
            string? smtpHost, int smtpPort, string? smtpUsername, string? newPassword,
            string? fromEmail, string? fromName, bool enableSsl)
        {
            // Read fresh from the DB rather than GetSettingsAsync()'s (up to
            // 1h stale) cache. Every other field below is always fully
            // overwritten by the caller regardless, but SmtpPassword is only
            // conditionally preserved from "current" — starting from a stale
            // cached password could silently revert a password another admin
            // already changed more recently than our cache entry.
            var current = await _settingsRepository.GetCurrentSettingsAsync() ?? new EmailSettings();

            current.SmtpHost = smtpHost;
            current.SmtpPort = smtpPort;
            current.SmtpUsername = smtpUsername;
            current.FromEmail = fromEmail;
            current.FromName = fromName;
            current.EnableSsl = enableSsl;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                current.SmtpPassword = newPassword;
            }

            await _settingsRepository.UpdateSettingsAsync(current);
            await _cacheService.SetAsync(CacheKey, current, TimeSpan.FromHours(1));
            return current;
        }
    }
}
