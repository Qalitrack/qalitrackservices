using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using UserService.Core.Interfaces.Emails;

namespace UserService.Infrastructure.Services;

public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpHost     = configuration["Email:SmtpHost"];
        var smtpPort     = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = configuration["Email:SmtpUsername"];
        var smtpPassword = configuration["Email:SmtpPassword"];
        var fromEmail    = configuration["Email:FromEmail"] ?? smtpUsername;
        var fromName     = configuration["Email:FromName"] ?? "QaliTrack";
        var enableSsl    = bool.Parse(configuration["Email:EnableSsl"] ?? "true");

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
