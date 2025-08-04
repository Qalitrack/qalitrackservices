using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure.Interfaces;

namespace UserService.Infrastructure.Services;

public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpHost = configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = configuration["Email:SmtpUsername"];
            var smtpPassword = configuration["Email:SmtpPassword"];
            var fromEmail = configuration["Email:FromEmail"];
            var fromName = configuration["Email:FromName"];
            var enableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true");
            logger.LogInformation("SMTP Config: Host={Host}, Port={Port}, Username={Username}", smtpHost, smtpPort, smtpUsername);
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername))
            {
                logger.LogWarning("SMTP configuration incomplete. Email not sent to {To}", to);
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage(new MailAddress(fromEmail ?? smtpUsername, fromName), new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
            logger.LogInformation("Email sent successfully to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
            throw;
        }
    }
}