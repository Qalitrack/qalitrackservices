using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Email;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Services;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmailSettingsController : ControllerBase
    {
        private readonly EmailSettingsService _emailSettingsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailSettingsController> _logger;

        public EmailSettingsController(
            EmailSettingsService emailSettingsService,
            IEmailService emailService,
            ILogger<EmailSettingsController> logger)
        {
            _emailSettingsService = emailSettingsService ?? throw new ArgumentNullException(nameof(emailSettingsService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<EmailSettingsDto>> GetSettings()
        {
            try
            {
                var settings = await _emailSettingsService.GetSettingsAsync();
                return Ok(new EmailSettingsDto
                {
                    SmtpHost = settings.SmtpHost,
                    SmtpPort = settings.SmtpPort,
                    SmtpUsername = settings.SmtpUsername,
                    FromEmail = settings.FromEmail,
                    FromName = settings.FromName,
                    EnableSsl = settings.EnableSsl,
                    IsPasswordSet = !string.IsNullOrWhiteSpace(settings.SmtpPassword),
                    IsConfigured = await _emailSettingsService.IsConfiguredAsync(),
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email settings");
                return StatusCode(500, "An error occurred while retrieving email settings");
            }
        }

        [HttpPut]
        public async Task<ActionResult<EmailSettingsDto>> UpdateSettings([FromBody] UpdateEmailSettingsDto dto)
        {
            try
            {
                var updated = await _emailSettingsService.UpdateSettingsAsync(
                    dto.SmtpHost, dto.SmtpPort, dto.SmtpUsername, dto.SmtpPassword,
                    dto.FromEmail, dto.FromName, dto.EnableSsl);

                return Ok(new EmailSettingsDto
                {
                    SmtpHost = updated.SmtpHost,
                    SmtpPort = updated.SmtpPort,
                    SmtpUsername = updated.SmtpUsername,
                    FromEmail = updated.FromEmail,
                    FromName = updated.FromName,
                    EnableSsl = updated.EnableSsl,
                    IsPasswordSet = !string.IsNullOrWhiteSpace(updated.SmtpPassword),
                    IsConfigured = await _emailSettingsService.IsConfiguredAsync(),
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email settings");
                return StatusCode(500, "An error occurred while updating email settings");
            }
        }

        // Sends immediately (bypasses the retry queue) so the admin gets a
        // pass/fail result right away instead of "accepted" with no feedback.
        [HttpPost("test")]
        public async Task<ActionResult> SendTestEmail([FromBody] SendTestEmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ToEmail))
            {
                return BadRequest("A recipient email address is required.");
            }

            if (!await _emailSettingsService.IsConfiguredAsync())
            {
                return BadRequest("Email settings are not fully configured yet.");
            }

            try
            {
                await _emailService.SendEmailAsync(
                    dto.ToEmail,
                    "QaliTrack test email",
                    "<p>This is a test email from QaliTrack. If you received this, your SMTP settings are working.</p>");
                return Ok(new { success = true, message = "Test email sent." });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Test email failed to send to {ToEmail}", dto.ToEmail);
                return BadRequest(new { success = false, message = $"Failed to send test email: {ex.Message}" });
            }
        }
    }
}
