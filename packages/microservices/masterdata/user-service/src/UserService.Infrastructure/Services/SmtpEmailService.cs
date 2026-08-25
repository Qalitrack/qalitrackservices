using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Services;

namespace UserService.Infrastructure.Services;

public class SmtpEmailService(EmailSettingsService emailSettingsService, ILogger<SmtpEmailService> logger)
    : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var settings = await emailSettingsService.GetSettingsAsync();

        var smtpHost     = settings.SmtpHost;
        var smtpPort     = settings.SmtpPort;
        var smtpUsername = settings.SmtpUsername;
        var smtpPassword = settings.SmtpPassword;
        var fromEmail    = settings.FromEmail ?? smtpUsername;
        var fromName     = settings.FromName ?? "QaliTrack";
        var enableSsl    = settings.EnableSsl;

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername))
        {
            logger.LogWarning("SMTP configuration incomplete. Email not sent to {To}", to);
            return;
        }

        // Gmail App Passwords are sometimes stored with spaces — strip them
        var password = smtpPassword?.Replace(" ", "") ?? string.Empty;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        try
        {
            var secureOption = enableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(smtpHost, smtpPort, secureOption);
            await client.AuthenticateAsync(smtpUsername, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
            throw;
        }
    }
}
